# UI Fixes & Lobby Management Spheres

## Overview

This document explains the UI improvements and new lobby management sphere system added to the PaintballArena plugin.

## UI Fixes

### Height Increased

**Before**: Panel height was 60% of screen (0.2 to 0.8)
**After**: Panel height is now 80% of screen (0.1 to 0.9)

**Result**: 33% more vertical space, all buttons visible without scrolling or overlap!

### Button Reorganization

All buttons are now organized into clear sections with proper spacing:

```
┌─────────────────────────────────────────────┐
│  PAINTBALL ARENA - ADMIN SETUP              │
│  ========================================    │
│         ARENA 1 - 5v5_TDM                   │
├─────────────────────────────────────────────┤
│  SELECT ARENA:                              │
│  [Arena 1]  [Arena 2]  [Arena 3]            │
├─────────────────────────────────────────────┤
│  GLOBAL POSITIONS:                          │
│  [Set Lobby] [Return Sphere] [Leave Lobby]  │
├─────────────────────────────────────────────┤
│  PER-ARENA POSITIONS:                       │
│  [Set Gate]         [Set Spectator]         │
├─────────────────────────────────────────────┤
│  BATTLE SPAWNS:                             │
│  [Add Side A Spawn]  [Add Side B Spawn]     │
├─────────────────────────────────────────────┤
│  TEAM LOBBY SPHERES:                        │
│  [Green] [Blue] [Orange] [Yellow] [Purple]  │
├─────────────────────────────────────────────┤
│  ARENA GATE SPHERES (Lobby):                │
│  [Arena 1 Gate] [Arena 2 Gate] [3 Gate]     │
├─────────────────────────────────────────────┤
│  UTILITIES:                                 │
│  [Clear Spheres]      [Save Config]         │
│                                             │
│  Instructions and info display...           │
│              [CLOSE]                        │
└─────────────────────────────────────────────┘
```

## New Features

### 1. Return to Lobby Sphere

**Purpose**: Allows players in arenas to return to the central lobby

**Setup**:
1. Open `/adminsetup`
2. Click `[Return Sphere]` button
3. Walk to desired location (usually in or near arenas)
4. Green sphere marker appears

**Player Experience**:
- Player in arena walks into green return sphere
- Automatically teleported to central lobby
- Team selection cleared
- Queue status reset
- Can select new team and queue again

**Visual Marker**: 🟢 Green sphere

**Anti-Loop**: Only triggers if player is NOT already in lobby

### 2. Leave Lobby Sphere

**Purpose**: Allows players to completely exit the arena system

**Setup**:
1. Open `/adminsetup`
2. Click `[Leave Lobby]` button
3. Walk to exit location (outside arena area)
4. Red sphere marker appears

**Player Experience**:
- Player in lobby walks into red leave sphere
- Automatically teleported to exit position
- All player data cleared
- Completely removed from arena system
- Free to do other server activities

**Visual Marker**: 🔴 Red sphere

**Anti-Loop**: Only triggers if player IS in the lobby system

## How Anti-Loop Protection Works

### Return Sphere Logic

```
Player walks into return sphere:
├─ Check: Is player already in lobby?
│  ├─ YES → Do nothing (prevent loop)
│  └─ NO → Continue
├─ Teleport to central lobby
├─ Clear team selection
├─ Clear queue status
└─ Set state to InLobby
```

### Leave Sphere Logic

```
Player walks into leave sphere:
├─ Check: Is player in lobby system?
│  ├─ NO → Do nothing (prevent loop)
│  └─ YES → Continue
├─ Teleport to exit position
├─ Clear all player data
├─ Remove from arena system
└─ Set state to None
```

## Complete Sphere System

### In Central Lobby

**Team Selection Spheres** (5):
- 🟢 Green Team
- 🔵 Blue Team
- 🟠 Orange Team
- 🟡 Yellow Team
- 🟣 Purple Team

**Arena Gate Spheres** (3):
- ⚫ Arena 1 Gate
- ⚫ Arena 2 Gate
- ⚫ Arena 3 Gate

**Leave System**:
- 🔴 Leave Lobby Sphere

### In/Near Arenas

**Return to Lobby**:
- 🟢 Return Sphere (can be placed anywhere)

**Battle Spawns**:
- 🔵 Side A Spawn points (multiple)
- 🔴 Side B Spawn points (multiple)

**Arena Entrances**:
- 🟢 Gate positions (per arena)
- 🟡 Spectator positions (per arena)

## Player Flow Examples

### Example 1: New Player Joining

```
1. Player spawns at Central Lobby (purple marker)
2. Walks to 🔵 Blue Team sphere
3. "Team Selected: Blue"
4. Walks to ⚫ Arena 1 Gate sphere
5. "Joined queue for Arena 1"
6. Teleported to spectator area
7. When turn comes → Teleported to Side A or B spawn
8. Battle starts!
```

