using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
namespace Reader.Windows
{
    internal static class Selection
    {
        [DllImport("user32.dll")] internal static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
        [DllImport("user32.dll")] static extern short GetAsyncKeyState(int key);
        [DllImport("user32.dll", SetLastError = true)] static extern bool OpenClipboard(IntPtr owner);
        [DllImport("user32.dll")] static extern bool CloseClipboard();
        [DllImport("user32.dll")] static extern uint EnumClipboardFormats(uint format);
        [DllImport("user32.dll")] static extern IntPtr GetClipboardData(uint format);
        [DllImport("user32.dll")] static extern IntPtr SetClipboardData(uint format, IntPtr data);
        [DllImport("user32.dll")] static extern bool EmptyClipboard();
        [DllImport("user32.dll")] static extern uint GetClipboardSequenceNumber();
        [DllImport("user32.dll")] static extern IntPtr GetClipboardOwner();
        [DllImport("kernel32.dll")] static extern UIntPtr GlobalSize(IntPtr h);
        [DllImport("kernel32.dll")] static extern IntPtr GlobalLock(IntPtr h);
        [DllImport("kernel32.dll")] static extern bool GlobalUnlock(IntPtr h);
        [DllImport("kernel32.dll")] static extern IntPtr GlobalAlloc(uint flags, UIntPtr bytes);
        [DllImport("kernel32.dll")] static extern IntPtr GlobalFree(IntPtr h);
        [StructLayout(LayoutKind.Sequential)] struct Key { public ushort Vk, Scan; public uint Flags, Time; public UIntPtr Extra; }
        [StructLayout(LayoutKind.Explicit, Size = 40)] struct Input { [FieldOffset(0)] public uint Type; [FieldOffset(8)] public Key Key; }
        [DllImport("user32.dll", SetLastError = true)] static extern uint SendInput(uint n, Input[] inputs, int size);

