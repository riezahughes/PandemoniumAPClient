# Pandemonium Archipelago Client

An [Archipelago](https://archipelago.gg) client for **Pandemonium!** (PlayStation) that runs alongside the game in the DuckStation emulator. It reads the game's memory to send checks to the multiworld, and writes to it to give you the items you receive.

## Requirements

- A Windows or macOS machine
- [Archipelago](https://archipelago.gg) installed
- The latest [Pandemonium client](https://github.com/riezahughes/PandemoniumAPClient/releases)
- The latest [Pandemonium AP World](https://github.com/riezahughes/PandemoniumAPWorld/releases)
- A legal copy of the NTSC-U version of Pandemonium! for the PlayStation (**SLUS-00232**). Other versions and regions use different memory addresses and won't work
- [DuckStation](https://www.duckstation.org)

## Setup

1. Install the APWorld in Archipelago.
2. Generate the templates and copy the Pandemonium template into the `players` folder of your Archipelago install (create it if it doesn't exist).
3. Edit the YAML to choose your options (see [Options the client supports](#options-the-client-supports)).
4. Generate the game and host the zip from your `output` folder, either yourself or on [archipelago.gg](https://archipelago.gg/uploads).
5. Boot Pandemonium in DuckStation.
6. **Get to the game's main menu**, then run the client (see below).
7. Enter the server, port, slot name and password when asked.

## Start the client from the main menu

Start the client **after** the game has reached its main menu (past the logos, the intro and the demo), not before.

The client only treats the game as "running" once the BIOS has loaded, no demo is playing and the title screen has been passed. Until then it can't trust the memory it reads, so it won't check locations and it holds back received items. If you start it too early, nothing is lost: it keeps retrying every couple of seconds, and items start arriving once you are in the main menu or in a level.

The client also sets up the game when you start a new game: all levels are opened on the level select screen (the client decides which ones you can really enter) and quitting a level returns you to the hub rather than the main menu. Every item you have received so far is given again at that point, because a new game wipes them.

## How it works

### Level unlocks

Every level has an unlock item (for example `Skull Fortress Unlock`) in the multiworld. On the level select screen, picking a level you haven't received the unlock for cancels your choice. Two levels are special:

- **Shroom Lord** and **Goon Honcho** (the bosses) have unlock items like any other level.
- **The Wishing Engine** has no unlock item. It opens once both bosses have been cleared.

### Items

| Item | What the client does |
| --- | --- |
| `<Level> Unlock` | Opens that level on the level select screen |
| `Progressive Health Upgrade` | Raises max health by one heart (the player starts with 2, up to 8) |
| `Free Health` | Restores one heart |
| `Extra Life` | Adds a life (caps at 31) |
| `Eldritch Fireball`, `Phoenix Fireball`, `Freeze Ray`, `Shrink Ray` | Sets your active power up |
| `Invincibility Shield` | Activates the shield |
| `Spellbook Page` | Counts towards the goal when the goal uses spell pages |

Pandemonium only has one power up slot, so a received power up replaces whatever you are holding.

### Locations (checks)

What exists depends on your YAML options:

- **Level clears**: finishing a level and reaching its cleared screen. With `multiclear` off there is one clear per level. With it on there is one per character (Fargus and Nikki), and you have to clear the level as each of them.
- **Full Tilt** and **Speed Greed**: the per-level challenge checks.
- **Level percentage**: reaching a given treasure percentage in a level. The percentage is only written when the level ends, so these are sent when the cleared screen appears. Canopy Village, Soldier Barracks and Frozen Cavern top out at 95%.
- **Pointsanity**: treasure points collected inside a level. The client keeps its own count for the current level, which starts again from 0 each time you enter a level. Dying keeps your count. Boss levels have no point checks.
- **Enemysanity**: defeating individual enemies.

### Clear conditions

There are two different things that get called "clearing", so here they are separately.

**Clearing a level.** A level clear is sent when you are in that level and the level cleared screen is showing. Just reaching the end of a level isn't enough, you need to get to that screen. If `multiclear` is on, the client also checks which character you finished as, so each character's clear is a separate check.

**Your goal.** The `goal` option in your YAML decides when the client tells the server you are finished:

| Goal | Condition |
| --- | --- |
| Credits | The credits start rolling while you are in the game |
| Spell pages | You have received the number of `Spellbook Page` items set by the `spellbook_pages` option. No need to be in a level |
| Both | You have received all the spell pages **and** have reached the credits |

The `items` command shows how many pages you have found. You can also finish by hand with `release` (see below).

### Options the client supports

`goal`, `spellbook_pages`, `enemysanity`, `full_tilt`, `speed_greed`, `multiclear`, `level_percentage`, `level_percentage_range`, `pointsanity`, `pointsanity_range` and `deathlink`.

With `deathlink` on, a death sent by another player drops your health to 0.

## Commands

Type these into the client window while it is running:

| Command | What it does |
| --- | --- |
| `items` | Shows each level (current level marked, unlocked ones in green, locked in red), your pointsanity break points for each level (struck through once reached), heart upgrades received and still needed, and spell page progress when the goal uses them |
| `hint` | Asks the server for a hint (`!hint`). `hint <item name>` hints a specific item |
| `release` | Manually sends goal completion to the server |
| `update` | Re-applies your received items to the game |
| `overlay` | Turns the in-game overlay on or off (Windows only) |
| `exit` | Shuts the client down |

## Overlay (Windows only)

Archipelago messages (items sent and received, etc.) are shown on top of the DuckStation window. Switch it on and off with the `overlay` command. On macOS it isn't available.

## Running on macOS

macOS needs two extra steps:

1. **Administrator access**: macOS only lets admin apps read another app's memory, so the client asks for your Mac password. Nothing appears as you type, that's normal.
2. **DuckStation memory address**: in DuckStation open **Tools > Memory Scanner**, find `Address of RAM for HxD Usage: 0x...` in the bottom right and paste the `0x...` address (or the whole line) into the client. It can change every time DuckStation restarts, so grab it fresh each session.

From the [releases page](https://github.com/riezahughes/PandemoniumAPClient/releases) pick `PandemoniumArchipelago_<version>_osx-arm64.zip` for Apple Silicon or `..._osx-x64.zip` for Intel Macs. The first time, right-click the client and choose **Open** (or run `xattr -d com.apple.quarantine PandemoniumArchipelago` once from Terminal) to get past Gatekeeper. Boot the game in DuckStation and reach the main menu before opening the client.

## Building

The client is built on [Archipelago.Core](https://github.com/ArchipelagoMW/Archipelago.Core) and targets .NET 8. For quick testing, create an `appsettings.Local.json` next to the project (it is git-ignored) and Debug builds will log in with it automatically:

```json
{ "port": "12345", "slot": "YourSlot", "pass": "" }
```

`AreaIds.md` has the notes on the game's area and level IDs.