### Example 2: Player Switching Teams

```
1. Player in Arena 1 battle
2. Walks to 🟢 Return sphere
3. Teleported to Central Lobby
4. Walks to 🟠 Orange Team sphere
5. "Team Selected: Orange"
6. Walks to ⚫ Arena 2 Gate
7. "Joined queue for Arena 2"
```

### Example 3: Player Leaving

```
1. Player in lobby
2. Walks to 🔴 Leave Lobby sphere
3. Teleported to exit position
4. All data cleared
5. Free to explore rest of server
```

## Admin Setup Guide

### Initial Setup Checklist

- [ ] Set Central Lobby position (purple marker)
- [ ] Set 5 Team Color spheres in lobby
- [ ] Set 3 Arena Gate spheres in lobby
- [ ] Set Return sphere (in/near arenas)
- [ ] Set Leave Lobby position (exit area)
- [ ] For each arena:
  - [ ] Set Gate position (arena entrance)
  - [ ] Set Spectator position (waiting area)
  - [ ] Add Side A spawn points
  - [ ] Add Side B spawn points
- [ ] Click Save Config
- [ ] Test all spheres with players

### Recommended Layout

```
                    [Exit Area]
                        ↑
                   🔴 Leave Lobby
                        |
    ========= CENTRAL LOBBY =========
           🟣 Lobby Spawn
    
    Team Selection:
    🟢 Green  🔵 Blue  🟠 Orange  🟡 Yellow  🟣 Purple
    
    Arena Gates:
    ⚫ Arena 1 Gate    ⚫ Arena 2 Gate    ⚫ Arena 3 Gate
    
    ============ ARENAS ============
    
    Arena 1          Arena 2          Arena 3
    🟢 Return        🟢 Return        🟢 Return
    🔵 Side A        🔵 Side A        🔵 Side A
    🔴 Side B        🔴 Side B        🔴 Side B
```

## Technical Details

### New Console Commands

- `adminsetup.setreturn` - Set return to lobby sphere position
- `adminsetup.setleave` - Set leave lobby exit position

### New Config Properties

```json
{
  "Global": {
    "LobbyCentral": "Vector3(x, y, z)",
    "ReturnToLobbySphere": "Vector3(x, y, z)",
    "LeaveLobby": "Vector3(x, y, z)",
    ...
  }
}
```

### State Management

**Player States**:
- `None` - Not in system
- `InLobby` - At central lobby
- `TeamSelected` - Selected team color
- `InQueue` - Waiting for arena
- `InBattle` - Currently fighting
- `Spectating` - Watching match

**State Transitions**:
```
None → InLobby (join server/leave lobby sphere)
InLobby → TeamSelected (walk into team sphere)
TeamSelected → InQueue (walk into arena gate)
InQueue → InBattle (match starts)
InBattle → Spectating (eliminated/round end)
InBattle/Spectating → InLobby (return sphere)
InLobby → None (leave lobby sphere)
```

## Benefits

### For Admins

✅ **Taller UI** - All buttons visible, no scrolling
✅ **Clear Organization** - Sections clearly labeled
✅ **Easy Setup** - Visual sphere markers
✅ **Complete Control** - All sphere types configurable

### For Players

✅ **Return Option** - Can leave arena easily
✅ **Exit Option** - Can leave system completely
✅ **No Confusion** - Clear visual markers
✅ **No Loops** - Proper state management
✅ **Flexible** - Can change teams anytime

### For Server

✅ **Professional** - Clean, organized interface
✅ **Intuitive** - Easy to understand
✅ **Reliable** - Anti-loop protection
✅ **Complete** - All player flows covered

## Troubleshooting

### UI Still Looks Wrong

**Solution**: 
1. Reload plugin: `o.reload PaintballArena`
2. Close and reopen UI: `/adminsetup`
3. Check screen resolution

### Sphere Not Detecting

**Solution**:
1. Check sphere position is set (not Vector3.zero)
2. Verify player within 2m of sphere
3. Check player state is correct for that sphere type
4. Review console for errors

### Player Stuck in Loop

**Solution**:
- Should not happen with current anti-loop protection
- If it does, player can type `/arena leave` to reset
- Admin can use `/adminsetup` to adjust sphere positions

### Spheres Overlap

**Solution**:
1. Open `/adminsetup`
2. Reposition spheres with more spacing
3. Recommended: 5m+ between different sphere types
4. Click Save Config

## Summary

The UI has been improved with:
- **80% taller panel** for better visibility
- **7 organized sections** for clarity
- **20+ buttons** all properly spaced

New lobby management spheres:
- **Return sphere** for arena → lobby
- **Leave sphere** for lobby → exit
- **Anti-loop protection** for both

All player flows now covered with proper sphere-based navigation!

**Status: Production Ready** 🚀
