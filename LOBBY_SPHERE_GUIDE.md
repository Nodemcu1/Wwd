# Lobby Sphere Setup Guide

## Overview
The PaintballArena plugin now supports physical sphere-based navigation in the lobby. Players can walk into colored spheres to select arenas and teams, creating an immersive experience.

## Sphere Types

### 1. Lobby Spawn (Purple 🟣)
**What it is**: The main hub where players spawn when they join the server or leave an arena.

**How to set**:
1. Type `/adminsetup`
2. Walk to your lobby spawn location (central hub)
3. Click **[Set Lobby]** button
4. Purple sphere appears
5. All players will spawn/return here

**Config**: `Global Settings > Lobby Position`

### 2. Team Selection Spheres (Blue 🔵 & Red 🔴)
**What they are**: Lobby spheres that players walk into to join a specific team in a specific arena.

**How to set**:
1. Type `/adminsetup`
2. Click **[Arena 1]** to select which arena these spheres are for
3. Walk to where you want the Blue team selection sphere
4. Click **[Set Blue Team Sphere]**
5. Light blue sphere appears
6. Walk to where you want the Red team selection sphere
7. Click **[Set Red Team Sphere]**
8. Orange sphere appears
9. Repeat for Arena 2 and Arena 3

**Config**: `Arena X Settings > Team Selection Spheres > Blue/Red`

**Purpose**: 
- Players entering the Blue sphere join that arena's Blue team
- Players entering the Red sphere join that arena's Red team

### 3. Gate Position (Green 🟢)
**What it is**: The entrance/exit to the actual arena battlefield.

**How to set**:
1. Type `/adminsetup`
2. Select an arena
3. Walk to the arena entrance (inside the battlefield)
4. Click **[Set Gate]**
5. Green sphere appears

**Config**: `Arena X Settings > Gate Position`

**Purpose**: Entry/exit point for the arena

### 4. Spectator Position (Yellow 🟡)
**What it is**: Where eliminated players view the match.

**How to set**:
1. Type `/adminsetup`
2. Select an arena
3. Walk to elevated viewing position
4. Click **[Set Spectator]**
5. Yellow sphere appears

**Config**: `Arena X Settings > Spectator Position`

### 5. Side A & B Spawns (Blue 🔵 & Red 🔴)
**What they are**: Where players spawn inside the arena during matches.

**How to set**:
1. Type `/adminsetup`
2. Select an arena
3. Walk to first spawn location for Side A (Blue team)
4. Click **[Add Side A Spawn]**
5. Repeat for all Side A spawns
6. Walk to first spawn location for Side B (Red team)
7. Click **[Add Side B Spawn]**
8. Repeat for all Side B spawns

**Config**: `Arena X Settings > Team Spawns > Blue/Red`

## Complete Lobby Setup Example

### Scenario: 3-Arena Hub
You want a central lobby with 3 arena zones, each with team selection spheres.

```
                    LOBBY HUB
                       🟣
                   (Spawn Point)
                        
    Arena 1 Area    Arena 2 Area    Arena 3 Area
    🔵      🔴      🔵      🔴      🔵      🔴
   Blue    Red     Blue    Red     Blue    Red
   Team    Team    Team    Team    Team    Team
```

### Setup Steps:

**Step 1: Set Lobby Spawn**
```
/adminsetup
→ Stand in center of lobby
→ Click [Set Lobby]
→ 🟣 Purple sphere appears
```

**Step 2: Set Arena 1 Team Spheres**
```
→ Click [Arena 1]
→ Walk to left side of lobby (Arena 1 zone)
→ Position for Blue team sphere
→ Click [Set Blue Team Sphere]
→ 🔵 Light blue sphere appears
→ Walk to nearby Red team sphere location
→ Click [Set Red Team Sphere]
→ 🔴 Orange sphere appears
```

**Step 3: Set Arena 2 Team Spheres**
```
→ Click [Arena 2]
→ Walk to middle of lobby (Arena 2 zone)
→ Position for Blue team sphere
→ Click [Set Blue Team Sphere]
→ 🔵 Light blue sphere appears
→ Walk to nearby Red team sphere location
→ Click [Set Red Team Sphere]
→ 🔴 Orange sphere appears
```

**Step 4: Set Arena 3 Team Spheres**
```
→ Click [Arena 3]
→ Walk to right side of lobby (Arena 3 zone)
→ Position for Blue team sphere
→ Click [Set Blue Team Sphere]
→ 🔵 Light blue sphere appears
→ Walk to nearby Red team sphere location
→ Click [Set Red Team Sphere]
→ 🔴 Orange sphere appears
```

