# Decisions

## Product

Reader remains a separate POC for now.

It must not alter Skrivi's runtime or UX.

Architecture should allow Reader to be merged into Skrivi later or removed cleanly.

## UX

Selected text -> hotkey -> read.

Same hotkey -> stop.

No pause key initially.

Read-from-cursor is future work.

## Selection

Windows UI Automation first.

Temporary Ctrl+C fallback second.

Restore clipboard safely.

## Privacy

Offline/local after installation.

No cloud TTS.

No account.

No telemetry.

Do not log selected text.

## Text preprocessing

Structural cleanup only.

Do not rewrite content.

## TTS evaluation

Quality first.

Deployment size second.

Use identical text between models.

Preserve native voice synthesis settings.

Do not use custom audio streaming in quality comparisons.

## Piper

Keep only as a lightweight baseline.

Do not continue polishing Piper as the main engine.

## Native speech speed

UI 1.00x means the model's native voice speed.

Never assume Piper length_scale=1.0 is the native voice speed.

## Current model direction

VoxCPM2 female Oslo is the current quality benchmark.

Next practical comparison is Chatterbox Q4 ONNX.

MOSS PyTorch deployment work is paused.

## Branding

Do not choose a Reader name/domain yet.

Do not invest in installer polish, signing, onboarding or website work until the model question is solved.
