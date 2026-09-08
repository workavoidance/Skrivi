# Next Experiments

## 1. VoxCPM2 4-bit local test

Highest priority.

Use the NbAiLab Norwegian female Oslo voice if technically possible.

Goal is not final deployment. Goal is to answer whether the quality already heard online survives local quantized inference.

Measure:

- exact download size
- installed size
- model load time
- time to first audio
- generation time
- real-time factor
- RAM use
- CPU use
- GPU dependency
- perceived quality

Target size for this first experiment: roughly 2-2.5 GB.

Do not silently substitute a generic VoxCPM2 voice.

If the exact female Oslo fine-tune cannot work with the quantized runtime, document why.

## 2. Chatterbox Multilingual Q4 ONNX

Second priority.

Avoid the slow PyTorch route already tested.

Measure the same metrics as VoxCPM2.

Key question:

Is roughly 800 MB Q4 Chatterbox an acceptable quality/size/speed middle ground?

## 3. Smaller Norwegian-specific models

Only after experiments 1 and 2.

Target:

- preferably under 300 MB
- strong preference under 500 MB
- CPU
- offline
- permissive licence
- clearly better quality than Piper

Search Norwegian research repositories, National Library releases and newer small ONNX/native TTS families.

## 4. Read-from-cursor technical spike

Only after a TTS engine is chosen.

Test UI Automation caret ranges in:

- Word
- Chrome
- Edge
- Google Docs

If a text range can be expanded reliably, read from caret until stopped.

Do not let this block selected-text v1.