**Step 5: Save Configuration**
```
→ Click [Save Config]
→ Type: o.reload PaintballArena
```

## Configuration File Structure

After setup, your config will look like:

```json
{
  "Arena 1 Settings": {
    "Team Selection Spheres": {
      "Blue": { "x": 10.0, "y": 0.0, "z": 10.0 },
      "Red": { "x": 10.0, "y": 0.0, "z": -10.0 }
    },
    "Gate Position": { "x": 100.0, "y": 0.0, "z": 100.0 },
    "Spectator Position": { "x": 120.0, "y": 10.0, "z": 100.0 },
    "Team Spawns": {
      "Blue": [...],
      "Red": [...]
    }
  },
  "Arena 2 Settings": {
    "Team Selection Spheres": {
      "Blue": { "x": 20.0, "y": 0.0, "z": 10.0 },
      "Red": { "x": 20.0, "y": 0.0, "z": -10.0 }
    },
    ...
  },
  "Arena 3 Settings": {
    "Team Selection Spheres": {
      "Blue": { "x": 30.0, "y": 0.0, "z": 10.0 },
      "Red": { "x": 30.0, "y": 0.0, "z": -10.0 }
    },
    ...
  },
  "Global Settings": {
    "Lobby Position": { "x": 0.0, "y": 0.0, "z": 0.0 }
  }
}
```

## Visual Layout Suggestions

### Layout Option 1: Linear Arrangement
```
Lobby    Arena 1        Arena 2        Arena 3
  🟣      🔵  🔴         🔵  🔴         🔵  ��
         Blue Red       Blue Red       Blue Red
```

### Layout Option 2: Circular Arrangement
```
        Arena 2
         🔵  🔴
            
🟣               Arena 3
Lobby             🔵  🔴
            
        Arena 1
         🔵  🔴
```

### Layout Option 3: Grouped by Arena
```
    🟣 Lobby
    
    [Arena 1 Zone]
    🔵 Blue    🔴 Red
    
    [Arena 2 Zone]
    🔵 Blue    🔴 Red
    
    [Arena 3 Zone]
    🔵 Blue    🔴 Red
```

## Best Practices

### Spacing
- **Team spheres**: 5-10 units apart (players can easily choose)
- **Arena zones**: 15-20 units between different arena zones
- **Lobby to arenas**: 20-30 units for clear separation

### Height
- **All lobby spheres**: Same Y coordinate (ground level)
- **Lobby spawn**: Slightly elevated if possible for better view

### Colors
The plugin uses these colors automatically:
- Lobby spawn: Purple (🟣)
- Blue team: Light blue (🔵)
- Red team: Orange (🔴)
- Gate: Green (🟢)
- Spectator: Yellow (🟡)

### Signage
Consider adding signs near spheres:
- "Arena 1 - 5v5 TDM"
- "Choose Team: Blue or Red"
- "Walk into sphere to join"

## Testing Your Setup

After configuration:

1. **Test Lobby Spawn**
   - Join server, should spawn at purple sphere
   - Type `/arena leave`, should return to purple sphere

2. **Test Team Spheres** (Future Implementation)
   - Walk into Blue sphere for Arena 1
   - Should join Arena 1, Blue Team
   - Walk into Red sphere for Arena 2
   - Should join Arena 2, Red Team

3. **Test Multiple Players**
   - Have multiple players join
   - Each chooses different arena/team
   - Verify proper separation

## Current Status

✅ **Implemented:**
- Lobby position can be set
- Team selection sphere positions can be set per arena
- All positions saved to config
- Visual sphere markers appear during setup

⏳ **To Be Implemented:**
- OnPlayerTrigger hooks to detect sphere entry
- Automatic team/arena assignment when player touches sphere
- Sphere entity spawning for players to see
- Collision detection for sphere interaction

## Current Workaround

Until sphere detection is implemented, players can use commands:
```
/arena join 1 Blue    # Join Arena 1 as Blue team
/arena join 2 Red     # Join Arena 2 as Red team
/arena join 3 Blue    # Join Arena 3 as Blue team
/arena leave          # Return to lobby
```

The sphere positions you set are saved and ready for when the detection system is added!

## Summary

The lobby sphere system provides a complete framework for:
- ✅ Central lobby spawn point
- ✅ Team selection spheres per arena (Blue/Red)
- ✅ Gate positions (arena entrances)
- ✅ Spectator positions
- ✅ Match spawn points

All configurable via visual UI with colored sphere markers!
