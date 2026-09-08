# Norwegian TTS Model Matrix

| Model | Norwegian | Tested | Quality result | Approx size / deployment | Licence | Current status |
|---|---|---|---|---|---|---|
| Piper NVCC medium | Yes | Yes | Below required quality | ~77 MB voice | GPL runtime, voice/data terms separate | Baseline only |
| Piper Talesyntese | Yes | Yes | Not preferred | ~63 MB voice | GPL runtime | Baseline only |
| VoxCPM2 NbAiLab female Oslo | Yes | Yes, online | Very good | stock multi-GB, ~2.3 GB class at 4-bit | verify exact fine-tune/runtime | Current quality benchmark |
| VoxCPM2 male Oslo | Yes | Available | Not central yet | similar | verify | Secondary |
| Chatterbox Multilingual PyTorch | Yes | Yes | Works, too slow | large runtime | MIT | PyTorch route rejected |
| Chatterbox Multilingual Q4 ONNX | Yes | Not yet properly tested | Unknown | ~800-850 MB | MIT | High-priority test |
| MOSS-TTS-Nano Norwegian | Yes | Not successfully quality-tested | Unknown | model small, tested PyTorch deployment too heavy | Apache 2.0 | Revisit only with lightweight runtime |
| SpeechT5 Norwegian | Yes | Not meaningfully tested | Unknown / older | ~578 MB weights + runtime | Apache 2.0 | Low priority |
| CosyVoice Norwegian | Yes | Not required | Potentially high | GB-class | CC BY-NC | Avoid for production |
| XTTS Norsk | Yes | Not required | Potentially good | ~2 GB | Coqui CPML | Avoid for production |
| Prat-9B / VibeVoice | Yes | No | Potentially high | huge | MIT | Impractical |
| Supertonic | No official Norwegian at last check | No | N/A | attractive small ONNX design | permissive | Watch for Norwegian support |

## Current benchmark

VoxCPM2 female Oslo.

Any new model should be compared against that quality, not merely against Piper.
