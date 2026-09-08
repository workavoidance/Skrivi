using System;
using System.IO;
using Reader.Core;
class Tests
{
    static int checks;
    static void Assert(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }
    static void Reject(Action action, string name) { bool rejected = false; try { action(); } catch (InvalidDataException) { rejected = true; } catch (InvalidOperationException) { rejected = true; } Assert(rejected, name); }
    static int Main(string[] args)
    {
        Assert(ReadingText.Validate("  Hei, æøå! \n") == "Hei, æøå!", "Norwegian Unicode preserved");
        Reject(() => ReadingText.Validate(" \r\n "), "Blank selection");
        Reject(() => ReadingText.Validate(new string('x',4097)), "No silent truncation");
        Reject(() => ReadingText.Validate("hello\0world"), "Null rejection");
        byte[] wav = File.ReadAllBytes(args[0]);
        var info = WaveInfo.Parse(wav); Assert(info.SampleRate == 48000 && info.Seconds > 0, "Actual native runtime WAV");
        Reject(() => WaveInfo.Parse(new byte[44]), "Invalid header");
        var truncated = new byte[wav.Length-1]; Array.Copy(wav,truncated,truncated.Length);
        Reject(() => WaveInfo.Parse(truncated), "Truncated runtime WAV");
        var changed=(byte[])wav.Clone();
        for(int i=12;i+8<changed.Length;)
        {
            int size=BitConverter.ToInt32(changed,i+4);
            if(System.Text.Encoding.ASCII.GetString(changed,i,4)=="fmt ") { Array.Copy(BitConverter.GetBytes(24000),0,changed,i+12,4);break; }
            i+=8+size+(size&1);
        }
        Reject(() => WaveInfo.Parse(changed), "Reject unexpected resampling");
        Assert(WaveInfo.ParseReference(wav).Seconds > 2, "Native sample accepted as reference");
        Assert(WaveInfo.ParseReference(changed).SampleRate == 24000, "24 kHz reference accepted without changing output parser");
        Reject(() => WaveInfo.ParseReference(truncated), "Truncated reference rejected");
        Console.WriteLine(checks+" core checks passed, including actual VoxCPM2 WAV.");return 0;
    }
}
