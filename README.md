# PaintballArena - Multi-Instance Paintball Plugin for Rust/Oxide

A comprehensive Oxide/UMod plugin for Rust game servers that enables multiple concurrent paintball arena instances with independent game states, timers, and scoring systems.

## Features

- **Concurrent Arena Management**: Run up to 3 arenas simultaneously with different game modes
- **Instance Isolation**: Each arena operates independently with its own timers, teams, and game state
- **Multiple Game Modes**: 5v5 TDM, 2v2 One in the Chamber, 1v1 One in the Chamber
- **Smart Resource Management**: Dynamic CPU throttling - inactive arenas don't consume resources
- **Player Routing**: Lobby-based gate system for arena selection
- **Combat System**: Instance-scoped damage handling with one-hit elimination
- **UI Isolation**: Per-arena scoreboards and kill feeds
- **Voice Chat Isolation**: Optional voice separation between arenas
- **Admin Setup Tools**: Visual sphere markers and in-game commands for easy arena configuration

## Quick Start

### For Players
1. Join an arena: `/arena join <1-3> <Blue/Red>`
2. Wait for match to start
3. Compete and have fun!

### For Admins
1. Copy `PaintballArena.cs` to your `oxide/plugins/` directory
2. Grant yourself admin permission: `o.grant user <yourname> paintballarena.admin`
3. Use `/adminsetup` to open the graphical admin panel
4. Click buttons to configure arena positions with visual sphere markers
5. See [ADMIN_UI_GUIDE.md](ADMIN_UI_GUIDE.md) for detailed UI instructions

## Documentation

- **[ADMIN_UI_GUIDE.md](ADMIN_UI_GUIDE.md)** - Complete UI-based setup guide (NEW!)
- **[PLUGIN_README.md](PLUGIN_README.md)** - Comprehensive plugin documentation
- **[INSTALLATION_GUIDE.md](INSTALLATION_GUIDE.md)** - Installation and configuration
- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Technical architecture details

## Files

- `PaintballArena.cs` - Main plugin file
- `PaintballArena.json` - Example configuration
- `ADMIN_SETUP_GUIDE.md` - Admin setup with visual spheres
- `PLUGIN_README.md` - Complete documentation
- `INSTALLATION_GUIDE.md` - Installation guide

## Key Commands

### Player Commands
- `/arena join <1-3> <Blue/Red>` - Join an arena and team
- `/arena leave` - Leave current arena
- `/arena status` - View arena statuses

### Admin Command (requires `paintballarena.admin` permission)
- `/adminsetup` - Open the graphical admin setup UI panel

**NEW**: No more typing commands! Open the UI and click buttons to configure arenas.

## Requirements

- Rust game server
- Oxide/UMod framework
- Newtonsoft.Json (included with Oxide)

## License

Open source - use freely for Rust game servers