# ClientTemplate

A starting point for an [Archipelago](https://archipelago.gg) client that runs alongside a game in the DuckStation emulator. It is built on [Archipelago.Core](https://github.com/ArchipelagoMW/Archipelago.Core) **2.1** (NuGet) and targets .NET 8 on Windows and macOS. Medievil 2's client is the reference for a finished version of this template.

## What's included

| File | Purpose |
| --- | --- |
| `App.cs` | Entry point: connects to DuckStation, logs into Archipelago, registers handlers, runs the command loop. Includes the macOS sudo relaunch and DuckStation address prompt. |
| `Addresses.cs` | Memory addresses. |
| `Helpers/APHelpers.cs` | Archipelago event hooks (connected, disconnected, item received, messages, location completed). |
| `Helpers/LocationHelpers.cs` | Builds the list of locations to watch. IDs must match your APWorld. |
| `Helpers/PlayerStateHelpers.cs` | Applying received items and deathlink kills to the game. |
| `Helpers/GameStateHelpers.cs` | Goal / victory condition checks. |
| `Helpers/TrapHelpers.cs` | Trap examples. |
| `.github/workflows/release.yml` | On tag push, builds Windows and macOS (arm64 + x64) releases. |

## Using the template

1. Rename the project, solution, namespace and `GameName` (top of `App.cs`, must match your APWorld). Update the file names in `.github/workflows/release.yml` to match.
2. Fill in `Addresses.cs` and `Helpers/LocationHelpers.cs`.
3. Implement the `TODO`s in `PlayerStateHelpers` and `GameStateHelpers`.
4. Optional: create `appsettings.Local.json` (git-ignored) to auto-login in Debug builds:
   ```json
   { "port": "12345", "slot": "YourSlot", "pass": "" }
   ```

## Running on macOS

macOS needs two extra steps, both handled in `App.cs`:

- **Admin access**: macOS only lets root read another process's memory, so the client relaunches itself with `sudo` and asks for the Mac password.
- **DuckStation memory address**: Core can't find this on macOS, so the user pastes it from DuckStation's **Tools > Memory Scanner** (`Address of RAM for HxD Usage: 0x...`). It can change every time DuckStation restarts.

Mac releases are zipped (so the executable bit survives) and built on a macOS runner so they are ad-hoc signed. If you add the Core overlay, also zip `libcimgui.dylib` (add it to the `zip` line in the workflow). Note for end users: right-click > **Open** the first time, or run `xattr -d com.apple.quarantine <client>` to get past Gatekeeper. Copy the "Running on macOS" section of the Medievil 2 README into your game's README for the full player-facing instructions.

## Overlay (Windows only)

The template includes an optional in-game overlay for Archipelago messages. It is **on by default**. To remove it, set `Program.OverlayEnabled = false` at the top of `App.cs`; position, font size and colour are in the `OverlayOptions` block in `Main`. Core only provides a Windows overlay, so the flag is ignored on macOS. Players can turn it on and off while the client is running with the `overlay` command (wrapped by `Helpers/ToggleableOverlayService.cs`).
