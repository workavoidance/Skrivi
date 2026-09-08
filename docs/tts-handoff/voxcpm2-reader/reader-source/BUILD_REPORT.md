# Reader POC 0.1.0 build report

Built 2026-09-08. This report separates measurements from untested Windows behavior.

## Deliverable

A compiled Windows x64 GUI executable (`Reader.exe`), a small reusable core assembly, the unmodified CrispASR v0.8.32 Windows CPU runtime and OpenBLAS DLL, source code, licenses, test audio and instructions. Model weights are imported from the previously downloaded bundle or downloaded during first setup; they are not duplicated in this ZIP.

The shell targets .NET Framework 4.8. It was compiled on Linux using Microsoft's Roslyn C# compiler 4.11 and .NET Framework 4.8 reference assemblies. The PE headers identify the Reader as an x86-64 Windows GUI executable. No Windows host, Word, Chrome/Edge Windows sessions or Windows audio device were available for end-to-end validation.

## Download and installed size

- Exact model download: **1,689,498,432 bytes** (1.689 GB decimal; 1.573 GiB).
- Upstream Windows CPU runtime ZIP downloaded for the build: **8,261,759 bytes**. Its release SHA-256 passed.
- Included runtime files after extraction, excluding the unused quantizer: **19,269,987 bytes**.
- The compiled Reader and core are approximately 52 KB combined, plus a small config file.
- The POC package additionally contains documentation, source and a 311 KB test WAV. The final ZIP size is reported with the deliverable.
- The installed model plus application is approximately **1.71 GB**. Keeping the previous download bundle/extracted model creates additional copies on disk. Model import copies to LocalAppData; it does not delete the source.
- A new app version needs only its small app package when the existing model remains in the persistent folder.

## Measured synthesis performance

Hardware visible to the Linux environment: AMD EPYC 9V74 80-Core Processor. The process used **4 CPU threads**, no GPU. This is a shared execution environment, not Jon's laptop; do not treat these as Windows laptop benchmark figures.

Input: `Hei! Dette er en prøve på norsk tale.`

| Metric | Measured result |
| --- | ---: |
| Server startup to healthy, including model load | 1.005 s |
| Request to complete WAV | 16.532 s |
| Generated audio | 3.240 s |
| Native audio format | 48,000 Hz, mono, PCM16 |
| Generation speed | 0.196 audio seconds per wall-clock second |
| Real-time factor | 5.103, lower is faster |
| Peak child-process resident memory | 2,597,531,648 bytes, about 2.60 GB |
| Runtime process termination test | 0.016 s |

The new app buffers a complete selected passage before playback, so its first-audio delay is expected to be roughly the generation time plus capture/playback overhead when the model is warm. Actual Windows time to first audio, app startup/checksum time, device output latency and Windows memory use are **not measured**. The app records its own timings and runtime peak working set for testing there.

CPU baseline: x64 with AVX2. No discrete GPU required by this package. Reserve more RAM than the measured 2.60 GB runtime peak for Windows, browsers, Word and the Reader; 8–16 GB system RAM is a practical test range, not a validated minimum. No CUDA/Vulkan build, GPU timings or 4-bit-vs-full-precision quality comparison was performed.

## Verification performed

- Model SHA-256 verified when downloaded: `502efe74f6a59c370b3abf5a3fcfd7c3955ca6c167b411c8aee3977e9e46c079`.
- Windows runtime release ZIP SHA-256 and ZIP integrity verified; included runtime binaries are unchanged.
- Same-version Linux runtime release SHA-256 verified.
- Pinned Q4 model loaded and generated a complete Norwegian WAV at its native sample rate.
- Eight C# core checks passed, including parsing that real WAV, preserving Norwegian Unicode, rejecting blank/oversized/null-containing selections and rejecting incomplete or unexpectedly resampled WAV data.
- Authenticated readiness endpoint returned HTTP 200. Unauthenticated access to that endpoint returned HTTP 401.
- Runtime process termination completed; this tests the Linux process, not Windows Job Object behavior.
- Final Windows shell and core compilation passed; PE format verified.

## Windows validation still required

The README contains the manual acceptance list. Hotkey registration, UI Automation, helper timeouts, clipboard backup/restore, Windows Job Object cleanup, actual SoundPlayer playback, cancel/restart races and offline reuse have been implemented but not exercised on Windows here. Do not describe this build as having passed those tests.

The CPU test is slower than real time. This is a useful correctness and quality POC, not evidence of an acceptable everyday-reader latency. Native streaming and/or a GPU-enabled VoxCPM2 runtime are candidates for a later iteration after evaluating the generated voice.

## Voice gap

The public base VoxCPM2 model was approved after online listening. That does not establish that its Q4 conversion or this runtime's default voice matches the demo. The adapter ignores voice-description instructions, including the Oslo-female prompt. A suitable reference voice was not supplied or tested, and the NbAiLab fine-tune is not bundled. The app explicitly labels this gap. No Piper or other TTS model was substituted.

## Licensing

New Reader code is MIT. The runtime's own included license is MIT, not Apache-2.0; its dependencies retain their notices. The public VoxCPM2 model and cstr model conversion are Apache-2.0. These are separate code and model licenses. Included license files and model provenance should accompany redistribution. This POC does not add or claim rights to an identifiable person's voice.

Sources and exact model/runtime identities are in README.md and MODEL_MANIFEST.json.
