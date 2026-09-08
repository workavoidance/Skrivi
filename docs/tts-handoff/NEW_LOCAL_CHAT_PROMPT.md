You are continuing a Norwegian local text-to-speech / Reader POC project that was previously developed in a long cloud ChatGPT conversation.

Before doing anything else:

1. Inspect the local project/repository.
2. Read every file under `docs/tts-handoff/` on branch `tts-handoff`.
3. In particular read:
   - `HANDOFF.md`
   - `MODEL_MATRIX.md`
   - `DECISIONS.md`
   - `NEXT_EXPERIMENTS.md`

Treat those files as the authoritative project handoff.

Do not restart broad TTS research from scratch unless current information genuinely needs verification.

Do not modify Skrivi or share runtime state with Skrivi unless explicitly asked.

The core project principle is:

> Mergeable core, disposable shell.

The Reader POC UX is already largely defined:

- highlight selected text
- press a global hotkey
- local TTS begins
- press the same hotkey again to stop
- Windows UI Automation selection capture first
- conservative clipboard fallback second
- completely local/offline after setup
- no telemetry or cloud TTS

The unresolved issue is the speech model.

Important existing conclusions:

- Piper is very small and fast but its Norwegian quality is below the required bar.
- Do not waste time further polishing Piper as the main engine.
- The NbAiLab VoxCPM2 female Oslo voice was tested and sounded very good.
- VoxCPM2 female Oslo is the current quality benchmark.
- Chatterbox PyTorch works but is too slow.
- Chatterbox Multilingual Q4 ONNX remains a high-priority candidate.
- MOSS-TTS-Nano looked attractive architecturally, but the tested PyTorch deployment path became too dependency-heavy and large.
- Do not repeat the old uv/pip/MOSS debugging unless you have identified a genuinely lightweight runtime.
- SpeechT5 is lower priority.
- Model quality should be judged before optimizing deployment.

Critical technical lesson:

When comparing speech quality, preserve the model's native synthesis settings.

For Piper specifically, UI speed 1.00x must mean the voice's native configured speed. Do not force `length_scale=1.0`.

The successful quality-reference playback path was:

    synthesize full WAV
    -> synchronous Windows playback
    -> let Windows finish naturally

Do not introduce custom streaming/resampling during quality evaluation.

Your first task:

Inspect what TTS shootout / Reader POC source already exists locally.

Then continue with the highest-priority technically feasible experiment from `NEXT_EXPERIMENTS.md`.

Preferred next experiment:

1. Build/test local VoxCPM2 4-bit using the NbAiLab female Oslo Norwegian voice if possible.
2. If that exact voice cannot work with the available quantized runtime, document the incompatibility clearly.
3. Measure actual download size, installed size, RAM, startup time, time to first audio and generation speed.
4. Preserve quality first.
5. Do not silently fall back to Piper.

After that, test Chatterbox Multilingual Q4 ONNX as the likely smaller alternative.

Make sensible technical decisions without repeatedly asking the user basic questions that are already answered in the handoff documents.
