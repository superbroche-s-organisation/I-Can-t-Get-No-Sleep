# I Can't Get No Sleep (ICGNS)

## Description

Keep playing after 2AM. At 6AM, the next day begins and the game saves automatically. Stay where you are, including on Mines, Skull Cavern and Volcano Dungeon floors, with a gradual sunrise from 4AM to 6AM and saving text in the bottom-left while the scene briefly pauses.

By default, items in the shipping bin are sold when you sleep or pass out from exhaustion. If you stay awake until 6AM, they remain in the bin for a later night, including after saving and reloading. Enable the optional dawn-shipping setting to sell them at 6AM with the normal sales screen and fades, then resume where you were.

## Download and installation

1. Install SMAPI.
2. Download the ICGNS release ZIP from the mod's Nexus Mods Files tab.
3. Extract the **ICGNS** folder into your Stardew Valley **Mods** folder.
4. Launch the game through SMAPI.

The installed folder should contain `Mods/ICGNS/manifest.json` and `Mods/ICGNS/ICGNS.dll`. No configuration is required.

For an update, close the game and replace the previous installation with the new ICGNS folder, keeping your existing `config.json` to retain preferences. If you used a private test build, remove its old mod folder before installing ICGNS so both builds cannot run together. Items retained by the test build are imported automatically when you load your save.

## Configuration

If **Generic Mod Config Menu** is installed, open its mod options and select **I Can't Get No Sleep**. Select **Save** after changing these checkboxes:

| Option | Default | Effect |
| --- | --- | --- |
| Enable mod | On | The first option. Uncheck to restore normal 2AM pass-out, clock warnings and shipping. An ongoing day/save transition finishes first. |
| Enable sunrise | On | Brightens outdoor lighting from 4AM to 6AM. Uncheck when another mod handles dawn. |
| Sell shipping bin at 6AM | Off | Sells even when staying awake, using the normal fade, sales summary and save, then fades back to your position. Empty bins use the normal save screen. |
| Show fatigue bar | On | Shows a purple fatigue gauge beside health/energy from level 1. Hiding it does not remove fatigue penalties. Personal display setting. |

Changes apply without restarting. In multiplayer, the host controls the mod switch and dawn shipping; each player chooses sunrise independently. Farmhand gameplay preferences apply in their own saves.

To let **Dynamic Dawn/Dusk**, **Golden Hours**, or another lighting mod handle sunrise, uncheck the option. Without GMCM, launch the game once with ICGNS and close it. In `Mods/ICGNS/config.json`, set:

```json
{
  "EnableMod": true,
  "EnableSunrise": true,
  "SellAt6AM": false,
  "ShowFatigueBar": true
}
```

The JSON shows the defaults. Restart after editing it manually. Set `EnableSunrise` to `false` to leave lighting to another mod. This keeps the 6AM day change and shipping behavior active. Disabling `EnableMod` restores normal gameplay; retained shipping items are still recovered safely.

## Main features

- Removes the automatic 2AM pass-out; exhaustion and combat still work normally.
- Starts and saves a new day at 6AM while keeping native daily farm updates.
- By default, retains unsold shipping-bin items until you sleep or pass out from exhaustion.
- Optionally sells the shipping bin at 6AM with native fades and a sales screen, then returns to your preserved position.
- Adds gradual outdoor sunrise between 4AM and 6AM, respecting time speed and pauses.
- Preserves your exact position and the occupied floor's contents in the Mines, Skull Cavern and Volcano Dungeon when 6AM arrives.
- Keeps ordinary 6AM transitions visible with the game's localized saving text in the bottom-left; the scene briefly freezes during saving. Special overnight events and required level-up menus retain their native screens.
- Adds optional Generic Mod Config Menu checkboxes for the mod, sunrise and dawn shipping, with English/French labels.
- Limits repeated failed standard NPC/animal path searches during extended nights and reduces extra rendering for the quiet save.
- Removes the late-hour red clock, shaking clock, and hourly fatigue bubbles.
- Shows a short morning message: "6AM, a new day begins!"
- Includes English and French messages.

## Progressive fatigue

After your first all-nighter ends at 6AM, you have twelve game hours before fatigue starts at 6PM. Every twelve additional hours adds another level:

- Levels 1 and 2: maximum energy is reduced by 10% and 20%, with normal movement speed.
- Level 3: 30% less maximum energy and only 0.25 less speed.
- Level 4: 40% less maximum energy and 0.50 less speed.
- Level 5 onward: maximum energy reduction is capped at 50%; speed continues to decrease gently by 0.25 per level, up to a maximum reduction of 2.

The Fatigue effect displays your current level. Sleeping in a bed or passing out from genuine overnight exhaustion resets it. Dawn shipping and awake saves do not. Food does not cure fatigue, but can replenish energy within the reduced maximum. Each player's progress is saved separately and pauses while disconnected. Disabling the mod removes the effect and pauses the counter; completing a night with the mod disabled clears it.

A purple **F** gauge appears next to the health and energy bars only after reaching fatigue level 1. It gradually fills up to level 10, when both penalties reach their caps. Hover over it to see the actual level and penalties; the true level can continue above 10. The personal **Show fatigue bar** option is enabled by default and can be changed in GMCM or with `ShowFatigueBar` in JSON. Hiding the gauge only changes its display. Real sleep or disabling the mod hides it again.

## Requirements

- Stardew Valley **1.6.15, build 24356**.
- SMAPI **4.5.2 or later**.
- Windows 64-bit; compiled and checked against the Windows game installation.

No additional mod is required. Harmony is provided by SMAPI. **Generic Mod Config Menu** is optional and enables the in-game checkboxes.

## Multiplayer

Install the same ICGNS version on the host and every player's computer, including split-screen players. The host controls the master switch and dawn shipping. With default settings, shared shipping requires everyone online to sleep or pass out; separate wallets follow each player's sleep status. Optional dawn shipping sells online players' bins at 6AM regardless of sleep. Disconnected players' personal bins remain retained.

Multiplayer support is implemented, but a complete live multiplayer session has not yet been verified.

## Compatibility and removal

ICGNS patches the installed game's clock and day transition. If the expected code has changed, it disables its patches and reports the problem in the SMAPI log. Avoid combining it with mods that replace the same timed pass-out or shipping settlement behavior. The late clock display from ItsStardewTime is supported when installed.

The 6AM transition does not recover health or energy or charge rescue fees. Shops keep their usual opening hours. Occupied dungeon floors are retained during the session; after quitting and reloading, the game regenerates the same floor and you resume at its entrance. Temporary event maps keep the game's normal home fallback.

The reported location-dependent stutter after 2AM still needs confirmation from the affected player. The update reduces repeated failed pathfinding and unnecessary buffer rendering; it does not claim the report's exact cause has been identified.

Before uninstalling, sleep once and let shipping and saving finish. If you removed ICGNS while it was holding unsold items, reinstall it and load your save to recover them.
