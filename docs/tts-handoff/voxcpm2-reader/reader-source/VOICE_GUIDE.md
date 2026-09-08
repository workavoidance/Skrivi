# Voice Lab 0.2

## Tweak the existing voice

1. Click Test Norwegian for a baseline.
2. Click Try another, then Test Norwegian again. The variation seed changes diffusion noise and can alter voice/prosody, but is not a gender selector.
3. Keep a seed you like. Settings are saved automatically.

Steps defaults to 10, range 4–20. Lower values can be faster but lose quality. Higher is not guaranteed to sound better. Guidance defaults to 2.0, range 1.0–4.0; it changes diffusion guidance, not pitch or gender. Listen for pronunciation changes. Changing steps or guidance reloads the engine on your next reading, without downloading anything. Seed/reference changes do not require a reload.

Reset tuning restores seed 42, steps 10 and guidance 2.0 while retaining your reference. Playback speed and pitch stay unchanged.

## Use a female reference

Click Import reference voice WAV. Choose a clean female sample you have permission to reuse, preferably Norwegian with the accent and cadence you want. Use one speaker, no music, 2–20 seconds, mono PCM16 WAV, 16–48 kHz, under 2 MB. MP3, stereo and float WAV are rejected, not silently converted.

A downloaded synthetic sample you generated in the official VoxCPM2 demo is a possible reference if you have permission to reuse it and it meets that format. This app does not contact the demo or fetch voices. No female sample is bundled. Reference conditioning works in a Linux technical test; female Oslo matching has not been tested or guaranteed.

Confirm permission in the dialog. Real speakers must consent to cloning. Reader labels speech as AI-generated, and you accept responsibility for retaining that disclosure if sharing output. This explicit confirmation permits omission of the spoken AI-disclosure prefix during reading; upstream non-spoken marking remains unchanged.

Click Test Norwegian, then compare the result. Use default voice turns reference conditioning off.

## Stored locally

Model location is unchanged. References are copied into `%LOCALAPPDATA%\VoxCPM2ReaderPOC\voices`. Settings are stored in `voice-settings.json` in the parent folder. They survive app updates and do not require an internet connection. No reference or selected text is uploaded.

Use default voice retains old WAV files. To remove them, quit Reader and delete the unwanted files from the voices folder. A missing or invalid selected reference produces an error. Do not edit or delete a reference during generation.

## Windows acceptance checks

- Try two seeds on the same sentence. Restart and verify the chosen seed persists.
- Change guidance/steps, test, stop while loading, then test again.
- Import a permitted female WAV, test it, restart, and test offline.
- Cancel the permission dialog and confirm the previous voice remains selected.
- Try an invalid WAV and confirm it is rejected without changing the selected voice.
- Switch back to default, then verify hotkey read/stop and clipboard behavior.

The Windows executable compiles, but these Windows UI checks and a female-voice listening comparison were not performed in the Linux build environment.