        // UI Automation providers can hang. Run them in an isolated, killable process.
        public static int UiaHelper(long expected)
        {
            Console.OutputEncoding = new UTF8Encoding(false);
            try
            {
                if (GetForegroundWindow().ToInt64() != expected) return 2;
                var element = AutomationElement.FocusedElement;
                for (int i = 0; element != null && i < 8; i++)
                {
                    if (element.Current.IsPassword) return 3;
                    object value;
                    if (element.TryGetCurrentPattern(TextPattern.Pattern, out value))
                    {
                        var parts = new List<string>();
                        foreach (var range in ((TextPattern)value).GetSelection())
                        {
                            string text = range.GetText(4097);
                            if (!string.IsNullOrEmpty(text)) parts.Add(text);
                        }
                        string selection = string.Join("\n", parts);
                        if (!string.IsNullOrWhiteSpace(selection))
                        { Console.Write(selection); return 0; }
                    }
                    element = TreeWalker.ControlViewWalker.GetParent(element);
                }
            }
            catch { return 2; }
            return 2;
        }
        public static async Task<string> CaptureAsync(IntPtr requester, CancellationToken token)
        {
            IntPtr target = GetForegroundWindow();
            GetWindowThreadProcessId(target, out uint targetPid);
            if (targetPid == Process.GetCurrentProcess().Id) throw new InvalidOperationException("Highlight text in another app, then press Ctrl+Alt+R.");
            var psi = new ProcessStartInfo(System.Windows.Forms.Application.ExecutablePath, "--capture-uia " + target.ToInt64())
            { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, StandardOutputEncoding = Encoding.UTF8 };
            using (var helper = Process.Start(psi))
            {
                var output = helper.StandardOutput.ReadToEndAsync();
                try
                {
                    var watch = Stopwatch.StartNew();
                    while (!helper.HasExited && watch.ElapsedMilliseconds < 1500) await Task.Delay(25, token);
                    if (helper.HasExited)
                    {
                        if (helper.ExitCode == 3) throw new InvalidOperationException("Password fields are not read.");
                        if (helper.ExitCode == 0) { string text = await output; if (!string.IsNullOrWhiteSpace(text)) return text; }
                    }
                }
                finally { try { if (!helper.HasExited) helper.Kill(); } catch { } }
            }
            token.ThrowIfCancellationRequested();
            // Wait for the physical hotkey keys to be released before sending Ctrl+C.
            var release = Stopwatch.StartNew();
            while ((GetAsyncKeyState(0x11) & 0x8000) != 0 || (GetAsyncKeyState(0x12) & 0x8000) != 0 || (GetAsyncKeyState(0x52) & 0x8000) != 0)
            {
                if (release.ElapsedMilliseconds > 2000) throw new InvalidOperationException("Release the hotkey, then try again.");
                await Task.Delay(25, token);
            }
            if (GetForegroundWindow() != target) throw new InvalidOperationException("The focused app changed. Select the text and try again.");
            await Acquire(requester, token);
            Snapshot snapshot;
            uint before;
            try { snapshot = new Snapshot(); before = GetClipboardSequenceNumber(); }
            finally { CloseClipboard(); }
            using (snapshot)
            {
                if (GetForegroundWindow() != target || GetClipboardSequenceNumber() != before)
                    throw new InvalidOperationException("The selection or clipboard changed. Try again.");
                token.ThrowIfCancellationRequested();
                var keys = new[] { MakeKey(0x11, false), MakeKey(0x43, false), MakeKey(0x43, true), MakeKey(0x11, true) };
                if (SendInput(4, keys, Marshal.SizeOf(typeof(Input))) != 4)
                {
                    SendInput(2, new[] { MakeKey(0x43, true), MakeKey(0x11, true) }, Marshal.SizeOf(typeof(Input)));
                    throw new InvalidOperationException("Windows blocked selection capture. Reader cannot copy from an app running as administrator.");
                }
                // Cleanup continues even after Stop: finish restoring a copy already in flight.
                var wait = Stopwatch.StartNew();
                while (GetClipboardSequenceNumber() == before && wait.ElapsedMilliseconds < 1200) await Task.Delay(20);
                if (GetClipboardSequenceNumber() == before) throw new InvalidOperationException("The app did not provide selected text. Try another selection.");
                uint observed = GetClipboardSequenceNumber();
                await Acquire(requester, CancellationToken.None);
                string text = null;
                try
                {
                    uint captured = GetClipboardSequenceNumber();
                    if (captured != observed) throw new InvalidOperationException("The clipboard changed again. Its newer contents were kept.");
                    GetWindowThreadProcessId(GetClipboardOwner(), out uint ownerPid);
                    if (ownerPid != targetPid) throw new InvalidOperationException("Another app changed the clipboard. Its newer contents were kept.");
                    IntPtr data = GetClipboardData(13);
                    if (data != IntPtr.Zero)
                    {
                        ulong count = GlobalSize(data).ToUInt64();
                        if (count > 0 && count <= 16 * 1024 * 1024)
                        {
                            IntPtr ptr = GlobalLock(data);
                            if (ptr != IntPtr.Zero)
                            {
                                try { text = Marshal.PtrToStringUni(ptr, (int)count / 2).TrimEnd('\0'); }
                                finally { GlobalUnlock(data); }
                            }
                        }
                    }
                    // Sequence check and restore happen under the same clipboard lock.
                    if (GetClipboardSequenceNumber() != captured)
                        throw new InvalidOperationException("The clipboard changed during capture; newer contents were kept.");
                    snapshot.Restore();
                }
                finally { CloseClipboard(); }
                token.ThrowIfCancellationRequested();
                return text;
            }
        }
        static Input MakeKey(ushort key, bool up) { return new Input { Type = 1, Key = new Key { Vk = key, Flags = up ? 2u : 0u } }; }
        static async Task Acquire(IntPtr owner, CancellationToken token)
        {
            for (int i = 0; i < 25; i++) { token.ThrowIfCancellationRequested(); if (OpenClipboard(owner)) return; await Task.Delay(20, token); }
            throw new InvalidOperationException("The clipboard is busy. Try again.");
        }
        sealed class Snapshot : IDisposable
        {
            readonly Dictionary<uint, IntPtr> copies = new Dictionary<uint, IntPtr>();
            public Snapshot()
            {
                try
                {
                    ulong total = 0; uint format = 0;
                    while ((format = EnumClipboardFormats(format)) != 0)
                    {
                        // Opaque GDI/owner-display handles cannot safely be restored as bytes.
                        if (format == 2 || format == 3 || format == 9 || format == 14 || (format >= 0x80 && format < 0xC000))
                            throw new InvalidOperationException("This clipboard contains an image or special format that Reader cannot safely restore. Clipboard fallback was skipped.");
                        IntPtr source = GetClipboardData(format); ulong size = GlobalSize(source).ToUInt64(); total += size;
                        if (source == IntPtr.Zero || size == 0 || total > 64 * 1024 * 1024)
                            throw new InvalidOperationException("Reader cannot safely preserve this clipboard content. Clipboard fallback was skipped.");
                        IntPtr copy = GlobalAlloc(2, (UIntPtr)size);
                        if (copy == IntPtr.Zero) throw new OutOfMemoryException();
                        copies.Add(format, copy);
                        IntPtr src = GlobalLock(source), dst = GlobalLock(copy);
                        if (src == IntPtr.Zero || dst == IntPtr.Zero)
                        { if (src != IntPtr.Zero) GlobalUnlock(source); if (dst != IntPtr.Zero) GlobalUnlock(copy); throw new InvalidOperationException("Clipboard backup failed; copying was skipped."); }
                        try { byte[] bytes = new byte[(int)size]; Marshal.Copy(src, bytes, 0, bytes.Length); Marshal.Copy(bytes, 0, dst, bytes.Length); }
                        finally { GlobalUnlock(source); GlobalUnlock(copy); }
                    }
                }
                catch { Dispose(); throw; }
            }
            public void Restore()
            {
                if (!EmptyClipboard()) throw new InvalidOperationException("Windows refused to restore the previous clipboard.");
                foreach (uint format in new List<uint>(copies.Keys))
                {
                    if (SetClipboardData(format, copies[format]) == IntPtr.Zero) throw new InvalidOperationException("Windows could not fully restore the previous clipboard.");
                    copies[format] = IntPtr.Zero; // Windows now owns this handle.
                }
            }
            public void Dispose() { foreach (IntPtr copy in copies.Values) if (copy != IntPtr.Zero) GlobalFree(copy); copies.Clear(); }
        }
    }
}
