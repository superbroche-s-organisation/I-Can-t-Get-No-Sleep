## Optional 6AM shipping

Added **Sell shipping bin at 6AM**, off by default. When checked, staying awake until 6AM sells the bin using the game's normal settlement, fades to the sales summary and save, then fades back to your preserved position. Empty bins use the normal save screen. Health, energy, dungeon continuity and rescue-fee rules still follow ICGNS's awake transition.

## Master switch

**Enable mod** is now the first GMCM checkbox and defaults to on. Uncheck it to restore vanilla clock warnings, 2AM pass-out and shipping. An ongoing day/save transition finishes first, and retained items are recovered safely.

GMCM remains optional. All three options are also available in `Mods/ICGNS/config.json`:

```json
{
  "EnableMod": true,
  "EnableSunrise": true,
  "SellAt6AM": false
}
```

Changes from GMCM apply without restarting; select **Save** to persist them. Restart after editing JSON manually. The host controls the master switch and dawn shipping in multiplayer; sunrise remains personal. Disconnected players' personal shipping bins are retained.

## Late-night performance

Added a one-second retry limit for identical failed standard NPC/animal path searches between 2AM and 6AM. Map, tile, destination, search limit, date and clock changes invalidate the cached failure. Successful paths, monsters, custom endpoints, events and daytime searches keep their native behavior. The quiet save only forces the additional render buffer during its final pre-dawn interval; native shipping transitions don't request it.

The affected player's stutter could not be reproduced without their log, location or mod list. These are targeted mitigations and require confirmation in that player's game.

## Installation and validation

Download **ICGNS-1.3.0.zip**, close the game and replace **Mods/ICGNS** with the folder from the ZIP. This is also the ZIP for Nexus Mods. Keep your `config.json` if updating an existing installation.

Requires Stardew Valley **1.6.15 (build 24356)**, SMAPI **4.5.2 or later**, and **Windows 64-bit**. GMCM is optional. GitHub supplies source ZIP/tar.gz downloads separately.

Compiled against the actual game/SMAPI libraries. All **508 automated checks passed**, including runtime toggles, GMCM defaults and option order, actual shipping inventory filtering, native fade selection, the real pathfinder with repeated blocked searches, dungeon continuity, item persistence and GPU sunrise rendering. Full live multiplayer and the affected player's stutter case remain unverified.

See [CHANGELOG.md](https://github.com/superbroche-s-organisation/I-Can-t-Get-No-Sleep/blob/main/CHANGELOG.md) for the full history.
