Continue my existing Norwegian TTS work from GitHub, preserving all earlier work.

Repository: https://github.com/workavoidance/Skrivi
Branch: tts-handoff
Read first: docs/tts-handoff/voxcpm2-reader/START_HERE.md and CONTINUATION.md.
Then read the earlier handoff files under docs/tts-handoff/.

Inspect the current local project, Git remotes, branch and uncommitted work before fetching. If this project already has that repository, fetch tts-handoff without overwriting or switching my active work blindly. Otherwise clone that branch into an unused subfolder inside this local project. Use a separate checkout when needed. Do the file transfer yourself through Git; do not ask me to copy ZIPs or documents.

The new dated handoff supersedes the earlier requirement for the inaccessible NbAiLab fine-tune: public OpenBMB VoxCPM2 is now approved. Its Reader 0.2 source and measured results are preserved in reader-source/. Reuse that implementation. The original multi-model shootout is a different app; locate and inspect its existing source or ZIP in the attached folders before modifying anything. Start with known Downloads names containing Norwegian_TTS_Quality_Tester, shootout, or Reader_POC_Alpha_0_4_Native_Voice_Speed. If a focused search cannot locate it, ask me for its location instead of recreating it.

First establish and record what exists: source paths, last working versions, build commands and persistent model caches. Detect my actual CPU, GPU, VRAM and RAM. Explain what can be reused, then extend the existing shootout with the next technically valid bigger-model comparison, using the detailed experiment plan in CONTINUATION.md.

Keep models persistent, preserve native cadence and full word endings, save comparable WAVs and measure actual generation time and effective CPU/GPU device. Keep a full-WAV quality baseline. I make the final listening decision. Keep TTS separate from Skrivi's runtime and do not merge this handoff branch into main as part of setup.

Maintain a PROJECT_STATUS.md inventory and commit substantive work so later chats can continue reliably. Make routine choices independently, but do not replace the existing shootout without my explicit agreement.
