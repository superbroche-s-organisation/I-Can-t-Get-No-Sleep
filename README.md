# I Can't Get No Sleep (ICGNS)

![I Can't Get No Sleep](marketing/nexus/i-cant-get-no-sleep-header-sv-v2-1300x372.png)

A Stardew Valley mod that lets you keep playing after 2AM. At 6AM, a new day begins and the game saves automatically.

By default, shipping-bin items are sold when you sleep or pass out from exhaustion. If you stay awake until 6AM, they remain in the bin, including after saving and reloading. An optional setting enables normal shipping and the sales screen at 6AM instead.

## Download and install

Download **[ICGNS-1.3.0.zip](https://github.com/superbroche-s-organisation/I-Can-t-Get-No-Sleep/releases/download/v1.3.0/ICGNS-1.3.0.zip)** from the [v1.3.0 release](https://github.com/superbroche-s-organisation/I-Can-t-Get-No-Sleep/releases/tag/v1.3.0), or use the [ZIP included in this repository](releases/ICGNS-1.3.0.zip?raw=true). This is the compiled mod for installation and Nexus Mods uploads.

GitHub also provides automatic **Source code (zip)** and **Source code (tar.gz)** downloads for development.

1. Install SMAPI.
2. Extract the **ICGNS** folder from the ZIP into your game's **Mods** folder.
3. Launch Stardew Valley through SMAPI.

Requires Stardew Valley **1.6.15 (build 24356)**, SMAPI **4.5.2 or later**, and Windows 64-bit. No configuration or additional mod is required.

For an update from a private test build, close the game and remove its old mod folder before installing ICGNS. Retained shipping items and wake-up markers are imported automatically when loading a save.

## Configuration

With **Generic Mod Config Menu** installed, open its mod options and select **I Can't Get No Sleep**. The options appear in this order:

| Option | Default | Behavior |
| --- | --- | --- |
| Enable mod | On | Turn off to restore normal 2AM pass-out, clock warnings and shipping. An ongoing save finishes first. |
| Enable sunrise | On | Gradually brighten outdoors from 4AM to 6AM. Turn off to use another lighting mod. |
| Sell shipping bin at 6AM | Off | Sell even when staying awake: normal fade, shipping summary/save, then fade back to your position. Empty bins use the normal save screen. |
| Show fatigue bar (unreleased) | On | Purple gauge beside health/energy, hidden until fatigue level 1. Display only; fatigue penalties stay active when hidden. |

Changes apply without restarting; select **Save** to keep them. GMCM is optional. In multiplayer, the host controls **Enable mod** and **Sell shipping bin at 6AM**; sunrise is personal. Farmhand gameplay preferences apply in their own saves.

To let **Dynamic Dawn/Dusk**, **Golden Hours**, or another lighting mod control sunrise, uncheck this option. Without GMCM, launch the game once with ICGNS, close it, then edit `Mods/ICGNS/config.json`:

```json
{
  "EnableMod": true,
  "EnableSunrise": true,
  "SellAt6AM": false,
  "ShowFatigueBar": true
}
```

The JSON above shows the defaults. Restart after editing it manually. Set `EnableSunrise` to `false` to let another mod handle lighting; this leaves the 6AM day change and shipping rules active. Setting `EnableMod` to `false` restores vanilla gameplay; retained shipping items are still recovered safely.

## Features

- Keep playing past 2AM, with the next day starting and saving at 6AM.
- By default, retain shipping-bin items until you sleep or pass out from exhaustion.
- Optionally sell at 6AM with the normal sales screen and fades, while keeping your position.
- Gradual outdoor sunrise from 4AM to 6AM, respecting clock speed and pauses.
- Stay on the same Mines, Skull Cavern or Volcano Dungeon floor at 6AM, with your position and the floor's contents preserved.
- Visible scene with localized saving text in the bottom-left during ordinary 6AM transitions; the scene briefly freezes while saving.
- Optional Generic Mod Config Menu checkboxes for the mod, sunrise and dawn shipping.
- Normal late-hour clock colors, without shaking or hourly fatigue bubbles.
- Short morning notification: "6AM, a new day begins!"
- English and French messages.

The native daily farm updates, exhaustion, combat, special overnight events, and required level-up menus remain active. Staying awake does not recover health or energy or charge rescue fees.

Generated dungeon floors are kept during the current session. Quitting and reloading regenerates the floor, placing you at its entrance on the same level. Temporary event maps retain the game's normal fallback.

## Progressive fatigue (unreleased)

The source now includes a cost for consecutive all-nighters; the published 1.3.0 ZIP does not include it yet. The first awake dawn starts a twelve-game-hour grace period. Fatigue begins at 6PM that day, then rises every twelve hours until you sleep.

| Time after the first awake dawn | Level | Maximum energy reduction | Speed reduction |
| --- | --- | --- | --- |
| 12 hours (6PM) | 1 | 10% | None |
| 24 hours (next 6AM) | 2 | 20% | None |
| 36 hours | 3 | 30% | 0.25 |
| 48 hours | 4 | 40% | 0.50 |
| 60 hours | 5 | 50% | 0.75 |
| Each further 12 hours | +1 | Capped at 50% | Another 0.25, capped at 2 |

The visible **Fatigue** effect shows your level. An optional purple **F** gauge appears beside the native health/energy bars only from level 1, fills between thresholds and reaches full at level 10, when both penalties are capped. Hover to see the actual level and penalties, including levels above 10. **Show fatigue bar** is on by default in GMCM (`ShowFatigueBar: true`); each player can hide it without disabling penalties. The gauge also disappears after real sleep or when the mod is disabled.

Bed sleep or a genuine overnight exhaustion pass-out resets fatigue; the synthetic 6AM transition and dawn shipping do not. Food can refill energy up to the reduced maximum, but does not clear fatigue. Penalties use the normal maximum including other buffs, without permanently changing your base stats. Removing fatigue does not itself grant energy; sleeping uses the game's normal recovery.

Progress is saved per player, including split-screen players. Reloading preserves the level and partial twelve-hour interval; disconnected players do not accumulate additional fatigue. Disabling the mod removes the effect and pauses the counter; finishing a night with the mod disabled clears it. Existing saves start tracking from their first all-nighter with this feature installed.

The fatigue rules have standalone tests. The source has also been compiled and checked against the installed Windows game, including GPU gauge rendering with native textures. Full live gameplay, saves and multiplayer behavior still need verification; no new public release includes fatigue yet.

## Multiplayer

Install the same version on the host and every player's computer, including split-screen players. The host controls the mod switch and dawn shipping. With default settings and shared money, shipping requires all online players to sleep or pass out; separate wallets follow each player's sleep status. Enabling dawn shipping sells online players' bins at 6AM even if they stayed awake; disconnected players' personal bins remain retained. A complete live multiplayer session still needs verification.

## Late-night performance

During extended nights, identical failed standard NPC/animal path searches retry at most once per second; successful paths, combat AI and custom endpoint algorithms are untouched. The quiet save also uses less forced world-buffer rendering. A synthetic test exercises the installed game's pathfinder to verify that repeated identical failures avoid repeated full searches.

The reported stutter after 2AM could not be reproduced without the affected player's log, location or mod list. These are targeted mitigations; that player's case still needs confirmation.

## Build from source

Install a .NET SDK that can target .NET 6 and install SMAPI in your game directory. References are read from your own game installation; game libraries are not included in this repository.

From the repository root in PowerShell:

```powershell
.\scripts\Build.ps1 -GamePath 'C:\Path\To\Stardew Valley' -RunTests
```

The script writes the mod ZIP to `artifacts/`. Use `-RunGraphicsTests` to also run rendering checks on the GPU in a hidden window. Tests use the installed game's DLLs and do not load a player save. The preview migration checks additionally use a local private-build archive when available; that archive is not distributed.

Without the game installed, test the fatigue clock and balance rules with `dotnet run --project tests/fatigue-logic/ICGNS.Fatigue.Tests.csproj --configuration Release`. This compiles the actual fatigue progress model; it does not validate the game API integration or replace `Build.ps1 -RunTests` before a release.

Every new public release must include an installable `ICGNS-<version>.zip`, followed by GitHub's automatic source archives. The [release procedure](releases/README.md) validates the compiled package and blocks publication when it is missing.

## Documentation and assets

- [Player guide](docs/README.release.md)
- [Changelog](CHANGELOG.md)
- [Development notes in French](docs/DEVELOPMENT.fr.md)
- [Nexus header and source artwork](marketing/nexus/)
- [YouTube thumbnail](marketing/youtube/)

Before uninstalling, sleep once and let shipping and saving finish. If ICGNS was removed while holding unsold items, reinstall it and load your save to recover them.
