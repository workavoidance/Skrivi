# TTS cloud-to-local handoff: September 8, 2026 update

This folder preserves the subsequent public VoxCPM2 Reader conversation. It supplements the earlier shootout handoff in the parent folder. Read this file first, then CONTINUATION.md, then the earlier HANDOFF.md and DECISIONS.md. Where they conflict, this dated update records the later decisions.

## What exists

| Item | State and location |
| --- | --- |
| Earlier multi-model shootout | Built in the earlier chat. Its lessons are in ../HANDOFF.md. Its executable/source was NOT recovered in this chat. Find the existing local copy before rebuilding. |
| Piper Reader reference | Earlier handoff names Reader_POC_Alpha_0_4_Native_Voice_Speed.zip. It is distinct from the VoxCPM2 Reader. |
| VoxCPM2 Reader 0.1 and Voice Lab 0.2 | Actually built and delivered in this chat. 0.2 source is preserved in reader-source/. Do not recreate it. |
| Norwegian reference voices | Seven clips from six NVCC speakers. Provenance is in voices/. FETCH_VOICES.py retrieves the exact original short clips without downloading the full archive. |
| Large-model shootout extension | Discussed, not implemented. Extend the original shootout once located. |

The repository/branch is a transfer location, not approval to integrate TTS into Skrivi. Keep Reader runtime and settings separate. No changes to the Skrivi application are required.

## Corrections to the earlier handoff

- Voice-file precision correction: inspecting all seven extracted archive WAV headers during this handoff found PCM16/48 kHz already. The corpus PDF describes the original sessions as 24-bit. Earlier sample notes repeated that session precision as if it were the extracted files. Restoration preserves the exact PCM payloads; only WAV metadata differs.

- The inaccessible NbAiLab fine-tune is **no longer a requirement**. The user tried the official public OpenBMB VoxCPM2 demo and explicitly approved its quality. He selected public VoxCPM2 over OmniVoice after licensing discussion.
- The tested compact model is **1,689,498,432 bytes**, not the earlier 2.3 GB estimate. Exact model revision/checksum are in reader-source/MODEL_MANIFEST.json. Its old validation fields are historical download-stage statements; the later build reports supersede them.
- The current C++ adapter **ignores text-only voice instructions**. Preserve the desired female Oslo voice as a quality target, not a claim of implemented control. Reference WAV conditioning is implemented in 0.2.
- User feedback: the POC and speech quality were great, including thanks for the voice samples; generation was far too slow on his reportedly powerful PC. No CPU/GPU/RAM specifications or Windows timing export were provided here.
- The product goal has no arbitrary selected-text ceiling. The shipped 0.2 implementation nevertheless has a **4,096-character POC limit** and buffers a complete selection. This is a limitation to address, not a new product requirement.
- The user now wants a renewed multi-model comparison, possibly with CPU/GPU toggles. He explicitly objected to rebuilding an existing shootout merely because another chat could not find it.

## Source and build

reader-source/ is a snapshot of the delivered 0.2 C# core and Windows shell, with tests, reports and provenance. No application code was rewritten for this transfer.

- Target: Windows x64, .NET Framework 4.8, AVX2 CPU.
- Build on a Windows developer machine with .NET 8 SDK: BUILD_LOCAL.ps1. This downloads only the pinned ~8.3 MB CPU runtime if needed, verifies it, builds the existing source into build/, and preserves upstream notices. It never downloads the model.
- BUILD_LOCAL.ps1 is a new handoff helper, reviewed but not executed on Windows here. FETCH_VOICES.py uses only Python's standard library; conversion was compared with all seven delivered WAV PCM payloads and matched exactly.
- Existing BUILD.ps1 and BUILD_LINUX.sh are archived build scripts. BUILD_LINUX.sh references the old cloud compiler paths. Do not use it as a ready-to-run local command.
- Existing TEST.ps1 needs Norwegian-Test.wav, which is not stored in this source snapshot. The original WAV is in the delivered 0.2 ZIP. Restore it or generate the same sentence locally before running that audio-dependent test. Source compilation does not require it.
- The csproj still declares 0.1.0 although the delivered UI and ZIP are 0.2. This pre-existing metadata inconsistency is preserved; correct it in a subsequent development change.

Model: `%LOCALAPPDATA%\VoxCPM2ReaderPOC\models\voxcpm2-q4_k.gguf`.
Voices: `%LOCALAPPDATA%\VoxCPM2ReaderPOC\voices`.
Settings: `%LOCALAPPDATA%\VoxCPM2ReaderPOC\voice-settings.json`.

Find/reuse these before downloading. Do not move or delete model caches automatically.

## How the local chat should begin

Read ../ and this folder's handoff documents. Inspect the attached project, Git state and existing shootout files. Locate the original shootout and establish a working baseline before editing. If it cannot be found after a focused search, ask for its location rather than creating a replacement. Preserve uncommitted changes, use a separate checkout if necessary, and do not switch or reset the user's active branch blindly.

Keep PROJECT_STATUS.md updated in the actual local working project with source paths, model caches, build/test commands, last working version, findings and next steps. Commit that record with substantive changes so future chats have a durable starting point.
