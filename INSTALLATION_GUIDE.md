# PaintballArena - Installation & Setup Guide

## Quick Start (5 Minutes)

### Step 1: Install the Plugin
1. Download `PaintballArena.cs` from this repository
2. Place it in your server's `oxide/plugins/` folder
3. The plugin will auto-load and generate a default configuration

### Step 2: Initial Configuration
1. Stop your server (or use `o.reload PaintballArena`)
2. Open `oxide/config/PaintballArena.json`
3. Update the positions for your server:

```json
{
  "Arena 1 Settings": {
    "Gate Position": { "x": 100.0, "y": 0.0, "z": 100.0 },
    "Spectator Position": { "x": 120.0, "y": 10.0, "z": 100.0 },
    "Team Spawns": {
      "Blue": [{ "x": 150.0, "y": 0.0, "z": 150.0 }],
      "Red": [{ "x": 50.0, "y": 0.0, "z": 50.0 }]
    }
  }
}
```

### Step 3: Build Your Arenas
1. Create 3 separate arena areas on your map
2. For each arena, note the coordinates for:
   - **Gate position** (lobby entrance to that arena)
   - **Spectator position** (where eliminated players view from)
   - **Team spawn points** (where Blue/Red teams spawn)

### Step 4: Test
1. Reload the plugin: `o.reload PaintballArena`
2. Join an arena: `/arena join 1 Blue`
3. Get another player to join: `/arena join 1 Red`
4. Match should auto-start when minimum players is reached!

## Position Setup Guide

### Getting Coordinates in Rust
1. Enable debug mode: `debug.enabled true`
2. Look at ground where you want a position
3. Press F1 and type: `status`
4. Copy your position coordinates
5. Add to config file

### Recommended Arena Layout

#### Small Arena (1v1)
- 50-100 meters between team spawns
- Spectator position elevated by 10-15 units
- Simple obstacle layout

#### Medium Arena (2v2)
- 100-150 meters between spawns
- Multiple cover positions
- Spectator position at arena center, elevated

#### Large Arena (5v5)
- 150-200 meters between spawns
- Complex multi-level design
- Multiple spectator positions (configure multiple spawn points)

## Configuration Tips

### Adjusting Game Modes

**Make Arena 1 a 3v3 Mode:**
```json
"Arena 1 Settings": {
  "Game Mode": "3v3_TDM",
  "Minimum Players To Start": 6,
  "Max Rounds": 7
}
```

**Create a "Practice Mode" (no time limit):**
```json
"Arena 2 Settings": {
  "Game Mode": "Practice",
  "Round Time (Seconds)": 99999,
  "Max Rounds": 1
}
```

### Setting Up Spawn Points

For 5v5, add multiple spawn points so players don't spawn on top of each other:

```json
"Team Spawns": {
  "Blue": [
    { "x": 150.0, "y": 0.0, "z": 150.0 },
    { "x": 155.0, "y": 0.0, "z": 150.0 },
    { "x": 160.0, "y": 0.0, "z": 150.0 },
    { "x": 150.0, "y": 0.0, "z": 155.0 },
    { "x": 150.0, "y": 0.0, "z": 160.0 }
  ]
}
```

The plugin will cycle through spawn points (player 1 → spawn 1, player 2 → spawn 2, etc.)

## Server Commands

### Admin Commands
```
o.reload PaintballArena          - Reload plugin and config
o.unload PaintballArena          - Unload plugin (stops all matches)
o.grant user <name> admin        - Give admin access (if permissions added)
```

### Player Commands
```
/arena join 1 Blue              - Join Arena 1 as Blue Team
/arena join 2 Red               - Join Arena 2 as Red Team
/arena leave                    - Leave current arena
/arena status                   - See status of all arenas
```

## Common Issues & Solutions

### Issue: "Match won't start even with enough players"
**Solution**: Check the `Minimum Players To Start` setting. For 5v5, set it to at least 2 for testing, or 10 for full matches.

### Issue: "Players are spawning in the air/underground"
**Solution**: Your Y coordinates might be wrong. Use F1 → `status` while standing at ground level to get correct Y value.

### Issue: "UI scoreboard not showing"
**Solution**: Ensure CUI dependencies are loaded. Check F1 console for errors. Try `/arena leave` then `/arena join` again.

### Issue: "Players from different arenas can hit each other"
**Solution**: This should not happen if the plugin is working correctly. Check that both players are properly registered in `playerArenaMap` by checking `/arena status`.

### Issue: "Timers are pausing both arenas"
**Solution**: This indicates a bug. Each arena should have independent timers. Reload the plugin and report the issue.

## Performance Tuning

### For Large Servers (100+ players)
- Set shorter round times to keep matches moving
- Enable voice isolation: `"Enable Voice Isolation": true`
- Consider running only 1-2 arenas if CPU usage is high

### For Small Servers (10-20 players)
- Lower minimum player requirements: `"Minimum Players To Start": 2`
- Use longer round times: `"Round Time (Seconds)": 600`
- Keep all 3 arenas available for variety

## Advanced Customization

### Adding New Weapons
Edit the `GiveLoadout()` method around line 303:
```csharp
var weapon = ItemManager.CreateByName("pistol.semiauto", 1);
```
Change `"pistol.semiauto"` to any Rust item name:
- `"rifle.ak"` - AK47
- `"rifle.bolt"` - Bolt Action
- `"bow.hunting"` - Hunting Bow
- `"shotgun.pump"` - Pump Shotgun

### Changing Ammo Types
Update the `GiveLoadout()` method:
```csharp
var ammo = ItemManager.CreateByName("ammo.pistol", ammoAmount);
```
Change `"ammo.pistol"` to match your weapon:
- `"ammo.rifle"` - Rifle ammo
- `"ammo.shotgun"` - Shotgun shells
- `"arrow.wooden"` - Arrows

## Support & Troubleshooting

### Checking Plugin Status
1. F1 Console
2. Type: `o.plugins`
3. Look for "PaintballArena v1.0.0"

### Viewing Logs
1. Navigate to `oxide/logs/`
2. Open latest log file
3. Search for "PaintballArena"

### Debug Mode
Enable additional logging by adding prints to the code or using:
```
oxide.show
```

## Building Your First Arena: Step-by-Step

### Example: Creating Arena 1 (5v5 TDM)

1. **Choose Location**: Find a 200x200 flat area on your map

2. **Set Lobby Gate**: 
   - Stand at the entrance
   - F1 → `status` → note position
   - Add to config as "Gate Position"

3. **Build Blue Team Spawn Area**:
   - Create a spawn platform/area
   - Add 5 spawn points in a line
   - Note coordinates for each

4. **Build Red Team Spawn Area**:
   - Create opposite team's spawn (150-200m away)
   - Add 5 spawn points
   - Note coordinates

5. **Set Spectator View**:
   - Build an elevated platform (10-15m high)
   - Position it to overlook the arena
   - Note coordinates

6. **Add to Config**:
   - Update `PaintballArena.json`
   - Test with `/arena join 1 Blue`

7. **Decorate**:
   - Add cover (barrels, walls, etc.)
   - Create lanes and chokepoints
   - Test gameplay flow

## Next Steps

- Read [PLUGIN_README.md](PLUGIN_README.md) for detailed documentation
- Check [REQUIREMENTS_VERIFICATION.md](REQUIREMENTS_VERIFICATION.md) to see all features
- Join our community for support and updates
- Share your arena designs with others!

## Credits

Developed for Rust/Oxide servers with concurrent multi-instance arena support.
