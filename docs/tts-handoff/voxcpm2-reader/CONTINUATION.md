# Decisions, evidence and next work

## Product boundaries

Standalone Norwegian-first Reader, separate from Skrivi. Modular reusable core and disposable tray shell. Selected text via Windows UI Automation first, conservative Ctrl+C fallback with clipboard restoration. Ctrl+Alt+R toggles read/stop. Offline after setup, no telemetry, no selected-text uploads, persistent models. English later. Preserve native cadence and full word endings. The current CPU build is a quality POC, not a production latency result.

## Implemented 0.2 behavior

- Reader.Core: engine/player interfaces, model identity, input validation, native WAV and reference validation.
- Reader.Windows: WinForms tray shell, global hotkey, UIA helper, clipboard preservation, Windows Job Object ownership, authenticated loopback runtime, SoundPlayer playback.
- CrispASR v0.8.32 CPU runtime, source revision 5baf533c9f5038226b4af1cd6bde3454d6bf9316.
- Runtime is started with --no-gpu and max(1, min(8, logical_CPU_count / 2)) threads.
- Entire WAV is received before playback. No streaming implemented.
- Model is kept loaded after completed readings. Stop kills the owned runtime; the next reading reloads it. Startup includes full model SHA-256 verification.
- Seed 42 default, configurable 1–999999. This changes noise/variation, not guaranteed gender.
- Diffusion steps 10 default, range 4–20. Guidance 2.0 default, range 1–4. Changing those reloads the engine on next use. No quality guarantee for non-default settings.
- Reference import: 2–20 second mono PCM16 WAV, 16–48 kHz, under 2 MB. Safe generated filenames, local copies and settings persistence. No built-in female preset. Import dialog records user permission and shared-output disclosure responsibility. No independent speaker consent was collected by the assistant.
- Speed stays 1.0. App does not pitch-shift, time-stretch, trim or resample generated speech. The upstream engine can segment long text itself.
- The source uses SoundPlayer asynchronous playback with a duration wait, but deliberately does not purge audio at the nominal end. Retain the earlier shootout's lesson about clipped endings and validate actual Windows behavior before changing playback.

## Evidence, not promises

The Windows shell compiled on Linux using Roslyn and .NET Framework reference assemblies. Eleven core checks passed. Native Windows UI, Word/Chrome/Edge capture, persistence, playback and cancellation were not exercised in the cloud environment. The user subsequently reported that the POC/quality were great but too slow; this is positive manual feedback, not a comprehensive acceptance test.

Same-version Linux CPU engine, four threads, short Norwegian sentence, 10 steps, guidance 2:

| Case | Generate | Output length |
| --- | ---: | ---: |
| Default seed 42 | 16.80 s | 3.24 s |
| Default seed 123 | 13.99 s | 3.08 s |
| Synthetic reference, seed 42 | 24.82 s | 2.92 s |

Peak RSS across that run was 3.14 GB. An earlier no-reference run used about 2.60 GB. These shared Linux server measurements are not estimates of the user's PC. Reference test used model-generated audio, not the supplied NVCC female sample. Distinct hashes verified that seed/reference changed output; no independent perceptual quality evaluation was performed.

Delivered ZIP sizes: 0.1 app 7,971,264 bytes; 0.2 app 8,179,519 bytes; reference sample pack 1,948,581 bytes. Model excluded from app ZIPs, exact model size 1,689,498,432 bytes. Original combined model/runtime bundle was about 1.70 GB. Runtime binaries alone are about 19.27 MB installed. Source and dependencies together with the model are about 1.71 GB, before extra source/download copies. Do not redownload these on each version.

## Model/licence decisions

Public VoxCPM2 Norwegian demo was user-approved. OmniVoice also sounded good to the user, but VoxCPM2 was selected for the distributed Reader after discussion of OmniVoice non-commercial model terms and additional tokenizer conditions. Keep OmniVoice as a possible internal comparison with restrictions visible. Do not infer redistribution rights from a community quantization label. Reverify exact revisions/licenses before shipping any new engine.

Reader code MIT; CrispASR code MIT with bundled dependency notices; public VoxCPM2 and the tested cstr conversion Apache-2.0. Earlier handoff covers Piper, Chatterbox, MOSS and other research. Those conclusions are historical and should not trigger a fresh broad research project without need.

Primary source links:
- https://huggingface.co/openbmb/VoxCPM2
- https://huggingface.co/cstr/voxcpm2-GGUF/tree/d426b5da661d5f833b45663879b1a0b0573a5b8e
- https://github.com/CrispStrobe/CrispASR/releases/tag/v0.8.32
- https://huggingface.co/k2-fsa/OmniVoice
- https://huggingface.co/k2-fsa/OmniVoice/blob/main/audio_tokenizer/LICENSE

## Voice samples

NVCC is published as CC0 by the National Library. Seven approximately 5.5-second WAVs, six distinct speakers: female Oslo (two clips, one person), male Oslo, female Malm/Trøndelag, Helgeland, Jostedal, Klepp/Rogaland. Start with Female_Oslo_1.wav. Speaker region is documented metadata; neutral sound and clone similarity were not listening-verified. Source microphone is kanal_2. The selected archive files are already PCM16/48 kHz; the PDF's 24-bit figure refers to original recording sessions. Restoration preserves the exact PCM payload, changing only WAV metadata. These are office-recorded commands, not studio narration. Full provenance is in voices/.

## Proposed next experiment, not yet built

Extend the EXISTING shootout with bigger models. Candidate comparison: Piper baseline; public VoxCPM2 Q4; higher-precision VoxCPM2; OmniVoice; compatible Chatterbox variant. These are candidates, not a claim that all adapters are ready. Keep engine environments isolated and downloads persistent.

1. Locate the existing shootout source and test its current baseline.
2. Inspect the actual Windows CPU, GPU, VRAM and RAM. No specs are known from this thread. Confirm a runtime actually supports this hardware before promising a GPU toggle.
3. Use identical Norwegian sentences/paragraphs, including numbers, names and abbreviations. Compare default voices separately from shared-reference runs where supported.
4. Run models sequentially, measure cold load and repeated warm runs separately. Save untouched WAVs, model/runtime revisions, settings, effective device, time to first audio, full generation time, audio duration, generation rate, peak RAM/VRAM and sizes. Report unavailable metrics as unavailable, not estimated facts.
5. Blind A/B listening with ratings for pronunciation, naturalness and comfort is proposed. The user is final judge, preferably with a native Norwegian listener. ASR/error checks can flag mistakes, not select the winner alone.
6. Evaluate GPU acceleration and later incremental playback separately from the reference full-WAV quality path. Do not repeat the old speed/resampling/clipped-end bugs. Streaming reduces initial wait but cannot fix generation that remains slower than playback indefinitely.

## Non-negotiable working lesson

Failed retrieval does not mean earlier work never existed. Search the known folder, repository and handoff first; inspect before editing; extend existing code. Do not rebuild the shootout from scratch without explicit agreement. Keep a durable inventory/status file and explain reuse decisions. Do not ask the user to manually move files that Git can fetch safely.
