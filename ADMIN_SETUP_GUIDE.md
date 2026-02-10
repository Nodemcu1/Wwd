# PaintballArena Admin Setup Guide

## Admin Permission

To use admin commands, you must have the `paintballarena.admin` permission.

### Granting Admin Permission

As a server administrator, grant the permission to a player:
```
o.grant user <playername> paintballarena.admin
```

Or grant to a group:
```
o.grant group <groupname> paintballarena.admin
```

## Admin Setup Workflow

### Step 1: Select an Arena
Before setting positions, select which arena (1, 2, or 3) you want to configure:

```
/arenaadmin selectarena 1
```

This tells the system which arena you're currently setting up.

### Step 2: Set Positions Using Spheres

Walk to each location where you want to place a spawn point, gate, or spectator position, then use the appropriate command.

#### Set Gate Position
The gate is where players enter the arena from the lobby.

1. Stand at the desired gate location
2. Run: `/arenaadmin setgate`
3. A **green sphere** will appear, marking the position

#### Set Spectator Position
The spectator position is where eliminated players watch the match.

1. Stand at the desired spectator location (usually elevated)
2. Run: `/arenaadmin setspectator`
3. A **yellow sphere** will appear, marking the position

#### Set Side A Spawns (Blue Team)
Side A is the Blue Team spawn points. You can set multiple spawn points numbered 1, 2, 3, etc.

1. Stand at the first spawn location for Side A
2. Run: `/arenaadmin setsidea 1`
3. A **blue sphere** will appear
4. Move to the next spawn location
5. Run: `/arenaadmin setsidea 2`
6. Repeat for as many spawn points as needed

**Example for 5v5 arena:**
```
/arenaadmin setsidea 1
/arenaadmin setsidea 2
/arenaadmin setsidea 3
/arenaadmin setsidea 4
/arenaadmin setsidea 5
```

#### Set Side B Spawns (Red Team)
Side B is the Red Team spawn points. Similar to Side A.

1. Stand at the first spawn location for Side B
2. Run: `/arenaadmin setsideb 1`
3. A **red sphere** will appear
4. Move to the next spawn location
5. Run: `/arenaadmin setsideb 2`
6. Repeat for as many spawn points as needed

### Step 3: Review Your Setup

Look around at all the colored spheres:
- **Green** = Gate
- **Yellow** = Spectator
- **Blue** = Side A (Blue Team) spawns
- **Red** = Side B (Red Team) spawns

If you made a mistake:
- You can re-run the command at the correct position (it will overwrite)
- Or use `/arenaadmin clearspheres` to remove all spheres and start over

### Step 4: Save Configuration

Once you're happy with the positions:

```
/arenaadmin save
```

This saves all positions to the configuration file.

### Step 5: Apply Changes

Reload the plugin to apply the new configuration:

```
o.reload PaintballArena
```

## Complete Example: Setting Up Arena 1

```
# Select the arena
/arenaadmin selectarena 1

# Walk to gate location
/arenaadmin setgate

# Walk to elevated spectator position
/arenaadmin setspectator

# Set 5 spawn points for Side A (Blue)
# Walk to each location before running the command
/arenaadmin setsidea 1
/arenaadmin setsidea 2
/arenaadmin setsidea 3
/arenaadmin setsidea 4
/arenaadmin setsidea 5

# Set 5 spawn points for Side B (Red)
# Walk to each location before running the command
/arenaadmin setsideb 1
/arenaadmin setsideb 2
/arenaadmin setsideb 3
/arenaadmin setsideb 4
/arenaadmin setsideb 5

# Review the colored spheres
# If satisfied, save
/arenaadmin save

# Apply changes
o.reload PaintballArena
```

## All Admin Commands

| Command | Description |
|---------|-------------|
| `/arenaadmin` | Show the admin menu |
| `/arenaadmin selectarena <1-3>` | Select arena to configure |
| `/arenaadmin setgate` | Set gate position at your location |
| `/arenaadmin setspectator` | Set spectator position at your location |
| `/arenaadmin setsidea <#>` | Set Side A (Blue) spawn point |
| `/arenaadmin setsideb <#>` | Set Side B (Red) spawn point |
| `/arenaadmin clearspheres` | Remove all sphere markers |
| `/arenaadmin save` | Save configuration |

## Sphere Colors Reference

| Color | Meaning |
|-------|---------|
| 🟢 Green | Gate/Entrance position |
| 🟡 Yellow | Spectator viewing position |
| 🔵 Blue | Side A (Blue Team) spawn points |
| 🔴 Red | Side B (Red Team) spawn points |

## Tips

1. **Plan your arena layout first** - Sketch it out before setting positions
2. **Elevate spectators** - Place them 10-15 units above ground for a good view
3. **Space out spawns** - Give each spawn point 5-10 units of separation
4. **Test distances** - Make sure teams spawn far enough apart
5. **Clear spheres often** - Use `/arenaadmin clearspheres` to reduce clutter while setting up
6. **Save frequently** - Use `/arenaadmin save` after each major change

## Troubleshooting

**Spheres not appearing?**
- Make sure you have admin permission
- Check that you've selected an arena first
- The sphere entity may not be available on all servers

**Can't see spheres?**
- Spheres are semi-transparent (50% opacity)
- Try adjusting your graphics settings
- Look for the colored glow effect

**Positions not saving?**
- Make sure you run `/arenaadmin save`
- Check the console for errors
- Verify the config file was updated in `oxide/config/PaintballArena.json`

**Changes not applying?**
- You must reload the plugin: `o.reload PaintballArena`
- Check for syntax errors in the config file

## Advanced: Editing Config Manually

If you prefer to edit the configuration file directly, you can find it at:
```
oxide/config/PaintballArena.json
```

Example structure:
```json
{
  "Arena 1 Settings": {
    "Gate Position": { "x": 100.0, "y": 0.0, "z": 100.0 },
    "Spectator Position": { "x": 120.0, "y": 10.0, "z": 100.0 },
    "Team Spawns": {
      "Blue": [
        { "x": 150.0, "y": 0.0, "z": 150.0 },
        { "x": 155.0, "y": 0.0, "z": 150.0 }
      ],
      "Red": [
        { "x": 50.0, "y": 0.0, "z": 50.0 },
        { "x": 55.0, "y": 0.0, "z": 50.0 }
      ]
    }
  }
}
```

Note: "Blue" = Side A, "Red" = Side B

## Quick Reference

**To set up a new arena:**
1. `/arenaadmin selectarena <#>` - Choose arena
2. Stand at each position and run the appropriate command
3. `/arenaadmin save` - Save changes
4. `o.reload PaintballArena` - Apply changes

**Visual Markers:**
- Walk around to see all colored spheres
- Use `/arenaadmin clearspheres` to remove markers
- Spheres are automatically cleaned up when plugin unloads
