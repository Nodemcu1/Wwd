# Visual Sphere Setup System - Quick Reference

## What Are the Spheres?

Visual markers that appear when you set arena positions. Each color represents a different type of position.

```
🟢 Green Sphere    = Gate/Entrance
🟡 Yellow Sphere   = Spectator Position  
🔵 Blue Sphere     = Side A (Blue Team) Spawn
🔴 Red Sphere      = Side B (Red Team) Spawn
```

## Visual Example: Arena Layout

```
                  Spectator Area
                      🟡
                    (elevated)
                       |
                       |
    Side A Spawns                    Side B Spawns
    (Blue Team)                      (Red Team)
    
    🔵 🔵 🔵 🔵 🔵                  🔴 🔴 🔴 🔴 🔴
     1  2  3  4  5                    1  2  3  4  5
    
         ↑                                 ↑
         |                                 |
    Team Blue                          Team Red
    Starting                           Starting
    Positions                          Positions
         
         
              Gate (Entrance)
                   🟢
                    ↑
              Players enter here
```

## Step-by-Step Visual Setup

### Step 1: Select Arena
```
Command: /arenaadmin selectarena 1
Output:  "Selected Arena 1 for setup"
```

### Step 2: Set Gate (Where Players Enter)
```
1. Walk to entrance location
2. Command: /arenaadmin setgate
3. Result: 🟢 Green sphere appears
4. Output: "Gate position set for Arena 1 at (100.0, 0.0, 100.0)"
```

### Step 3: Set Spectator Position (Elevated View)
```
1. Walk to elevated platform (10-15 units above arena)
2. Command: /arenaadmin setspectator
3. Result: 🟡 Yellow sphere appears
4. Output: "Spectator position set for Arena 1 at (120.0, 10.0, 100.0)"
```

### Step 4: Set Side A Spawns (Blue Team)
```
For a 5v5 arena, you need 5 spawn points:

Location 1: /arenaadmin setsidea 1  → 🔵 appears
Location 2: /arenaadmin setsidea 2  → 🔵 appears
Location 3: /arenaadmin setsidea 3  → 🔵 appears
Location 4: /arenaadmin setsidea 4  → 🔵 appears
Location 5: /arenaadmin setsidea 5  → 🔵 appears

Each sphere shows where a Blue team player will spawn!
```

### Step 5: Set Side B Spawns (Red Team)
```
Walk to opposite side of arena:

Location 1: /arenaadmin setsideb 1  → 🔴 appears
Location 2: /arenaadmin setsideb 2  → 🔴 appears
Location 3: /arenaadmin setsideb 3  → 🔴 appears
Location 4: /arenaadmin setsideb 4  → 🔴 appears
Location 5: /arenaadmin setsideb 5  → 🔴 appears

Each sphere shows where a Red team player will spawn!
```

### Step 6: Review Your Setup
```
Look around your arena:

✓ 1 Green sphere (gate)
✓ 1 Yellow sphere (spectator)
✓ 5 Blue spheres (Side A spawns)
✓ 5 Red spheres (Side B spawns)

Total: 12 spheres marking your complete arena!
```

### Step 7: Save and Apply
```
Command: /arenaadmin save
Output:  "Arena configuration saved!"
         "Arena 1 Configuration:"
         "  Gate: (100.0, 0.0, 100.0)"
         "  Spectator: (120.0, 10.0, 100.0)"
         "  Side A Spawns: 5"
         "  Side B Spawns: 5"

Then reload: o.reload PaintballArena
```

## Common Setups by Arena Size

### 1v1 Arena
```
🟢 Gate
🟡 Spectator
🔵 Side A Spawn #1
🔴 Side B Spawn #1

Total: 4 spheres
```

### 2v2 Arena
```
🟢 Gate
🟡 Spectator
🔵 🔵 Side A Spawns (#1, #2)
🔴 🔴 Side B Spawns (#1, #2)

Total: 6 spheres
```

