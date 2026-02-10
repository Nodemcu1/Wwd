# PaintballArena Plugin - Multi-Instance Paintball Arena System

## Overview

PaintballArena is a comprehensive Rust/Oxide plugin that enables multiple concurrent paintball arena instances to run simultaneously on a single server. Each arena operates independently with its own game state, timers, teams, and scoring system.

## Key Features

### 1. **Concurrent Instance Management**
- Support for up to 3 simultaneous arenas running different game modes
- Example: Arena 1 running "5v5 TDM" while Arena 2 runs "1v1 One in the Chamber"
- Each arena maintains complete independence from others

### 2. **Player-to-Arena Mapping**
- Utilizes `Dictionary<ulong, ArenaInstance>` to track player locations
- All damage, chat, and gameplay hooks verify player arena membership
- Prevents cross-arena interference

### 3. **Independent Timer System**
- Each arena has its own `Timer` class instance
- Pausing/stopping timers in one arena does not affect others
- Round timers, countdown timers operate per-arena

### 4. **Lobby & Gate System**
- Central lobby hub with 3 distinct gates
- Gate A → Arena 1 Lobby
- Gate B → Arena 2 Lobby  
- Gate C → Arena 3 Lobby
- Team selection spheres (Blue/Red) route players based on gate entry

### 5. **Combat Logic (Instance-Scoped)**
- `OnEntityTakeDamage` hook implementation
- Verifies both attacker and victim are in the **same** arena
- One-hit elimination mechanic
- Teleports eliminated players to arena-specific spectator positions

### 6. **UI & Chat Isolation**
- Scoreboards display only relevant arena information
- Kill feeds broadcast only to players in the same arena
- Voice chat isolation (optional) prevents cross-arena audio

### 7. **Game Modes**

#### Mode A: 5v5 Team Deathmatch
- First team to 10 rounds wins
- 128 ammo per player
- 5-minute round timer

#### Mode B: 2v2 One in the Chamber
- First team to 5 rounds wins
- 1 bullet per player
- Ammo refund on elimination
- 3-minute round timer

#### Mode C: 1v1 One in the Chamber
- First player to 3 rounds wins
- 1 bullet per player
- Ammo refund on elimination
- 2-minute round timer

### 8. **Dynamic CPU Throttling**
- Inactive arenas (0 players) do not consume server resources
- Automatic cleanup of timers when arenas are empty
- Resource optimization via `OptimizeArenaResources()`

### 9. **Waiting for Players State**
- Minimum player requirement per arena (configurable)
- Displays "Waiting for X more players..." message
- Automatic 10-second countdown when minimum is reached

### 10. **Arena-Specific Loadouts**
- Mode-based weapon distribution
- TDM Mode: 128 ammo
- One in the Chamber: 1 ammo
- Automatic ammo refund system for Chamber modes

## Configuration

The plugin generates a configuration file with the following structure:

```json
{
  "Arena 1 Settings": {
    "Arena ID": 1,
    "Game Mode": "5v5_TDM",
    "Minimum Players To Start": 2,
    "Max Rounds": 10,
    "Round Time (Seconds)": 300,
    "Gate Position": { "x": 100.0, "y": 0.0, "z": 100.0 },
    "Spectator Position": { "x": 120.0, "y": 10.0, "z": 100.0 },
    "Team Spawns": {
      "Blue": [{ "x": 150.0, "y": 0.0, "z": 150.0 }],
      "Red": [{ "x": 50.0, "y": 0.0, "z": 50.0 }]
    }
  },
  "Arena 2 Settings": {
    "Arena ID": 2,
    "Game Mode": "2v2_Chamber",
    "Minimum Players To Start": 2,
    "Max Rounds": 5,
    "Round Time (Seconds)": 180,
    "Gate Position": { "x": 200.0, "y": 0.0, "z": 200.0 },
    "Spectator Position": { "x": 220.0, "y": 10.0, "z": 200.0 },
    "Team Spawns": {
      "Blue": [{ "x": 250.0, "y": 0.0, "z": 250.0 }],
      "Red": [{ "x": 150.0, "y": 0.0, "z": 150.0 }]
    }
  },
  "Arena 3 Settings": {
    "Arena ID": 3,
    "Game Mode": "1v1_Chamber",
    "Minimum Players To Start": 2,
    "Max Rounds": 3,
    "Round Time (Seconds)": 120,
    "Gate Position": { "x": 300.0, "y": 0.0, "z": 300.0 },
    "Spectator Position": { "x": 320.0, "y": 10.0, "z": 300.0 },
    "Team Spawns": {
      "Blue": [{ "x": 350.0, "y": 0.0, "z": 350.0 }],
      "Red": [{ "x": 250.0, "y": 0.0, "z": 250.0 }]
    }
  },
  "Global Settings": {
    "Lobby Position": { "x": 0.0, "y": 0.0, "z": 0.0 },
    "Enable Voice Isolation": true,
    "Voice Isolation Distance": 50.0
  }
}
```

## Commands

### Player Commands

