using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Reader.Core;

namespace Reader.Windows
{
    internal static class Program
    {
        [STAThread] static int Main(string[] args)
        {
            if (args.Length == 2 && args[0] == "--capture-uia") return Selection.UiaHelper(long.Parse(args[1]));
            using (var mutex = new Mutex(true, @"Local\VoxCPM2ReaderPOC", out bool first))
            {
                if (!first) { MessageBox.Show("Reader is already running. Look for its tray icon.", "Reader POC"); return 0; }
                Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ReaderForm()); return 0;
            }
        }
    }
    internal sealed class ReaderForm : Form
    {
        [DllImport("user32.dll", SetLastError = true)] static extern bool RegisterHotKey(IntPtr h, int id, uint modifiers, uint key);
        [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr h, int id);
        [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr h);
        readonly string dataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VoxCPM2ReaderPOC");
        string ModelPath { get { return Path.Combine(dataRoot, "models", ModelSpec.FileName); } }
        readonly Label status = new Label { AutoSize = false, Height = 48, Dock = DockStyle.Top, Text = "Starting..." };
        readonly Label metrics = new Label { AutoSize = false, Height = 65, Dock = DockStyle.Top };
        readonly Button import = new Button { Text = "Import downloaded model", AutoSize = true };
        readonly Button download = new Button { Text = "Download model (1.69 GB)", AutoSize = true };
        readonly Button test = new Button { Text = "Test Norwegian", AutoSize = true };
        readonly Button stop = new Button { Text = "Stop", AutoSize = true };
        readonly WindowsAudio audio = new WindowsAudio();
        readonly FlowLayoutPanel voicePanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        readonly NumericUpDown seed = new NumericUpDown { Minimum = 1, Maximum = 999999, Value = 42, Width = 100 };
        readonly NumericUpDown steps = new NumericUpDown { Minimum = 4, Maximum = 20, Value = 10, Width = 65 };
        readonly NumericUpDown guidance = new NumericUpDown { Minimum = 1, Maximum = 4, DecimalPlaces = 1, Increment = 0.1m, Value = 2, Width = 65 };
        readonly Label voiceLabel = new Label { AutoSize = true, MaximumSize = new Size(530, 0) };
        VoiceSettings voice;
        string SettingsPath { get { return Path.Combine(dataRoot, "voice-settings.json"); } }
        NotifyIcon tray;
        VoxEngine engine;
        CancellationTokenSource active;
        bool exiting, hotkey, quitting;
        Icon ownedIcon;
        public ReaderForm()
        {
            Text = "Reader POC 0.2 · Voice Lab"; ClientSize = new Size(610, 640);
            voice = VoiceSettings.Load(SettingsPath);
            Font = new Font("Segoe UI", 10); BackColor = Color.FromArgb(250, 249, 245);
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; StartPosition = FormStartPosition.CenterScreen;
            var layout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(20), AutoScroll = true };
            Controls.Add(layout);
            layout.Controls.Add(new Label { Text = "Highlight text. Press Ctrl+Alt+R.\nPress it again to stop.", Font = new Font(Font, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 0, 0, 14) });
            layout.Controls.Add(new Label { Text = "AI-generated speech · local VoxCPM2 Q4 · Norwegian first\nFemale voice: import a female reference WAV. No built-in female preset.", AutoSize = true, Margin = new Padding(0, 0, 0, 14) });
            seed.Value = voice.Seed; steps.Value = voice.Steps; guidance.Value = voice.Guidance;
            var voiceButtons = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            var chooseVoice = new Button { Text = "Import reference voice WAV", AutoSize = true };
            chooseVoice.Click += (s, e) => ImportVoice(); voiceButtons.Controls.Add(chooseVoice);
            var defaultVoice = new Button { Text = "Use default voice", AutoSize = true };
            defaultVoice.Click += (s, e) => { voice.Reference = ""; voice.Consent = ""; SaveVoice(); }; voiceButtons.Controls.Add(defaultVoice);
            voicePanel.Controls.Add(voiceLabel); voicePanel.Controls.Add(voiceButtons);
            var knobs = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            knobs.Controls.Add(new Label { Text = "Variation seed", AutoSize = true }); knobs.Controls.Add(seed);
            var random = new Button { Text = "Try another", AutoSize = true };
            random.Click += (s, e) => seed.Value = new Random().Next(1, 1000000); knobs.Controls.Add(random);
            voicePanel.Controls.Add(knobs);
            var quality = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            quality.Controls.Add(new Label { Text = "Steps", AutoSize = true }); quality.Controls.Add(steps);
            quality.Controls.Add(new Label { Text = "Guidance", AutoSize = true }); quality.Controls.Add(guidance);
            var reset = new Button { Text = "Reset tuning", AutoSize = true };
            reset.Click += (s, e) => { seed.Value = 42; steps.Value = 10; guidance.Value = 2; }; quality.Controls.Add(reset);
            voicePanel.Controls.Add(quality);
            voicePanel.Controls.Add(new Label { Text = "Seed explores variations, not gender. Steps: 10 default; lower may lose quality.\nGuidance: 2.0 default; changes may affect pronunciation and expression.\nChanging steps/guidance reloads the engine on your next reading.\nPlayback speed and pitch stay unchanged.", AutoSize = true });
            layout.Controls.Add(voicePanel);
            seed.ValueChanged += (s, e) => SaveVoice(); steps.ValueChanged += (s, e) => SaveVoice(); guidance.ValueChanged += (s, e) => SaveVoice();
            UpdateVoiceLabel();
            var buttons = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            buttons.Controls.Add(import); buttons.Controls.Add(download); layout.Controls.Add(buttons);
            var actions = new FlowLayoutPanel { AutoSize = true };
            actions.Controls.Add(test); actions.Controls.Add(stop);
            var hide = new Button { Text = "Hide to tray", AutoSize = true }; hide.Click += (s, e) => Hide(); actions.Controls.Add(hide);
            var quit = new Button { Text = "Quit", AutoSize = true }; quit.Click += (s, e) => Quit(); actions.Controls.Add(quit); layout.Controls.Add(actions);
            status.Width = 535; metrics.Width = 535; layout.Controls.Add(status); layout.Controls.Add(metrics);
            var folder = new LinkLabel { Text = "Open model and measurements folder", AutoSize = true };
            folder.LinkClicked += (s, e) => { Directory.CreateDirectory(dataRoot); Process.Start("explorer.exe", "\"" + dataRoot + "\""); }; layout.Controls.Add(folder);
            import.Click += async (s, e) => await ImportAsync();
            download.Click += async (s, e) => await SetupAsync(null);
            test.Click += async (s, e) => await ReadAsync("Hei! Dette er en prøve på norsk tale.");
            stop.Click += (s, e) => Stop();
            using (var bitmap = new Bitmap(32, 32))
            {
                using (var g = Graphics.FromImage(bitmap)) { g.Clear(Color.Transparent); using (var brush = new SolidBrush(Color.FromArgb(231, 108, 42))) g.FillEllipse(brush, 3, 3, 26, 26); }
                IntPtr icon = bitmap.GetHicon(); ownedIcon = (Icon)Icon.FromHandle(icon).Clone(); DestroyIcon(icon);
            }
            Icon = ownedIcon;
            var menu = new ContextMenuStrip();
            menu.Items.Add("Open Reader", null, (s, e) => { Show(); Activate(); });
            menu.Items.Add("Stop", null, (s, e) => Stop());
            menu.Items.Add("Quit", null, (s, e) => Quit());
            tray = new NotifyIcon { Icon = ownedIcon, Text = "Reader POC · Ctrl+Alt+R", ContextMenuStrip = menu, Visible = true };
            tray.DoubleClick += (s, e) => { Show(); Activate(); };
            Shown += async (s, e) =>
            {
                hotkey = RegisterHotKey(Handle, 1, 0x4000 | 0x0001 | 0x0002, 0x52);
                if (!hotkey) MessageBox.Show(this, "Ctrl+Alt+R is already used by another app. Close that app and restart Reader. The Norwegian test button still works.", "Hotkey unavailable");
                if (File.Exists(ModelPath)) await InitializeAsync();
                else SetStatus("Import yesterday's extracted GGUF file, or download it once.");
            };
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == 1)
            { if (active != null) Stop(); else _ = ReadAsync(null); }
            base.WndProc(ref m);
        }
        void SetStatus(string text)
        {
            if (exiting || IsDisposed) return;
            if (InvokeRequired) { try { BeginInvoke(new Action<string>(SetStatus), text); } catch { } return; }
            status.Text = text;
        }
        void Busy(bool value) { voicePanel.Enabled = import.Enabled = download.Enabled = test.Enabled = !value; stop.Enabled = value; }
        void UpdateVoiceLabel() { voiceLabel.Text = string.IsNullOrEmpty(voice.Reference) ? "Voice: model default (no reference)" : "Voice: imported reference, stored locally"; }
        VoiceSettings CurrentVoice()
        {
            return new VoiceSettings { Seed = (int)seed.Value, Steps = (int)steps.Value, Guidance = guidance.Value, Reference = voice.Reference, Consent = voice.Consent };
        }
        void SaveVoice()
        {
            voice = CurrentVoice(); UpdateVoiceLabel();
            try { voice.Save(SettingsPath); } catch (Exception e) { Report(e); }
        }
        void ImportVoice()
        {
            using (var dialog = new OpenFileDialog { Title = "Choose a clean 2–20 second female or other reference", Filter = "Mono PCM16 WAV|*.wav" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    if (new FileInfo(dialog.FileName).Length > 2000000) throw new InvalidDataException("Use a 2–20 second mono PCM16 WAV, 16–48 kHz, under 2 MB.");
                    byte[] bytes = File.ReadAllBytes(dialog.FileName); WaveInfo.ParseReference(bytes);
                    if (MessageBox.Show(this, "Use only your own voice, a speaker who explicitly permits voice cloning, or a synthetic voice you have permission to reuse.\n\nConfirm that you have this permission. Reader labels playback as AI-generated; you must identify any output you share as AI-generated too.\n\nThe reference will be copied locally and never uploaded. Continue?", "Reference voice permission", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                    string folder = Path.Combine(dataRoot, "voices"); Directory.CreateDirectory(folder);
                    string name = "voice-" + Guid.NewGuid().ToString("N") + ".wav";
                    File.WriteAllBytes(Path.Combine(folder, name), bytes);
                    voice.Reference = name; voice.Consent = "The user explicitly confirmed ownership, speaker consent for cloning, or permission to reuse this synthetic reference.";
                    SaveVoice(); SetStatus("Reference imported. Test Norwegian to hear it. Exact voice matching is not guaranteed.");
                }
                catch (Exception e) { Report(e); }
            }
        }
        void Report(Exception error)
        {
            if (exiting) return;
            string message = error is OperationCanceledException ? "Stopped." : error.Message;
            SetStatus(message);
            if (!(error is OperationCanceledException)) tray.ShowBalloonTip(5000, "Reader POC", message, ToolTipIcon.Warning);
        }
        void Stop()
        {
            if (active == null) return;
            active.Cancel(); audio.Stop(); engine?.Stop(); SetStatus("Stopping...");
        }
        async Task InitializeAsync()
        {
            if (active != null) return;
            var cts = active = new CancellationTokenSource(); Busy(true);
            try
            {
                SetStatus("Verifying the local model..."); await VerifyAsync(ModelPath, cts.Token);
                engine?.Dispose(); engine = new VoxEngine(ModelPath); engine.Status += SetStatus;
                engine.Configure(CurrentVoice());
                await engine.WarmAsync(cts.Token);
                SetStatus("Ready. Highlight text in another app and press Ctrl+Alt+R.");
                metrics.Text = "Model load: " + engine.LastLoadSeconds.ToString("F1") + " s. Model storage is reused between builds.";
            }
            catch (Exception e) { engine?.Stop(); Report(e); }
            finally { if (active == cts) active = null; cts.Dispose(); if (!exiting) Busy(false); }
        }
        async Task ReadAsync(string sample)
        {
            if (active != null) { Stop(); return; }
            if (engine == null) { SetStatus("Import or download the model first."); Show(); return; }
            var cts = active = new CancellationTokenSource(); Busy(true); var watch = Stopwatch.StartNew();
            try
            {
                SetStatus("Getting selected text...");
                string text = sample ?? await Selection.CaptureAsync(Handle, cts.Token);
                text = ReadingText.Validate(text); cts.Token.ThrowIfCancellationRequested();
                engine.Configure(CurrentVoice());
                byte[] wav = await engine.SynthesizeAsync(text, cts.Token);
                var info = WaveInfo.Parse(wav); double firstAudio = watch.Elapsed.TotalSeconds;
                SetStatus("Reading... Ctrl+Alt+R stops");
                var playing = audio.PlayAsync(wav, cts.Token);
                metrics.Text = string.Format("First audio: {0:F1} s · generation: {1:F1} s · audio: {2:F1} s\nSpeed: {3:F2} audio seconds/s · runtime peak RAM: {4:F0} MB", firstAudio, engine.LastGenerationSeconds, info.Seconds, info.Seconds / engine.LastGenerationSeconds, engine.PeakRamBytes / 1000000.0);
                SaveMeasurement(info, firstAudio);
                await playing;
                SetStatus("Ready. Highlight text and press Ctrl+Alt+R.");
            }
            catch (Exception e) { audio.Stop(); Report(cts.IsCancellationRequested ? new OperationCanceledException() : e); }
            finally { if (active == cts) active = null; cts.Dispose(); if (!exiting) Busy(false); }
        }
        void SaveMeasurement(WaveInfo info, double first)
        {
            try
            {
                Directory.CreateDirectory(dataRoot);
                // Numeric timings only. No selected text, audio, paths from other apps or runtime output.
                string line = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:o},load_s={1:F3},first_audio_s={2:F3},generation_s={3:F3},audio_s={4:F3},runtime_peak_bytes={5}\n", DateTime.UtcNow, engine.LastLoadSeconds, first, engine.LastGenerationSeconds, info.Seconds, engine.PeakRamBytes);
                string path = Path.Combine(dataRoot, "measurements.txt");
                if (File.Exists(path) && new FileInfo(path).Length > 100000) File.WriteAllText(path, "");
                File.AppendAllText(path, line);
            }
            catch { /* Measurements must not interrupt speech. */ }
        }
        async Task ImportAsync()
        {
            using (var dialog = new OpenFileDialog { Title = "Choose the extracted VoxCPM2 Q4 model", Filter = "VoxCPM2 GGUF|*.gguf", FileName = ModelSpec.FileName })
                if (dialog.ShowDialog(this) == DialogResult.OK) await SetupAsync(dialog.FileName);
        }
        async Task SetupAsync(string source)
        {
            if (active != null) return;
            var cts = active = new CancellationTokenSource(); Busy(true);
            string part = ModelPath + "." + Guid.NewGuid().ToString("N") + ".part";
            bool success = false;
            try
            {
                engine?.Dispose(); engine = null;
                Directory.CreateDirectory(Path.GetDirectoryName(ModelPath));
                if (source != null && string.Equals(Path.GetFullPath(source), Path.GetFullPath(ModelPath), StringComparison.OrdinalIgnoreCase))
                    await VerifyAsync(ModelPath, cts.Token);
                else
                {
                    if (source != null)
                    {
                        SetStatus("Copying model to its permanent folder...");
                        using (var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (var output = new FileStream(part, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                            await input.CopyToAsync(output, 1024 * 1024, cts.Token);
                    }
                    else
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        using (var client = new WebClient())
                        using (cts.Token.Register(client.CancelAsync))
                        {
                            client.DownloadProgressChanged += (s, e) => SetStatus("Downloading model: " + (e.BytesReceived / 1000000) + " / 1689 MB");
                            SetStatus("Starting model download...");
                            await client.DownloadFileTaskAsync(new Uri(ModelSpec.Url), part);
                        }
                    }
                    SetStatus("Verifying the model checksum..."); await VerifyAsync(part, cts.Token);
                    cts.Token.ThrowIfCancellationRequested();
                    if (File.Exists(ModelPath)) File.Replace(part, ModelPath, null); else File.Move(part, ModelPath);
                }
                success = true;
            }
            catch (Exception e) { Report(cts.IsCancellationRequested ? new OperationCanceledException() : e); }
            finally
            {
                try { if (File.Exists(part)) File.Delete(part); } catch { }
                if (active == cts) active = null; cts.Dispose(); if (!exiting) Busy(false);
            }
            if (success && !exiting) await InitializeAsync();
        }
        static Task VerifyAsync(string path, CancellationToken token)
        {
            return Task.Run(() =>
            {
                if (new FileInfo(path).Length != ModelSpec.Bytes) throw new InvalidDataException("This is not the expected VoxCPM2 Q4 model, or the download is incomplete.");
                using (var file = File.OpenRead(path))
                using (var hash = SHA256.Create())
                {
                    var buffer = new byte[1024 * 1024]; int n;
                    while ((n = file.Read(buffer, 0, buffer.Length)) > 0) { token.ThrowIfCancellationRequested(); hash.TransformBlock(buffer, 0, n, buffer, 0); }
                    hash.TransformFinalBlock(new byte[0], 0, 0);
                    string value = BitConverter.ToString(hash.Hash).Replace("-", "").ToLowerInvariant();
                    if (value != ModelSpec.Sha256) throw new InvalidDataException("Model checksum failed. Import the verified model or download it again.");
                }
            }, token);
        }
        async void Quit()
        {
            if (quitting) return; quitting = true; Stop();
            // A Ctrl+C already sent must finish clipboard restoration before normal exit.
            while (active != null) await Task.Delay(25);
            exiting = true; Close();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!exiting && e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; Hide(); return; }
            exiting = true; active?.Cancel(); audio.Dispose(); engine?.Dispose();
            if (hotkey) UnregisterHotKey(Handle, 1); tray.Visible = false; tray.Dispose(); ownedIcon.Dispose();
            base.OnFormClosing(e);
        }
    }
}
