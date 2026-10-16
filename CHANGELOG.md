# Changelog — I Can't Get No Sleep (ICGNS)

Changes are listed from newest to oldest. Earlier versions were private development builds.

## 1.2.0 — 2026-10-16

### Fixed

- Staying awake until 6AM in the Mines, Skull Cavern or Volcano Dungeon no longer sends you home. The occupied floor, its contents, your exact position and facing direction are preserved during the day change.
- Protected floors include those occupied by awake farmhands. Unoccupied floors and ordinary nights still use the game's normal dungeon cleanup.
- After quitting and reloading, generated dungeon floors are recreated by the game; you resume at the same floor's entrance to avoid spawning inside a new wall or rock.

### Added

- Optional Generic Mod Config Menu support, with an **Enable sunrise** checkbox and English/French labels and tooltips.
- The sunrise setting now takes effect immediately from the menu. Save your changes in GMCM to keep them for future sessions.

### Changed

- Replaced the bottom-left save icon with the game's localized saving text and animated dots, while keeping the scene visible during ordinary 6AM transitions.
- Added regression checks for dungeon continuity, consecutive all-nighters, ordinary sleep, GMCM callbacks, and compatibility with the installed GMCM API.

## 1.1.0 — 2026-10-15

### Added

- Added the optional `EnableSunrise` setting in `Mods/ICGNS/config.json`. Its default value is `true`, preserving ICGNS's gradual outdoor sunrise from 4AM to 6AM.
- Added English and French instructions for using the setting with lighting mods such as Dynamic Dawn/Dusk and Golden Hours.

### Changed

- Setting `EnableSunrise` to `false` leaves dawn lighting to other mods. The 6AM day change, automatic save, save icon, and shipping rules remain active.
- Each player can choose this visual setting independently. Restart the game after changing it.
- The compiled Windows/SMAPI package is available as `ICGNS-1.1.0.zip` for direct installation and Nexus Mods uploads.
- GitHub publication now requires a verified installable `ICGNS-<version>.zip`, with GitHub's automatic source archives below it. Publishing a new release containing only sources is blocked.

### Configuration

Launch the game once with this version, close it, and set the following in `Mods/ICGNS/config.json` to turn off ICGNS's dawn lighting:

```json
{
  "EnableSunrise": false
}
```

Compatibility in a live game with Dynamic Dawn/Dusk and Golden Hours still needs verification.

## 1.0.0 — 2026-10-15

### Changed

- Standardized the public name as I Can't Get No Sleep.
- Renamed the installation folder, DLL, assembly, namespaces, and public mod ID to ICGNS / Tom.ICGNS.
- Updated English and French messages and installation instructions.
- Prepared the Nexus release ZIP with an English README and no bundled game libraries or development tools.

### Added

- Automatic migration of private test-build shipping inventories and wake-up markers, preserving item quantities, quality, custom data, and existing ICGNS inventories.
- Migration of disconnected players' retained shipping items and safe repeat imports without duplication.

## 0.2.1

### Fixed

- Fixed sunrise darkening the world by fading the subtractive lighting mask to neutral daylight.
- Preserved visible scene colors during saving with opaque world-buffer rendering.
- Kept the scene available for capture at 100% zoom.

## 0.2.0

### Changed

- Added gradual outdoor sunrise from 4AM to 6AM.
- Removed late-hour red clock warnings, clock shaking, and hourly fatigue bubbles.
- Kept ordinary 6AM save transitions visible with a small save icon.
- Simplified the morning notification to "6AM, a new day begins!" (with a French translation).

## 0.1.0

### Added

- Added play through the night and native next-day saving at 6AM.
- Retained shipping items when staying awake; allowed shipping after voluntary sleep or genuine exhaustion.
- Added multiplayer shipping policies and persistence for retained items.
