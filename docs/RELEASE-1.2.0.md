## Stay in the mine at 6AM

Fixed the automatic return home when staying awake in the Mines, Skull Cavern or Volcano Dungeon. At 6AM, you keep the same floor, its contents, your exact position and facing direction. Occupied floors belonging to awake farmhands are preserved too; unoccupied floors and ordinary sleep still use normal cleanup.

Dungeon contents are retained during the current session. After quitting and reloading, the game regenerates the floor and you resume at its entrance on the same level.

## Sunrise checkbox

Added optional **Generic Mod Config Menu** support. Open its mod options, select **I Can't Get No Sleep**, then check or uncheck **Enable sunrise**. Changes apply immediately; select **Save** to keep them for future sessions. English and French labels/tooltips are included.

Without GMCM, the existing `EnableSunrise` setting in `Mods/ICGNS/config.json` is still available. Restart after editing JSON manually.

## Saving text

The bottom-left floppy-disk icon is replaced by the game's localized saving text with animated dots. The scene remains visible during ordinary 6AM transitions.

## Installation

Download **ICGNS-1.2.0.zip**, close the game, and replace the **ICGNS** folder in your game's **Mods** folder. Launch through SMAPI. This is also the ZIP to upload to Nexus Mods.

Requires Stardew Valley **1.6.15 (build 24356)**, SMAPI **4.5.2 or later**, and **Windows 64-bit**. GMCM is optional.

GitHub provides **Source code (zip)** and **Source code (tar.gz)** separately for development.

## Validation

Compiled against the actual Windows game and SMAPI libraries. All **350 automated checks passed**, covering the installed game's Harmony patches, occupied dungeon retention and wake-up, consecutive all-nighters, ordinary sleep, safe entrance placement on reload, GMCM save/reset/checkbox callbacks, installed GMCM API signatures, shipping persistence, and GPU sunrise rendering. The installable package is validated against its manifest, assembly version and packaged documents.

A complete live multiplayer session and compatibility with other lighting mods still need in-game verification.

See [CHANGELOG.md](https://github.com/superbroche-s-organisation/I-Can-t-Get-No-Sleep/blob/main/CHANGELOG.md) for the full version history.
