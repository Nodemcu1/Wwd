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

## Quick Start

1. Copy `PaintballArena.cs` to your `oxide/plugins/` directory
2. Customize `PaintballArena.json` configuration with your arena positions
3. Reload the plugin: `o.reload PaintballArena`
4. Players use `/arena join <1-3> <Blue/Red>` to join

## Documentation

See [PLUGIN_README.md](PLUGIN_README.md) for comprehensive documentation including:
- Detailed feature descriptions
- Configuration guide
- Command reference
- Architecture details
- Troubleshooting

## Files

- `PaintballArena.cs` - Main plugin file
- `PaintballArena.json` - Example configuration
- `PLUGIN_README.md` - Complete documentation

## Requirements

- Rust game server
- Oxide/UMod framework
- Newtonsoft.Json (included with Oxide)

## License

Open source - use freely for Rust game servers