- `/arena join <1-3> <Blue/Red>` - Join a specific arena and team
  - Example: `/arena join 1 Blue` joins Arena 1 as Blue Team
  
- `/arena leave` - Leave the current arena and return to lobby

- `/arena status` - View status of all arenas (state, players, scores)

### Admin Commands

Requires `paintballarena.admin` permission. Grant with:
```
o.grant user <username> paintballarena.admin
```

- `/arenaadmin` - Display admin menu with all available commands

- `/arenaadmin selectarena <1-3>` - Select which arena to configure
  - Must be done before setting positions

- `/arenaadmin setgate` - Set gate/entrance position at your current location
  - Creates a green sphere marker

- `/arenaadmin setspectator` - Set spectator viewing position at your current location
  - Creates a yellow sphere marker

- `/arenaadmin setsidea <#>` - Set Side A (Blue Team) spawn point at your current location
  - Example: `/arenaadmin setsidea 1` sets first spawn point
  - Creates a blue sphere marker

- `/arenaadmin setsideb <#>` - Set Side B (Red Team) spawn point at your current location
  - Example: `/arenaadmin setsideb 1` sets first spawn point
  - Creates a red sphere marker

- `/arenaadmin clearspheres` - Remove all sphere markers you've created

- `/arenaadmin save` - Save current configuration to file
  - Remember to reload plugin after saving: `o.reload PaintballArena`

For detailed setup instructions, see [ADMIN_SETUP_GUIDE.md](ADMIN_SETUP_GUIDE.md).

## Installation

1. Download `PaintballArena.cs`
2. Place in `oxide/plugins/` directory
3. Plugin will auto-generate configuration file on first load
4. Customize arena positions, modes, and settings in the config
5. Reload the plugin: `o.reload PaintballArena`

## Architecture Details

### ArenaInstance Class
Each arena is represented by an `ArenaInstance` object containing:
- **ArenaId**: Unique identifier (1-3)
- **Mode**: Game mode string
- **Config**: Arena-specific configuration
- **State**: Current state (WaitingForPlayers, Countdown, InProgress, Ended)
- **Teams**: Dictionary of team names to player lists
- **Score**: Team scores
- **Spectators**: List of eliminated players
- **CurrentRound**: Round counter
- **RoundTimer**: Independent timer for round duration
- **CountdownTimer**: Independent timer for match start countdown

### State Flow
1. **WaitingForPlayers**: Arena waits for minimum player count
2. **Countdown**: 10-second countdown after minimum players join
3. **InProgress**: Active gameplay with round timer
4. **Ended**: Match complete, returning players to lobby

### Combat Flow
1. `OnEntityTakeDamage` hook triggered
2. Check if both players are in arena system
3. Verify both players in **same arena instance**
4. Check teams (prevent friendly fire)
5. Execute one-hit elimination
6. Teleport victim to spectator position
7. Award point to attacker's team
8. Refund ammo (if Chamber mode)
9. Check round end conditions
10. Update UI for all arena players

## Customization

### Adding New Game Modes
1. Add mode configuration in config file
2. Update `GetAmmoForMode()` method for custom ammo amounts
3. Modify `DistributeLoadouts()` for custom weapon sets
4. Adjust scoring logic in `HandleElimination()` if needed

### Adjusting Team Sizes
- Modify `Teams` dictionary initialization in `ArenaInstance` constructor
- Add additional team colors beyond Blue/Red
- Update spawn point configurations

### Voice Isolation
- Enable/disable in Global Settings
- Adjust isolation distance
- Customize `OnPlayerVoice` hook for advanced isolation logic

## Performance Optimization

The plugin includes several optimization features:

1. **Lazy Arena Activation**: Only arenas with players consume resources
2. **Timer Cleanup**: Automatic destruction of unused timers
3. **Instance-Scoped Updates**: UI updates only affect relevant players
4. **Empty Arena Detection**: Automatic reset of empty arenas

## Troubleshooting

**Players can damage each other across arenas:**
- Verify `OnEntityTakeDamage` hook is properly checking arena instance IDs
- Ensure `playerArenaMap` is correctly maintained

**Timers affecting multiple arenas:**
- Check that each arena creates its own Timer instance
- Verify timer cleanup in `Unload()` method

**Players not spawning correctly:**
- Verify spawn positions in configuration
- Check that `TeamSpawns` dictionary contains valid Vector3 coordinates

**UI not displaying:**
- Ensure CUI dependencies are loaded
- Check that player is in `playerArenaMap`
- Verify scoreboard creation logic

## Support

For issues, suggestions, or contributions:
- Report bugs with detailed reproduction steps
- Include server logs and configuration
- Test in isolated environment before production deployment

## License

This plugin is provided as-is for use in Rust game servers running Oxide/UMod framework.

## Version History

**v1.0.0** - Initial Release
- Multi-instance arena support
- 3 concurrent arenas
- Independent timers per arena
- Combat logic with instance scoping
- UI and chat isolation
- Dynamic CPU throttling
- Arena-specific loadouts
- Waiting for players state
