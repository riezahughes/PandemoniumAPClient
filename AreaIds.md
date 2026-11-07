# Pandemonium area (part) IDs

Cleaned up from the RetroAchievements code notes. Offsets are PSX RAM, as in `Addresses.cs`.

## Level numbering

The notes number levels 1-18 and skip the bosses. The client's `LevelId` (0x0007202c) counts the bosses too.
Every "level N" in the notes needs converting before you compare it to `LevelId`. Evidence: the notes' "level 15 part 2" enemies sit at the same addresses as the client's `CloudCitadelPart2*`.

| Notes level | Client level | LevelId |
|---|---|---|
| 1-6 (Skull Fortress - Acid Pools) | same | 1-6 |
| (boss 1) | Shroom Lord | 7 |
| 7-12 (Burning Desert - Honcho's Logmill) | +1 | 8-13 |
| (boss 2) | Goon Honcho | 14 |
| 13-18 (Honcho's Airship - Storm Temple) | +2 | 15-20 |

## Address summary

| Address | Size | Meaning |
|---|---|---|
| 0x0720f5 | byte | Map ID of the area that is loaded. It is NOT unique: many levels share values, and every part 2 and part 4 is 0x7e. |
| 0x0720f6 | byte | Second check, notes only give it for Branky Wastes and Cloud Citadel. The notes say to use it together with 0x072111. |
| 0x072104 | ushort | Per-part value for Canopy Village and Dragoon Skyfort. Resets to 0 on death. The notes say it doesn't work properly for Canopy Village. |
| 0x07210d | byte | Second ID for Dungeon Tower, Spider Forest and Storm Temple. Also 0x12 at the very end of the credits. |
| 0x072111 | byte | Second ID for Hollow Stairway, Branky Wastes, Canopy Village and Cloud Citadel. |
| 0x087440 | byte | Second ID for Storm Temple parts 1 and 2. |
| 0x0720f4 / 0x0720f7 | byte | 00 at the end-of-level and loading screens. |

Which of 0x07210d and 0x072111 owns which level is inferred. The PDF text lost the address-to-note alignment for that block.

## 0x0720f5: map ID per area

`--` means the notes don't list it. Level names are the client's.

| Level (LevelId) | Part 1 | Part 2 | Part 3 | Part 4 | Part 5 | Part 6 |
|---|---|---|---|---|---|---|
| Skull Fortress (1) | 16 | 7e | 3e | | | |
| Hollow Stairway (2) | 86 | 7e | 86 | | | |
| Dungeon Tower (3) | 86 | 7e | 86 | | | |
| Lost Caves (4) | 0e | 7e | ae | 7e | | |
| Fungus Grotto (5) | d6 | 7e | 86 | 7e | 56 | |
| Acid Pools (6) | 6e | 7e | | | | |
| Shroom Lord boss (7) | 86 | | | | | |
| Burning Desert (8) | d6 | 7e | 76 | 7e | | |
| Branky Wastes (9) | ce | 7e | 3e | 7e | | |
| Spider Forest (10) | 2e | 7e | ae | | | |
| Canopy Village (11) | 86 | 7e | 86 | 7e | 46 | 7e |
| Soldier Barracks (12) | -- | -- | -- | -- | -- | -- |
| Honcho's Logmill (13) | fe | -- | 5e | | | |
| Honcho's Airship (15) | e6 | -- | 56 | | | |
| Dragoon Skyfort (16) | 1e | 7e | 86 | | | |
| Cloud Citadel (17) | e6 | 7e | 86 | 7e | | |
| Efreet Palace (18) | 1e | 7e | f6 | | | |
| Frozen Cavern (19) | 56 | -- | -- | | | |
| Storm Temple (20) | de | -- | c6 | | | |

Other values: 00 = end-of-level screen, 06 = credits part 1, 7e = credits part 2, 2e = Mystic Map, 96 = Speed Greed bonus (first part only), fe = Full Tilt bonus.

## Second-byte IDs

0x0720f6 (use with 0x072111):

| Level | Part 1 | Part 2 | Part 3 | Part 4 |
|---|---|---|---|---|
| Branky Wastes | 11 | 09 | 11 | 09 |
| Cloud Citadel | 10 | 09 | 12 | 09 |

0x072111:

| Level | Part 1 | Part 2 | Part 3 | Part 4 | Part 5 | Part 6 |
|---|---|---|---|---|---|---|
| Hollow Stairway | 1e | 1e | 5e | | | |
| Branky Wastes | 1e | 66 | 66 | 1e | | |
| Canopy Village | 1e | 1e | 66 | 66 | 1e | 1e |
| Cloud Citadel (unstable per the notes) | 1e | 00 | 1e | 66 | | |

0x07210d:

| Level | Part 1 | Part 2 | Part 3 |
|---|---|---|---|
| Dungeon Tower | 00 | 1e | 9e |
| Spider Forest | 36 | 9e | 9e |
| Storm Temple | -- | 00 | 2e |

0x087440: Storm Temple parts 1 and 2 = 1e.

0x072104 (ushort):

| Level | Part 1 | Part 2 | Part 3 | Part 4 | Part 5 | Part 6 |
|---|---|---|---|---|---|---|
| Canopy Village (unreliable) | 0 | 8 | b | 10 | 24 | 0 |
| Dragoon Skyfort | 1b | 21 | c | 3 | | |

## What this means for the false checks

- 0x0720f5 alone can't separate parts. Part 2 and part 4 of most levels are both 0x7e.
- Hollow Stairway, Dungeon Tower and Canopy Village parts 1 and 3 share 0x86.
- To gate an enemy check by part, combine `LevelId` + 0x0720f5 + the level's second byte.
- Levels where the notes give only 0x0720f5 (Skull Fortress, Lost Caves, Fungus Grotto, Burning Desert, Dragoon Skyfort and others) can only be split into 1/2/3 where the values differ.
- Some gaps will need finding by hand: Soldier Barracks, and part 2 of Logmill, Airship, Frozen Cavern and Storm Temple.
