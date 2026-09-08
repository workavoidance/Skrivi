# Norwegian reference voices for Reader 0.2

Start with **Female_Oslo_1.wav**. The corpus identifies this speaker as a woman from Oslo, aged 30 at recording. Female_Oslo_2.wav is another recording of the SAME woman, not a second voice. Oslo is the closest documented match here to the requested neutral Eastern Norwegian sound; neutrality has not been independently assessed by listening.

## Import

1. Extract this ZIP.
2. Open Reader 0.2 and click Import reference voice WAV.
3. Select Female_Oslo_1.wav.
4. Review the reference permission dialog and the source information below.
5. Click Test Norwegian. Keep steps 10 and guidance 2.0 for the first comparison.

Each clip is about 5.5 seconds and 0.53 MB. All are mono 48 kHz PCM16 WAV, within the Reader's required 2–20 second range. Header inspection confirmed that the selected archive files are already PCM16, although the corpus PDF describes the original recording sessions as 24-bit. Restoration preserves the exact PCM payload and changes only WAV metadata. No trimming, pitch change, speed change or noise reduction was applied.

## Included voices

| Files | Corpus speaker | Documented dialect |
| --- | --- | --- |
| Female_Oslo_1.wav, Female_Oslo_2.wav | KON | Oslo |
| Male_Oslo.wav | MON | Oslo |
| Female_Malm_Trondelag.wav | KMN | Malm, Trøndelag |
| Female_Helgeland.wav | KNN | Helgeland, Nordland |
| Female_Jostedal.wav | KNV | Jostedal, Vestland |
| Female_Klepp_Rogaland.wav | KSV | Klepp, Rogaland |

Seven clips, six distinct speakers. For neutral Eastern Norwegian, start with the Oslo files. The others are included for contrasting voices and dialects.

## Source and permission

Source: Norwegian Voice Control Corpus (NVCC), Norwegian Language Bank, National Library of Norway. The publisher releases the corpus under **CC0**, stating that it can be used for any purpose and reshared without permission. This is the publisher's corpus licence statement; no separate speaker-specific cloning consent has been obtained in this task.

Documentation, including licence on page 1 and speaker table on page 4:
https://www.nb.no/sbfil/nvcc/NVCC_about_the_corpus.pdf

Original archive:
https://www.nb.no/sbfil/nvcc/nvcc_1.0.tar

Full source filenames, transcriptions, checksums and conversion details are in provenance.json. Only the higher-quality computer microphone channel (kanal_2) was selected. Metadata flagged for background noise, mistakes or garbled speech was excluded, but this does not establish studio quality.

These are short voice-assistant commands recorded in offices, not audiobook narration. File integrity and Reader-compatible audio format were verified. No subjective listening assessment or VoxCPM2 cloning-quality test was performed for these particular clips. An Oslo reference can influence the model but cannot guarantee its output accent or voice similarity.
