# Changelog — I Can't Get No Sleep (ICGNS)

Changes are listed from newest to oldest. Earlier versions were private development builds.

## 1.1.0 — 2026-10-15

### Added

- Added `EnableSunrise` in `config.json` (default: `true`). Set it to `false` to disable ICGNS's gradual dawn lighting when using mods such as Dynamic Dawn/Dusk or Golden Hours, while retaining the 6AM day change and automatic save.

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
