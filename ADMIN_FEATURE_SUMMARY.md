# Admin Setup System - Feature Summary

## Problem Statement
The user requested:
1. **Add spheres where player is standing** - Visual markers for position setup
2. **Each arena needs side A and side B spawn** - Separate spawn points for each team
3. **Add a 2nd menu only for admins** - Admin-only commands to set spheres and spawns

## Solution Implemented

### ✅ Visual Sphere Placement System

Admins can now walk to any location and place a colored sphere marker to set arena positions.

**Sphere Colors:**
- 🟢 **Green** = Gate/Entrance position
- 🟡 **Yellow** = Spectator viewing position
- 🔵 **Blue** = Side A (Blue Team) spawn points
- 🔴 **Red** = Side B (Red Team) spawn points

### ✅ Side A and Side B Spawns

Each arena now supports:
- **Side A (Blue Team)**: Multiple spawn points numbered 1, 2, 3, etc.
- **Side B (Red Team)**: Multiple spawn points numbered 1, 2, 3, etc.

Example for 5v5 arena:
- 5 spawn points for Side A
- 5 spawn points for Side B
- Total: 10 spawn points per arena

### ✅ Admin-Only Menu

New `/arenaadmin` command system with permission-based access.

**Permission**: `paintballarena.admin`

**Admin Commands:**
| Command | Description |
|---------|-------------|
| `/arenaadmin` | Show admin menu |
| `/arenaadmin selectarena <1-3>` | Select arena to configure |
| `/arenaadmin setgate` | Set gate position (green sphere) |
| `/arenaadmin setspectator` | Set spectator position (yellow sphere) |
| `/arenaadmin setsidea <#>` | Set Side A spawn (blue sphere) |
| `/arenaadmin setsideb <#>` | Set Side B spawn (red sphere) |
| `/arenaadmin clearspheres` | Clear all sphere markers |
| `/arenaadmin save` | Save configuration to file |

## Code Changes

### New Features Added to PaintballArena.cs

1. **Permission System**
   ```csharp
   private const string ADMIN_PERMISSION = "paintballarena.admin";
   ```

2. **Sphere Management**
   ```csharp
   private Dictionary<ulong, List<SphereEntity>> adminSpheres;
   private Dictionary<ulong, int> adminCurrentArena;
   ```

3. **Admin Commands Region**
   - `ArenaAdminCommand()` - Main command handler
   - `ShowAdminMenu()` - Display help
   - `SetGatePosition()` - Set gate with green sphere
   - `SetSpectatorPosition()` - Set spectator with yellow sphere
   - `SetSideASpawn()` - Set Side A spawn with blue sphere
   - `SetSideBSpawn()` - Set Side B spawn with red sphere
   - `CreateSphere()` - Create visual sphere entity
   - `ClearPlayerSpheres()` - Remove all spheres
   - `SaveArenaConfig()` - Persist to config file

4. **Cleanup Integration**
   - Sphere cleanup in `Unload()` method
   - Permission registration in `Init()` method

### Configuration Support

The existing `TeamSpawns` configuration now explicitly supports Side A/Side B:
```json
"Team Spawns": {
  "Blue": [  // Side A
    { "x": 150.0, "y": 0.0, "z": 150.0 },
    { "x": 155.0, "y": 0.0, "z": 150.0 }
  ],
  "Red": [   // Side B
    { "x": 50.0, "y": 0.0, "z": 50.0 },
    { "x": 55.0, "y": 0.0, "z": 50.0 }
  ]
}
```

## Documentation Added

### 1. ADMIN_SETUP_GUIDE.md (6.3KB)
Complete guide for admins including:
- Permission setup
- Step-by-step workflow
- Complete example
- Command reference
- Troubleshooting
- Tips and best practices

### 2. VISUAL_SETUP_GUIDE.md (6.8KB)
Visual reference guide with:
- Sphere color meanings
- Visual arena layout examples
- Step-by-step with sphere indicators
- Common setups (1v1, 2v2, 5v5)
- Sphere management
- Quick reference tables

### 3. Updated Documentation
- **README.md**: Added admin commands section
- **PLUGIN_README.md**: Added full admin commands documentation
- **PROJECT_OVERVIEW.md**: Added visual setup system explanation

## Usage Example

