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

Changes apply without restarting; select **Save** to keep them. GMCM is optional. In multiplayer, the host controls **Enable mod** and **Sell shipping bin at 6AM**; sunrise is personal. Farmhand gameplay preferences apply in their own saves.

To let **Dynamic Dawn/Dusk**, **Golden Hours**, or another lighting mod control sunrise, uncheck this option. Without GMCM, launch the game once with ICGNS, close it, then edit `Mods/ICGNS/config.json`:

```json
{
  "EnableMod": true,
  "EnableSunrise": true,
  "SellAt6AM": false
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

Every new public release must include an installable `ICGNS-<version>.zip`, followed by GitHub's automatic source archives. The [release procedure](releases/README.md) validates the compiled package and blocks publication when it is missing.

## Documentation and assets

- [Player guide](docs/README.release.md)
- [Changelog](CHANGELOG.md)
- [Development notes in French](docs/DEVELOPMENT.fr.md)
- [Nexus header and source artwork](marketing/nexus/)
- [YouTube thumbnail](marketing/youtube/)

Before uninstalling, sleep once and let shipping and saving finish. If ICGNS was removed while holding unsold items, reinstall it and load your save to recover them.
