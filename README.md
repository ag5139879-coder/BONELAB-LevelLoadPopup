# BONELAB Level Load Popup

Restores the old **Patch 1** style level loading popup (map name + optional preview image) for **BONELAB Patch 6**.

When you load into a level, a notification/popup appears showing the level name (and later the thumbnail if we can pull it).

## Requirements
- MelonLoader (compatible with Patch 6)
- BoneLib

## Installation
1. Build the project (or download a release when available)
2. Place `LevelLoadPopup.dll` into your `BONELAB/Mods` folder
3. Launch the game modded

## Building
1. Open the solution / project in Visual Studio or Rider
2. Add references to:
   - MelonLoader.dll
   - BoneLib.dll
   - Game assemblies (Il2Cpp / Assembly-CSharp from your BONELAB install)
3. Build → copy the output DLL to Mods

## Status
- [x] Basic level name popup on load
- [ ] Full custom UI with level thumbnail image
- [ ] Config options (duration, style, etc.)

Made for the community.