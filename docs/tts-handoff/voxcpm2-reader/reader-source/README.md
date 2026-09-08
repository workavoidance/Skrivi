# Reader POC 0.2.0 · Voice Lab

**Upgrading:** quit 0.1.0 from its tray menu, extract this new ZIP, then open Reader.exe. It automatically reuses your stored model. No model download is needed.

See `VOICE_GUIDE.md` for variation, quality controls and importing a female reference voice. `VOICE_BUILD_REPORT.md` contains this version's tests; `BUILD_REPORT.md` is the historical 0.1.0 baseline.

A standalone Windows tray reader using the public OpenBMB VoxCPM2 model, converted to Q4_K GGUF by cstr and run with CrispASR v0.8.32. Separate from Skrivi.

**Status: compiled Windows test build. Local model synthesis was verified on Linux. Windows selection, hotkeys and playback have not been exercised on a Windows machine in this environment. This is a POC for Jon to test, not a school deployment release.**

## First run

1. Extract this ZIP completely into a normal folder, such as `Downloads\VoxCPM2Reader`. Do not run it from inside the ZIP.
2. Open `Reader.exe`. Windows 10/11 x64 with .NET Framework 4.8 and an AVX2-capable CPU is required. No Python, administrator access, account or separate .NET SDK is needed to run it. The new executable is unsigned.
3. Click **Import downloaded model** and choose `voxcpm2-q4_k.gguf` from yesterday's extracted model bundle. Alternatively click **Download model (1.69 GB)**. The import/download verifies the exact pinned SHA-256.
4. Wait for **Ready**. Click **Test Norwegian** for a first audio check.
5. Highlight a short sentence in Word, Chrome or Edge, then press **Ctrl+Alt+R**. Wait for generation; pressing the same hotkey stops capture, generation or playback.
6. Close the window to hide it to the orange tray icon. Use the tray menu to open it or quit.

The included `Norwegian-Test.wav` is actual output from this Q4 runtime test. Its input was: “Hei! Dette er en prøve på norsk tale.” Listen to it to assess whether this quantized default voice is good enough before investing in further integration.

## Voice and quality gap

This is the approved **public VoxCPM2 base model**, not the NbAiLab Norwegian fine-tune. Without a reference it uses the default voice. It does **not** claim to reproduce the female Oslo demo voice.

The instruction, “An adult Norwegian woman with a clear Oslo accent, speaking naturally at a normal conversational pace,” is not implemented by this runtime's VoxCPM2 adapter. Reference import exposes the adapter's supported alternative. Female Norwegian matching still requires a suitable sample and your listening test.

Default diffusion steps and CFG are retained unless you change them. Speed stays at 1.0. Playback uses Windows SoundPlayer on the complete native 48 kHz mono PCM16 WAV. The app does not trim silence, resample, speed up or split sentences. CrispASR performs its own sentence segmentation for longer requests. Non-spoken synthetic-audio marking remains enabled.

Audio is buffered until the selected passage finishes generating. This makes first audio slower than a native streaming implementation. There is a 4,096-character POC limit; long selections are rejected, not silently truncated. Start with one sentence.

## Model reuse and offline operation

The model is copied once to:

`%LOCALAPPDATA%\VoxCPM2ReaderPOC\models\voxcpm2-q4_k.gguf`

Future app versions use this same path. Each launch checks the local model's checksum, then loads it. There is no automatic model update or network request during normal reading. Replacing the app folder does not remove the model. Importing creates a separate persistent copy, so initially allow space for both the downloaded source and that copy.

The runtime is kept loaded across completed readings. Stopping an active operation terminates the app's own runtime so generation stops too; the next reading reloads it. Normal quit and process crashes close a Windows Job Object that owns this runtime. No Skrivi processes, settings, caches or runtime state are accessed.

The speech API binds only to `127.0.0.1` on a temporary port with a per-launch random API key. Proxy use and redirects are disabled for speech requests. No cloud TTS, account or telemetry is used. Runtime stdout/stderr are drained and discarded because upstream diagnostics can include input text. Generated audio stays in memory. Only numeric timing measurements are written to `measurements.txt` in the POC folder under LocalAppData.

