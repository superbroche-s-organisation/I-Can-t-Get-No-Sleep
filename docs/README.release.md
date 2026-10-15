# I Can't Get No Sleep (ICGNS)

## Description

Keep playing after 2AM. At 6AM, the next day begins and the game saves automatically. Stay where you are on permanent, accessible maps, with a gradual sunrise from 4AM to 6AM and a small save icon while the scene briefly pauses.

Items in the shipping bin are sold when you sleep or pass out from exhaustion. If you stay awake until 6AM, they remain in the bin for a later night, including after saving and reloading.

## Download and installation

1. Install SMAPI.
2. Download the ICGNS release ZIP from the mod's Nexus Mods Files tab.
3. Extract the **ICGNS** folder into your Stardew Valley **Mods** folder.
4. Launch the game through SMAPI.

The installed folder should contain `Mods/ICGNS/manifest.json` and `Mods/ICGNS/ICGNS.dll`. No configuration is required.

For an update, close the game and replace the previous installation with the new ICGNS folder. If you used a private test build, remove its old mod folder before installing ICGNS so both builds cannot run together. Items retained by the test build are imported automatically when you load your save.

## Configuration

To let **Dynamic Dawn/Dusk**, **Golden Hours**, or another lighting mod handle sunrise, launch the game once with ICGNS and close it. In `Mods/ICGNS/config.json`, set:

```json
{
  "EnableSunrise": false
}
```

Restart the game to apply the change. The default is `true`, preserving ICGNS's gradual outdoor sunrise from 4AM to 6AM. Disabling it leaves the 6AM day change, automatic save, save icon, and shipping behavior active.

## Main features

- Removes the automatic 2AM pass-out; exhaustion and combat still work normally.
- Starts and saves a new day at 6AM while keeping native daily farm updates.
- Retains unsold shipping-bin items until you sleep or pass out from exhaustion.
- Adds gradual outdoor sunrise between 4AM and 6AM, respecting time speed and pauses.
- Keeps ordinary 6AM transitions visible with a small save icon; the scene briefly freezes during saving. Special overnight events and required level-up menus retain their native screens.
- Removes the late-hour red clock, shaking clock, and hourly fatigue bubbles.
- Shows a short morning message: "6AM, a new day begins!"
- Includes English and French messages.

## Requirements

- Stardew Valley **1.6.15, build 24356**.
- SMAPI **4.5.2 or later**.
- Windows 64-bit; compiled and checked against the Windows game installation.

No additional mod is required. Harmony is provided by SMAPI.

## Multiplayer

Install the same ICGNS version on the host and every player's computer, including split-screen players. With shared money, shipping requires all online players to sleep or pass out. With separate wallets, each player's bin follows their own sleep status. Items belonging to disconnected players are retained.

Multiplayer support is implemented, but a complete live multiplayer session has not yet been verified.

## Compatibility and removal

ICGNS patches the installed game's clock and day transition. If the expected code has changed, it disables its patches and reports the problem in the SMAPI log. Avoid combining it with mods that replace the same timed pass-out or shipping settlement behavior. The late clock display from ItsStardewTime is supported when installed.

The 6AM transition does not recover health or energy or charge rescue fees. Shops keep their usual opening hours. From generated mine or volcano floors, or temporary maps, the game uses its normal home fallback.

Before uninstalling, sleep once and let shipping and saving finish. If you removed ICGNS while it was holding unsold items, reinstall it and load your save to recover them.
