# 5-Team System Guide - Matching Paintball Gun Colors

## Overview

The PaintballArena plugin now supports **5 teams** to match the 5 paintball colors available in the paintball gun system:

- 🟢 **Green** - Green paintballs
- 🔵 **Blue** - Blue paintballs  
- 🟠 **Orange** - Orange paintballs
- 🟡 **Yellow** - Yellow paintballs
- 🟣 **Purple** - Purple paintballs

This matches the real Rust paintball gun mechanics where players can load 5 different colored paintballs that change their suit color.

## What Changed

### Before (2 Teams)
- Blue Team (Side A)
- Red Team (Side B)

### After (5 Teams)
- Green Team
- Blue Team
- Orange Team
- Yellow Team
- Purple Team

## Configuration

### Team Spawn Points

Each arena now has 5 sets of spawn points, one for each team:

```json
"Team Spawns": {
  "Green": [{ "x": 150.0, "y": 0.0, "z": 150.0 }],
  "Blue": [{ "x": 140.0, "y": 0.0, "z": 150.0 }],
  "Orange": [{ "x": 60.0, "y": 0.0, "z": 50.0 }],
  "Yellow": [{ "x": 50.0, "y": 0.0, "z": 50.0 }],
  "Purple": [{ "x": 100.0, "y": 0.0, "z": 100.0 }]
}
```

### Team Selection Spheres (Lobby)

Each arena has 5 lobby spheres for team selection:

```json
"Team Selection Spheres": {
  "Green": { "x": 5.0, "y": 0.0, "z": 15.0 },
  "Blue": { "x": 10.0, "y": 0.0, "z": 10.0 },
  "Orange": { "x": 15.0, "y": 0.0, "z": 0.0 },
  "Yellow": { "x": 10.0, "y": 0.0, "z": -10.0 },
  "Purple": { "x": 5.0, "y": 0.0, "z": -15.0 }
}
```

## Admin Setup UI

### New Battle Spawn Buttons

The admin UI now has 5 buttons for setting spawn points:

```
[Add Green Spawn]  [Add Blue Spawn]  [Add Orange Spawn]  [Add Yellow Spawn]  [Purple]
```

Colors:
- 🟢 Green button: Dark green background
- 🔵 Blue button: Dark blue background
- 🟠 Orange button: Orange/brown background
- 🟡 Yellow button: Yellow background
- 🟣 Purple button: Purple background

### New Lobby Sphere Buttons

```
TEAM LOBBY SPHERES:
[Green Sphere]  [Blue Sphere]  [Orange Sphere]  [Yellow Sphere]  [Purple]
```

### Spawn Counter Display

The UI now shows spawn counts for all 5 teams:

```
Arena 1 | 🟢:2 🔵:3 🟠:2 🟡:1 🟣:1
```

## Player Commands

### Join Command

Updated to accept all 5 team names:

```bash
/arena join <1-3> <Green/Blue/Orange/Yellow/Purple>
```

**Examples:**
```bash
/arena join 1 Green     # Join Arena 1 as Green team
/arena join 2 Blue      # Join Arena 2 as Blue team
/arena join 1 Orange    # Join Arena 1 as Orange team
/arena join 3 Yellow    # Join Arena 3 as Yellow team
/arena join 2 Purple    # Join Arena 2 as Purple team
```

### Help Command

```
Available commands:
/arena join <1-3> <Green/Blue/Orange/Yellow/Purple> - Join an arena and team
/arena leave - Leave current arena
/arena status - View status of all arenas
```

## Admin Console Commands

### Setting Lobby Team Selection Spheres

These set the positions in the lobby where players walk to select a team:

```
adminsetup.setlobbygreen   - Set Green team lobby sphere
adminsetup.setlobbyblue    - Set Blue team lobby sphere
adminsetup.setlobbyorange  - Set Orange team lobby sphere
adminsetup.setlobbyyellow  - Set Yellow team lobby sphere
adminsetup.setlobbypurple  - Set Purple team lobby sphere
```

### Setting Battle Spawn Points

These set spawn positions inside the arena for each team:

```
adminsetup.setteamgreen    - Add Green team spawn point
adminsetup.setteamblue     - Add Blue team spawn point
adminsetup.setteamorange   - Add Orange team spawn point
adminsetup.setteamyellow   - Add Yellow team spawn point
adminsetup.setteampurple   - Add Purple team spawn point
```

## Team Colors Reference

The plugin uses these RGB colors for visual markers:

| Team | Color Code | RGB Values | Hex | Visual |
|------|-----------|------------|-----|--------|
| Green | 🟢 | (0, 0.8, 0) | #00CC00 | Bright green |
| Blue | 🔵 | (0, 0.3, 1) | #004DFF | Sky blue |
| Orange | 🟠 | (1, 0.5, 0) | #FF8000 | Orange |
| Yellow | 🟡 | (1, 1, 0) | #FFFF00 | Yellow |
| Purple | 🟣 | (0.6, 0, 0.8) | #9900CC | Purple |

## Setup Workflow

### Complete 5-Team Arena Setup

**1. Open Admin UI:**
```
/adminsetup
```

**2. Select Arena:**
Click [Arena 1], [Arena 2], or [Arena 3]

**3. Set Lobby Team Selection Spheres:**
For each team color, walk to the lobby position and click the corresponding button:

