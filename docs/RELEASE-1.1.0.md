## Optional sunrise lighting

ICGNS 1.1.0 adds `EnableSunrise` in `Mods/ICGNS/config.json`. It defaults to `true`, preserving the gradual outdoor sunrise from 4AM to 6AM.

To let Dynamic Dawn/Dusk, Golden Hours, or another lighting mod handle dawn, launch the game once with this version, close it, and set:

```json
{
  "EnableSunrise": false
}
```

Restart the game to apply the setting. Each player can choose it independently. The 6AM day change, automatic save, save icon, and shipping behavior remain active.

## Installation

Download **ICGNS-1.1.0.zip**, extract the **ICGNS** folder into the game's **Mods** folder, and launch Stardew Valley through SMAPI. This ZIP is also the file to upload to Nexus Mods.

Requires Stardew Valley **1.6.15 (build 24356)**, SMAPI **4.5.2 or later**, and **Windows 64-bit**. Close the game before replacing an earlier version of the mod.

GitHub's **Source code (zip)** and **Source code (tar.gz)** downloads are provided separately for development.

## Validation

The Windows mod package was compiled from the existing `v1.1.0` tag using the real game and SMAPI libraries. All 331 automated checks passed, including configuration on/off, the installed game's Harmony patches, item retention and migration, and sunrise rendering on the GPU. The ZIP's assembly version and packaged documents were validated against the tagged sources.

Live compatibility with Dynamic Dawn/Dusk and Golden Hours still needs verification.

See [CHANGELOG.md](https://github.com/superbroche-s-organisation/I-Can-t-Get-No-Sleep/blob/main/CHANGELOG.md) for the complete version history.
