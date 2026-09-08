> Later update: read [voxcpm2-reader/START_HERE.md](voxcpm2-reader/START_HERE.md) first. The public VoxCPM2 model has since been approved and built; the exact NbAiLab fine-tune is no longer required. The original text below is retained as historical context.

# Norwegian Local TTS / Reader POC Handoff

## Purpose

This document transfers the important context, discoveries, failures and decisions from a long cloud ChatGPT development conversation into a durable GitHub handoff.

Read this before doing further TTS research or changing the Reader/TTS shootout code.

## Product goal

We are exploring a local text-to-speech accessibility tool aimed primarily at dyslexic students.

Core interaction:

1. User highlights text in another application.
2. User presses a global hotkey.
3. The application reads the selected text aloud locally.
4. Pressing the same hotkey again stops playback.

The product relationship to Skrivi is intentionally undecided. For now, Reader is a separate POC. It must not affect Skrivi's runtime or UX.

Design principle:

> Mergeable core, disposable shell.

## User/product requirements

- Primary audience: dyslexic students.
- Decision-maker initially: parents, similar to Skrivi.
- Windows first, macOS later if useful.
- Fully local after installation.
- No account, telemetry or cloud TTS.
- Selected text must not leave the computer.
- Initial compatibility target: Chrome, Edge, Microsoft Word desktop, Google Docs and Microsoft 365 browser apps.
- PDF support is not required for the first usable version.

## Reader interaction

- Tap global hotkey once to start.
- Tap same hotkey again to stop.
- No separate pause/resume keys in v1.
- No arbitrary maximum selected-text length.
- v1 requires selected text.
- Future idea: if nothing is selected, read from the caret onward until stopped.

## Selection capture

Preferred Windows strategy:

1. Windows UI Automation / TextPattern GetSelection.
2. If unavailable, temporary Ctrl+C fallback.
3. Capture selected Unicode text.
4. Restore previous clipboard safely.

Do not make clipboard-only capture the architectural foundation.

## Text cleanup

Structural cleanup only:

- normalize line endings
- collapse accidental whitespace
- remove zero-width characters
- join wrapped lines where appropriate

Do not rewrite words, spell-check, expand abbreviations, reinterpret maths or paraphrase.

## Language behaviour

Required eventually:

- Norwegian
- English
- Auto

Auto should detect Norwegian vs English locally. Very short selections should fall back to the app interface language. Manual override should remain available.

## Audio behaviour

Use the current Windows default output device. Do not add an audio-device selector initially.

## Critical playback lessons

### Custom streaming sounded worse

An experimental Reader version used:

text -> Piper chunks -> custom sounddevice streaming

This sounded worse than the original shootout.

Reference quality path:

text -> synthesize complete WAV -> Windows WAV playback

Do not introduce custom streaming/resampling during quality comparison unless it is proven perceptually identical.

### Premature playback purge clipped endings

A later build generated a full WAV but used asynchronous playback, estimated duration, then called SND_PURGE. This could stop playback while final samples were still buffered.

Reported symptom:

- shortened syllables
- clipped word endings

Correct reference behaviour:

- synthesize complete WAV
- use synchronous Windows PlaySound
- let Windows decide when playback actually ends
- only call SND_PURGE when the user explicitly stops

### Piper native-speed bug

The successful shootout created something equivalent to:

    SynthesisConfig(speaker_id=...)

and did not specify length_scale.

The Reader speed UI incorrectly treated UI 1.00x as Piper length_scale 1.0. That made the voice significantly faster.

Correct rule:

> UI 1.00x means use the voice model's own native/default speed.

At 1.00x, do not override length_scale.

For non-default speed:

    effective_length_scale = voice.config.length_scale / requested_speed

This fixed the cadence mismatch.

## Piper speaker mapping

NVCC IDs do not mean Bokmål/Nynorsk.

- K = kvinne/female
- M = mann/male
- ON = Eastern Norway
- SV = Southwest Norway
- NV = Northwest Norway
- MN = Mid Norway
- NN = Northern Norway

Approximate mapping:

| ID | Sex | Region / dialect |
|---|---|---|
| KNN | Female | Helgeland / Northern Norway |
| KSV | Female | Klepp / Rogaland |
| MMN | Male | Steinkjer / Trøndelag |
| KON | Female | Oslo / Eastern Norway |
| MNN | Male | Rana / Nordland |
| MSV | Male | Stavanger / Rogaland |
| MON | Male | Oslo / Eastern Norway |
| MNV | Male | Voss / Western Norway |
| KMN | Female | Malm / Trøndelag |
| KNV | Female | Jostedal / Western Norway |

Important correction: KNN is not a Nynorsk marker. KON and MON are the logical Oslo/Eastern defaults.

## Piper conclusion

Strengths:

- tiny, roughly 60-80 MB voices
- fast
- CPU friendly
- easy local deployment

Weakness:

- Norwegian voice quality judged below the desired product bar

Current status:

> Piper is the size/speed/reference baseline, not the preferred production model.

## VoxCPM2

The NbAiLab Norwegian VoxCPM2 female Oslo-area voice was tested online and judged very good. This is the current quality benchmark.

Approximate size discussion:

- stock/BF16: about 5 GB class
- 8-bit: roughly 3.2 GB
- 4-bit: roughly 2.3 GB