### 5v5 Arena
```
🟢 Gate
🟡 Spectator
🔵 🔵 🔵 🔵 🔵 Side A Spawns (#1-5)
🔴 🔴 🔴 🔴 🔴 Side B Spawns (#1-5)

Total: 12 spheres
```

## Sphere Management

### Clear All Spheres
```
Command: /arenaadmin clearspheres
Result:  All your sphere markers disappear
Use:     When you want to start over or reduce clutter
```

### Replace a Sphere
```
Just run the command again at the new location!

Example: If Side A Spawn #3 is in the wrong spot:
1. Walk to correct location
2. /arenaadmin setsidea 3
3. Old sphere is replaced with new one
```

## Tips for Good Arena Layout

### Distance Between Spawns
```
Side A ←-------- 100-200 units --------→ Side B

Too close: Players spawn in each other's sight
Too far: Takes too long to engage
Sweet spot: 150 units for most maps
```

### Spectator Height
```
Ground Level: 0 units (can't see over obstacles)
Good View: 10-15 units above ground
Great View: 20+ units (but may be too far)
```

### Spawn Spacing
```
Spawn #1 → 5 units → Spawn #2 → 5 units → Spawn #3

Too close: Players spawn inside each other
Too far: Team is spread out
Sweet spot: 5-10 units between spawns
```

## Troubleshooting Spheres

### "No sphere appeared!"
- Check you have admin permission
- Make sure you selected an arena first
- Server may not support sphere entities

### "Sphere is wrong color!"
- Green = Gate (correct)
- Yellow = Spectator (correct)
- Blue = Side A (correct)
- Red = Side B (correct)
- If wrong, you used the wrong command

### "Too many spheres, can't see!"
- Use `/arenaadmin clearspheres` to remove all
- Set positions one at a time
- Save frequently so you don't lose work

### "Sphere disappeared after restart"
- Spheres are temporary markers only
- They're cleaned up when plugin unloads
- Positions are saved in config file
- Reload plugin to use saved positions

## Advanced: Multiple Arenas

### Setting Up All 3 Arenas
```
Arena 1 (5v5):
/arenaadmin selectarena 1
[Set all positions with spheres]
/arenaadmin save
/arenaadmin clearspheres

Arena 2 (2v2):
/arenaadmin selectarena 2
[Set all positions with spheres]
/arenaadmin save
/arenaadmin clearspheres

Arena 3 (1v1):
/arenaadmin selectarena 3
[Set all positions with spheres]
/arenaadmin save

o.reload PaintballArena
```

## Summary: Complete Workflow

```
1. Grant Permission
   o.grant user <yourname> paintballarena.admin

2. Select Arena
   /arenaadmin selectarena 1

3. Walk and Mark
   → Walk to gate → /arenaadmin setgate → 🟢
   → Walk to spectator → /arenaadmin setspectator → 🟡
   → Walk to spawns → /arenaadmin setsidea 1-5 → 🔵🔵🔵🔵🔵
   → Walk to spawns → /arenaadmin setsideb 1-5 → 🔴🔴🔴🔴🔴

4. Verify Visually
   Look around, check all sphere positions

5. Save and Apply
   /arenaadmin save
   o.reload PaintballArena

6. Test
   /arena join 1 Blue
   Check spawn positions in-game
```

## Quick Command Reference

| What to Set | Command | Sphere Color |
|-------------|---------|--------------|
| Arena Selection | `/arenaadmin selectarena <1-3>` | - |
| Gate/Entrance | `/arenaadmin setgate` | 🟢 Green |
| Spectator View | `/arenaadmin setspectator` | 🟡 Yellow |
| Side A Spawn | `/arenaadmin setsidea <#>` | 🔵 Blue |
| Side B Spawn | `/arenaadmin setsideb <#>` | 🔴 Red |
| Clear Markers | `/arenaadmin clearspheres` | - |
| Save Config | `/arenaadmin save` | - |

**Remember**: 
- Side A = Blue Team
- Side B = Red Team
- Walk to location BEFORE running command
- Spheres are markers - actual positions saved on `/arenaadmin save`