Clipboard fallback necessarily interacts with Windows' clipboard. Windows clipboard history/sync and the source app's behavior remain OS settings outside this POC. Direct UI Automation capture does not alter the clipboard.

## Selection and clipboard behavior

Reader first tries UI Automation TextPattern selection in an isolated helper process with a 1.5-second timeout. It refuses detected password fields. It never falls back to reading the entire control or cursor position.

If selection is unavailable, it waits for the hotkey keys to be released and sends Ctrl+C to the still-focused app. It snapshots all safely duplicable clipboard formats in memory first, including rich text and HTML. It restores them under the clipboard lock only when the observed copied contents are still current and owned by the target app. Newer clipboard changes are preserved.

For opaque image/GDI, owner-display or otherwise unsupported clipboard formats, or snapshots over 64 MiB, it refuses clipboard fallback before sending Ctrl+C. This conservative POC does not claim universal clipboard preservation. Clipboard lock failures and ambiguous ownership produce visible errors. Normal Quit waits for a copy already sent to complete its cleanup. Forced termination or Windows shutdown can interrupt cleanup.

Reader runs without elevation. Copying from an elevated app may be blocked by Windows. If another app owns Ctrl+Alt+R, Reader reports the conflict; hotkey customization is not included in v1.

## Measurements and remaining tests

See `BUILD_REPORT.md` for the measured size/performance data and exact validation scope. The app displays time to the playback call, generation time, generated audio duration, audio seconds per generation second and runtime peak working-set RAM after each successful request. This is not a hardware measurement of when sound physically reaches the speaker.

On Windows, test:

- Word, Chrome and Edge: highlight a Norwegian sentence, read, verify every word and its ending.
- Stop during capture, generation and playback; read again afterwards.
- Copy formatted text first, invoke fallback from an app without UIA selection, and paste afterwards to verify content and formatting. Change the clipboard during capture and check that newer content is retained.
- Try an image clipboard; unsupported snapshots should leave it untouched and report the fallback limitation.
- Quit during capture; no stuck Ctrl key, stray runtime or unexpected clipboard change.
- Restart without internet; run an updated copy of the app and confirm no model redownload.
- Compare the voice and cadence with the online demo. This comparison has not been passed yet.

## Development and later integration

`source/src/Reader.Core` holds platform-independent model identity, input validation, WAV validation and speech/playback interfaces. `source/src/Reader.Windows` holds the replaceable WinForms tray shell, Windows selection adapter, native clipboard handling, local-runtime adapter and playback adapter. The core can be retargeted or brought into Skrivi independently.

Install a .NET 8 SDK on a developer machine, then run `source/BUILD.ps1`. Run `source/TEST.ps1` for the core checks. The SDK is a build dependency only. The bundled runtime executable and DLL are unmodified upstream files; the quantizer is omitted because the app does not use it.

## Removal

Quit Reader and delete its extracted app folder. The model is intentionally retained. To remove its model and measurements too, delete `%LOCALAPPDATA%\VoxCPM2ReaderPOC`. No installer, startup registration, service or Skrivi changes are created.

## Licenses and sources

- Reader shell/core: MIT, included `LICENSE`.
- CrispASR runtime: MIT, included `runtime/LICENSE`, plus its bundled `runtime/THIRD_PARTY_NOTICES.txt`. OpenBLAS is BSD-3-Clause. Keep these notices with redistribution.
- VoxCPM2 model and the cstr quantization: Apache-2.0, included `licenses/VoxCPM2-APACHE-2.0.txt` and model card. The small app ZIP contains the model manifest, not the model weights.

Primary sources:

- https://huggingface.co/openbmb/VoxCPM2
- https://huggingface.co/cstr/voxcpm2-GGUF
- https://github.com/CrispStrobe/CrispASR/releases/tag/v0.8.32
- https://github.com/CrispStrobe/CrispASR/blob/5baf533c9f5038226b4af1cd6bde3454d6bf9316/examples/cli/crispasr_backend_voxcpm2_tts.cpp