A 4-bit local VoxCPM2 experiment around 2-2.5 GB became the next quality-first Reader build target.

This is knowingly too large for the eventual product. The experiment is still useful to answer whether the good online quality survives local quantized inference at acceptable latency.

Do not silently substitute a generic VoxCPM2 voice for the NbAiLab female Oslo fine-tune if the comparison depends on that voice.

## Chatterbox

Chatterbox Multilingual supports Norwegian and is MIT licensed.

The normal PyTorch version was made to work locally.

Outcome:

- it works
- slower than desired on first and subsequent runs
- therefore not suitable in the tested PyTorch form

A multilingual Q4 ONNX version remains worth testing. Approximate model package discussed: 800-850 MB.

Key questions:

1. Is quality close enough to VoxCPM2?
2. Is time-to-first-audio acceptable?
3. Is roughly 800 MB still too large for the eventual product?

## MOSS-TTS-Nano

A Norwegian-specific NbAiLab MOSS-TTS-Nano fine-tune was found.

Attractive on paper:

- about 100M parameters
- Norwegian-specific
- Apache 2.0
- upstream architecture positioned as CPU-friendly

Testing became problematic. Hugging Face demos were unreliable. The attempted local PyTorch route pulled in PyTorch, torchaudio, Transformers, ONNX Runtime, sentencepiece, accelerate, safetensors and other dependencies.

The dependency download alone was roughly 250-300 MB before model weights and the installed footprint was much larger.

There were repeated environment problems involving uv cache locks, hidden installer progress and partially-created environments.

Conclusion:

> Do not keep investing in the PyTorch MOSS deployment route for the Reader product.

Revisit only if a genuinely lightweight native/ONNX deployment appears.

## SpeechT5 Norwegian

A Norwegian SpeechT5 fine-tune exists.

Approximate characteristics:

- Apache 2.0
- about 100M parameters
- roughly 578 MB model weights
- older architecture

It was not meaningfully quality-tested. Current priority is low.

## Other researched models

### CosyVoice Norwegian
Potentially high quality, but Norwegian fine-tune is CC BY-NC 4.0. Avoid for production unless licensing strategy changes.

### XTTS Norsk
Dedicated Norwegian/Bokmål fine-tune, but Coqui CPML/non-commercial restrictions. Avoid for production.

### Prat-9B / VibeVoice
Norwegian/Eastern Norwegian oriented and MIT licensed, but far too large for realistic deployment.

### Supertonic
Very attractive small ONNX/CPU architecture, but official model did not support Norwegian at last check. Worth watching.

## Size target evolution

Desired eventual target became roughly:

- preferably under 300 MB
- ideally under 500 MB
- CPU friendly
- offline
- no huge runtime
- clearly better Norwegian quality than Piper

VoxCPM2 currently passes quality but fails size. Piper passes deployment but fails quality.

That is the core tradeoff.

## Reader POC architecture

The Reader POC should keep reusable logic separate from the temporary shell.

Suggested split:

    reader_core
      selection
      language choice
      cleanup
      controller
      SpeechEngine interface

    reader_poc
      Windows adapters
      tray
      settings
      toast
      startup
      concrete TTS adapter

The reusable core must not import Piper, Qt or Windows-specific implementations.

TTS sits behind a small SpeechEngine interface so Piper/VoxCPM2/Chatterbox/etc can be swapped.

Reader-specific settings/models/runtime should remain independent from Skrivi.

## Hotkey

Reader is a tap/toggle interaction, unlike Skrivi push-to-talk.

A three-key combination such as Left Ctrl + Left Alt + R is reasonable.

Be careful with Norwegian keyboards because AltGr/Right Alt may behave as Ctrl+Alt in some APIs. Prefer physical left-side modifiers where possible.

## Current Reader reference build

Important milestone from cloud development:

`Reader_POC_Alpha_0_4_Native_Voice_Speed.zip`

This was the build where the Piper native-speed bug was fixed.

It is useful as a reference implementation for:

- selection capture
- hotkey toggle
- settings
- tray/toast
- Reader architecture

It is not evidence that Piper is the final model.

## Shootout lessons

1. Always compare models with identical text.
2. Preserve native model cadence.
3. Do not introduce custom streaming during quality comparison.
4. Save generated WAVs so comparison is repeatable.
5. Show generation time, download size, installed size, licence and runtime requirements.
6. Keep engines isolated so one broken dependency does not break the tester.
7. Persistent caches/models should survive tester versions.
8. Do not hide installer/download progress.

## Current quality ranking

1. VoxCPM2 female Oslo: very good, current benchmark.
2. Chatterbox Multilingual: works, but tested PyTorch path too slow.
3. Piper NVCC: excellent technically, quality below bar.
4. Unknown: MOSS Nano Norwegian, Chatterbox Q4 ONNX, SpeechT5 Norwegian.

## Strategic conclusion

The Reader interaction is largely solved. The unresolved problem is the speech engine.

Current situation:

- Piper: practical but insufficient quality
- VoxCPM2: sufficient quality but probably too large
- Chatterbox Q4 ONNX: promising middle ground, untested
- MOSS: attractive conceptually, tested deployment path too heavy
- no final production model chosen

Immediate goal:

> Find the smallest practical local Norwegian TTS model that meets the VoxCPM2-ish quality bar.