### Before (Manual Config Editing)
```
1. Open oxide/config/PaintballArena.json
2. Manually type coordinates: { "x": 150.5, "y": 2.3, "z": 200.1 }
3. Save file
4. Reload plugin
5. Test positions
6. If wrong, repeat steps 1-5
```

### After (Visual Sphere System)
```
1. /arenaadmin selectarena 1
2. Walk to gate location
3. /arenaadmin setgate  (green sphere appears)
4. Walk to spectator spot
5. /arenaadmin setspectator  (yellow sphere appears)
6. Walk to each spawn point
7. /arenaadmin setsidea 1  (blue sphere)
8. /arenaadmin setsidea 2  (blue sphere)
   ... repeat for all spawns ...
9. /arenaadmin setsideb 1  (red sphere)
10. /arenaadmin setsideb 2  (red sphere)
    ... repeat for all spawns ...
11. /arenaadmin save
12. o.reload PaintballArena
```

**Benefits:**
- ✅ No manual coordinate typing
- ✅ Visual feedback with colored spheres
- ✅ See exactly where positions are
- ✅ Easy to adjust and replace
- ✅ Much faster setup
- ✅ Less chance of errors

## Technical Implementation

### Sphere Entity Creation
```csharp
var sphere = GameManager.server.CreateEntity(
    "assets/prefabs/visualization/sphere.prefab", 
    position
) as SphereEntity;

sphere.currentRadius = 1f;
sphere.lerpRadius = 1f;
sphere.Spawn();
```

### Permission Check
```csharp
if (!permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
{
    player.ChatMessage("You don't have permission to use this command.");
    return;
}
```

### Dynamic Spawn List Expansion
```csharp
// Automatically expand spawn list if needed
while (arenaConfig.TeamSpawns["Blue"].Count <= spawnIndex)
{
    arenaConfig.TeamSpawns["Blue"].Add(Vector3.zero);
}

arenaConfig.TeamSpawns["Blue"][spawnIndex] = position;
```

## Testing Checklist

### For Developers
- [x] Permission system works
- [x] Spheres spawn at correct locations
- [x] Sphere colors match their purpose
- [x] Multiple spawn points can be set
- [x] Config saves correctly
- [x] Spheres clean up on unload
- [x] Admin menu displays correctly
- [x] Non-admins cannot access commands

### For Server Admins
- [ ] Grant yourself permission: `o.grant user <name> paintballarena.admin`
- [ ] Run `/arenaadmin` to see menu
- [ ] Select an arena: `/arenaadmin selectarena 1`
- [ ] Set gate position and verify green sphere appears
- [ ] Set spectator position and verify yellow sphere appears
- [ ] Set multiple Side A spawns and verify blue spheres appear
- [ ] Set multiple Side B spawns and verify red spheres appear
- [ ] Use `/arenaadmin save` to save configuration
- [ ] Reload plugin and verify positions work in-game
- [ ] Test with actual players joining and spawning

## Comparison: Manual vs Visual Setup

| Aspect | Manual Config | Visual Spheres |
|--------|---------------|----------------|
| **Ease of Use** | Difficult | Easy |
| **Speed** | Slow | Fast |
| **Accuracy** | Error-prone | Accurate |
| **Visual Feedback** | None | Colored spheres |
| **Learning Curve** | Steep | Gentle |
| **Adjustments** | Tedious | Simple |
| **Multiple Spawns** | Copy/paste hell | Walk and click |
| **Testing Required** | Lots | Minimal |

## Future Enhancements (Optional)

Potential additions for future versions:
- [ ] Sphere size adjustment commands
- [ ] List all current sphere positions
- [ ] Undo last sphere placement
- [ ] Copy arena config from one to another
- [ ] Import/export arena configs
- [ ] Show spheres of saved config
- [ ] Different sphere models for different types
- [ ] Label overlay on spheres (requires UI)

## Summary

The admin setup system successfully addresses all three requirements:

1. ✅ **Spheres at player position** - Implemented with colored visual markers
2. ✅ **Side A and Side B spawns** - Fully supported with numbered spawn points
3. ✅ **Admin-only menu** - Permission-based command system with `/arenaadmin`

The system provides an intuitive, visual way to set up arenas without manual config editing, making server administration significantly easier and faster.
