# I Can't Get No Sleep (ICGNS)

## Description

Keep playing after 2AM. At 6AM, the next day begins and the game saves automatically. Stay where you are, including on Mines, Skull Cavern and Volcano Dungeon floors, with a gradual sunrise from 4AM to 6AM and saving text in the bottom-left while the scene briefly pauses.

Items in the shipping bin are sold when you sleep or pass out from exhaustion. If you stay awake until 6AM, they remain in the bin for a later night, including after saving and reloading.

## Download and installation

1. Install SMAPI.
2. Download the ICGNS release ZIP from the mod's Nexus Mods Files tab.
3. Extract the **ICGNS** folder into your Stardew Valley **Mods** folder.
4. Launch the game through SMAPI.

The installed folder should contain `Mods/ICGNS/manifest.json` and `Mods/ICGNS/ICGNS.dll`. No configuration is required.

For an update, close the game and replace the previous installation with the new ICGNS folder. If you used a private test build, remove its old mod folder before installing ICGNS so both builds cannot run together. Items retained by the test build are imported automatically when you load your save.

## Configuration

If **Generic Mod Config Menu** is installed, open its mod options, select **I Can't Get No Sleep**, and check or uncheck **Enable sunrise**. Changes apply immediately; select **Save** to keep them for future sessions. Each player can choose independently.

To let **Dynamic Dawn/Dusk**, **Golden Hours**, or another lighting mod handle sunrise, uncheck the option. Without GMCM, launch the game once with ICGNS and close it. In `Mods/ICGNS/config.json`, set:

```json
{
  "EnableSunrise": false
}
```

Restart the game after editing JSON manually. The default is `true`, preserving ICGNS's gradual outdoor sunrise from 4AM to 6AM. Disabling it leaves the 6AM day change, automatic save, saving text, and shipping behavior active.

## Main features

- Removes the automatic 2AM pass-out; exhaustion and combat still work normally.
- Starts and saves a new day at 6AM while keeping native daily farm updates.
- Retains unsold shipping-bin items until you sleep or pass out from exhaustion.
- Adds gradual outdoor sunrise between 4AM and 6AM, respecting time speed and pauses.
- Preserves your exact position and the occupied floor's contents in the Mines, Skull Cavern and Volcano Dungeon when 6AM arrives.
- Keeps ordinary 6AM transitions visible with the game's localized saving text in the bottom-left; the scene briefly freezes during saving. Special overnight events and required level-up menus retain their native screens.
- Adds an optional Generic Mod Config Menu checkbox for sunrise, with immediate changes and English/French labels.
- Removes the late-hour red clock, shaking clock, and hourly fatigue bubbles.
- Shows a short morning message: "6AM, a new day begins!"
- Includes English and French messages.

## Requirements

- Stardew Valley **1.6.15, build 24356**.
- SMAPI **4.5.2 or later**.
- Windows 64-bit; compiled and checked against the Windows game installation.

No additional mod is required. Harmony is provided by SMAPI. **Generic Mod Config Menu** is optional and enables the in-game sunrise checkbox.

## Multiplayer

Install the same ICGNS version on the host and every player's computer, including split-screen players. With shared money, shipping requires all online players to sleep or pass out. With separate wallets, each player's bin follows their own sleep status. Items belonging to disconnected players are retained.

Multiplayer support is implemented, but a complete live multiplayer session has not yet been verified.

## Compatibility and removal

ICGNS patches the installed game's clock and day transition. If the expected code has changed, it disables its patches and reports the problem in the SMAPI log. Avoid combining it with mods that replace the same timed pass-out or shipping settlement behavior. The late clock display from ItsStardewTime is supported when installed.

The 6AM transition does not recover health or energy or charge rescue fees. Shops keep their usual opening hours. Occupied dungeon floors are retained during the session; after quitting and reloading, the game regenerates the same floor and you resume at its entrance. Temporary event maps keep the game's normal home fallback.

Before uninstalling, sleep once and let shipping and saving finish. If you removed ICGNS while it was holding unsold items, reinstall it and load your save to recover them.
