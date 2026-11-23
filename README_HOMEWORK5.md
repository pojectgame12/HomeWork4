# Homework 5 — Shadow Switch: The Last Beacon

**Play:** https://projectgame12.itch.io/homework5  
**Unity repository:** https://github.com/pojectgame12/HomeWork4  
**Game:** Shadow Switch (not Echo Shift)

This folder contains **Homework 5 documentation and interactive narrative artifacts**, not replacement Unity project files. Upload its contents to the **root of the existing HomeWork4 repository**. Preserve the current `Assets/` folder and existing Homework 4 `README.md`.

## Submission files

| File | Purpose |
| --- | --- |
| [`dramatic-elements.md`](dramatic-elements.md) | Complete Hebrew answers to the ten dramatic-elements prompts, updated to the implemented three-stage campaign |
| [`dramatic-assets/dramatic-arc.svg`](dramatic-assets/dramatic-arc.svg) | Dramatic tension arc diagram (GitHub-renderable) |
| [`dramatic-assets/dramatic-arc.png`](dramatic-assets/dramatic-arc.png) | PNG fallback for the arc |
| [`story/ShadowSwitch_Story.twee`](story/ShadowSwitch_Story.twee) | Editable Twee 3 interactive story source, intended for Twine/Harlowe |
| [`story/ShadowSwitch_story_preview.html`](story/ShadowSwitch_story_preview.html) | Standalone browser-playable story preview, **not** the official Twine export |
| [`story/story-map.md`](story/story-map.md) | Branch map for the interactive story |
| [`liveops/eclipse-night.submission-snapshot.json`](liveops/eclipse-night.submission-snapshot.json) | Documentary snapshot of the remote event JSON in its last verified **disabled** state; does not overwrite live configuration |
| [`HOMEWORK5_SUBMISSION_GUIDE_HE.md`](HOMEWORK5_SUBMISSION_GUIDE_HE.md) | Upload, Twine publish and verification instructions |

## Implemented game overview

Noor must restore power to a darkened observatory by directing a lamp at an obelisk, temporarily solidifying a shadow bridge, collecting energy crystals and reaching the exit. The narrative escalates through **Stage 1 — The First Bridge**, **Stage 2 — The Lost Archive**, and **Stage 3 — The Grey Warden**. The campaign adds three difficulty profiles, optional hints, persistent medals and time records, archive lore and a branching ending choice.

The event **Eclipse Night** is configured at `liveops/eclipse-night.json` and retrieved by `ShadowLiveOps.cs` at runtime. The remote configuration has been seen active and subsequently disabled in the Unity editor without a code rebuild. An independent WebGL live-update check remains recommended.

## Verification status

- Three stages: user-reported 15/15 functional checks passed in Unity.
- Remote Live Ops: active mode and later disabled mode confirmed in Unity.
- Itch.io: uploaded at the URL above; automated browser-play testing not independently verified.
- Twine: Twee source and interactive preview provided; **export the official HTML via Twine > Publish to File** before claiming the Twine requirement fully complete.
- Instructor approval: confirm separately according to the course instructions.

## Important distinction

Twee/Twine storytelling extends the same fictional world but is a **separate narrative prototype**; its variables do not directly control Unity. The game contains its own, smaller set of ending branches. The local fallback can retain an enabled event even if the remote request fails, so inspect the displayed Live Ops source while testing.