```
→ Walk to Green sphere location
→ Click [Green Sphere]
→ 🟢 Green sphere marker appears

→ Walk to Blue sphere location
→ Click [Blue Sphere]
→ 🔵 Blue sphere marker appears

→ Walk to Orange sphere location
→ Click [Orange Sphere]
→ 🟠 Orange sphere marker appears

→ Walk to Yellow sphere location
→ Click [Yellow Sphere]
→ 🟡 Yellow sphere marker appears

→ Walk to Purple sphere location
→ Click [Purple]
→ 🟣 Purple sphere marker appears
```

**4. Set Battle Spawn Points:**
Walk to each spawn location and click the corresponding team button:

```
→ Walk to first Green spawn
→ Click [Add Green Spawn]
→ Walk to second Green spawn
→ Click [Add Green Spawn]
→ Repeat for all Green spawns

→ Walk to first Blue spawn
→ Click [Add Blue Spawn]
→ Repeat for all Blue spawns

(Continue for Orange, Yellow, Purple)
```

**5. Save Configuration:**
```
→ Click [Save Config]
→ Type: o.reload PaintballArena
```

## Suggested Lobby Layout

### Option 1: Linear Arrangement

```
         LOBBY
           🟣
           
   🟢      🔵      🟠      🟡      🟣
  Green   Blue   Orange  Yellow  Purple
```

### Option 2: Circular Arrangement

```
          🟢 Green
             
    🟣                🔵
  Purple     🟣    Blue
           Lobby
    🟡                🟠
  Yellow            Orange
```

### Option 3: Team Zones (3 Arenas)

```
        🟣 MAIN LOBBY

ARENA 1 ZONE          ARENA 2 ZONE          ARENA 3 ZONE
🟢 🔵 🟠 🟡 🟣        🟢 🔵 🟠 🟡 🟣        🟢 🔵 🟠 🟡 🟣
```

## Game Mode Considerations

### 5v5 TDM (Arena 1)
- 5 players per team = 25 total players max
- OR mixed teams: 2 Green + 2 Blue + 1 Orange vs others
- Flexible team compositions

### 2v2 (Arena 2)
- 2 players per team = 10 total players max
- Could be: Green vs Blue vs Orange vs Yellow vs Purple

### 1v1 (Arena 3)
- 1 player per team = 5 total players
- Free-for-all with 5 different colored teams

## Benefits of 5-Team System

### Matches Game Mechanics
- ✅ Paintball gun has 5 colors
- ✅ Suit changes to match paintball color
- ✅ Plugin now supports all 5 colors

### Gameplay Variety
- ✅ More team combinations possible
- ✅ Free-for-all with 5 teams
- ✅ Asymmetric team battles (2v2v1)
- ✅ Alliance gameplay
- ✅ Color-coded team identification

### Visual Clarity
- ✅ Each team has distinct color
- ✅ Lobby spheres match team colors
- ✅ Spawn markers match team colors
- ✅ Easy to identify friend vs foe

## Migration from 2-Team System

### For Server Admins

If you're upgrading from the old 2-team system:

**Old System:**
- Blue Team (Side A)
- Red Team (Side B)

**New System:**
- Keep Blue team (existing data migrated)
- Red team removed
- Add: Green, Orange, Yellow, Purple

**What to Do:**
1. Delete old config file
2. Let plugin regenerate with new 5-team structure
3. Use admin UI to reconfigure positions
4. Test with players joining different teams

### Backwards Compatibility

Old commands still work but redirect:
- `SetSideASpawn()` → Maps to Blue team
- `SetSideBSpawn()` → Maps to Red team (deprecated)

## Example Game Scenarios

### Scenario 1: Classic 5v5
- Green Team: 5 players
- Blue Team: 5 players
- Other teams empty

### Scenario 2: 3-Way Battle
- Green Team: 3 players
- Blue Team: 3 players
- Orange Team: 3 players

### Scenario 3: Free-For-All
- Green: 1 player
- Blue: 1 player
- Orange: 1 player
- Yellow: 1 player
- Purple: 1 player

### Scenario 4: Asymmetric
- Green + Blue alliance: 6 players
- Orange + Yellow + Purple alliance: 4 players

## Technical Details

### Color Code Function

```csharp
private Color GetTeamColor(string team)
{
    switch (team)
    {
        case "Green": return new Color(0f, 0.8f, 0f, 0.5f);
        case "Blue": return new Color(0f, 0.3f, 1f, 0.5f);
        case "Orange": return new Color(1f, 0.5f, 0f, 0.5f);
        case "Yellow": return new Color(1f, 1f, 0f, 0.5f);
        case "Purple": return new Color(0.6f, 0f, 0.8f, 0.5f);
        default: return new Color(0.5f, 0.5f, 0.5f, 0.5f);
    }
}
```

### Validation

```csharp
string[] validTeams = { "Green", "Blue", "Orange", "Yellow", "Purple" };
if (!validTeams.Contains(team))
{
    player.ChatMessage("Invalid team. Choose: Green, Blue, Orange, Yellow, or Purple");
    return;
}
```

## Summary

The PaintballArena plugin now fully supports the 5-color paintball system available in Rust:

✅ **5 Teams**: Green, Blue, Orange, Yellow, Purple
✅ **5 Lobby Spheres**: Per arena for team selection
✅ **5 Spawn Sets**: Per arena for battle positions
✅ **Color Matching**: Plugin colors match paintball colors
✅ **Admin UI**: Easy setup with color-coded buttons
✅ **Player Commands**: Simple `/arena join <arena> <color>` format

**Players can now join any team color that matches their paintball choice!** 🎨🎯
