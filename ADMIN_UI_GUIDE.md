# Admin Setup UI - Quick Reference Guide

## Opening the Admin Setup Panel

Simply type in chat:
```
/adminsetup
```

A graphical UI panel will open with all setup controls!

## UI Panel Overview

```
┌──────────────────────────────────────────────┐
│   PAINTBALL ARENA - ADMIN SETUP              │
├──────────────────────────────────────────────┤
│   ARENA 1 - 5v5_TDM                          │
├──────────────────────────────────────────────┤
│ SELECT ARENA:                                │
│ [Arena 1] [Arena 2] [Arena 3]                │
├──────────────────────────────────────────────┤
│ SET POSITION (stand at location first):      │
│ [Set Gate]        [Set Spectator]            │
│ [Add Side A Spawn] [Add Side B Spawn]        │
├──────────────────────────────────────────────┤
│ UTILITIES:                                   │
│ [Clear Spheres]   [Save Config]              │
├──────────────────────────────────────────────┤
│ Arena 1: Side A Spawns: 5 | Side B Spawns: 5│
│ 🟢 Gate | 🟡 Spectator | 🔵 Side A | 🔴 Side B│
│                [CLOSE]                        │
└──────────────────────────────────────────────┘
```

## Step-by-Step Setup

### 1. Open the UI
```
/adminsetup
```

### 2. Select Arena
Click one of the arena buttons:
- **Arena 1** - Configure first arena
- **Arena 2** - Configure second arena
- **Arena 3** - Configure third arena

The selected arena will be highlighted in green.

### 3. Set Positions

Walk to each location and click the appropriate button:

#### Set Gate Position
1. Walk to where you want the gate/entrance
2. Click **[Set Gate]** button
3. Green sphere (🟢) appears
4. ✓ Confirmation message

#### Set Spectator Position
1. Walk to elevated viewing position
2. Click **[Set Spectator]** button
3. Yellow sphere (🟡) appears
4. ✓ Confirmation message

#### Add Side A Spawns (Blue Team)
1. Walk to first spawn location
2. Click **[Add Side A Spawn]** button
3. Blue sphere (🔵) appears
4. ✓ Confirmation shows spawn #1
5. Walk to next location
6. Click **[Add Side A Spawn]** again
7. Another blue sphere appears
8. ✓ Confirmation shows spawn #2
9. Repeat for all Side A spawns

#### Add Side B Spawns (Red Team)
1. Walk to first spawn location
2. Click **[Add Side B Spawn]** button
3. Red sphere (🔴) appears
4. ✓ Confirmation shows spawn #1
5. Walk to next location
6. Click **[Add Side B Spawn]** again
7. Another red sphere appears
8. ✓ Confirmation shows spawn #2
9. Repeat for all Side B spawns

### 4. Save Configuration
Click **[Save Config]** button when done:
- Configuration is saved
- Message confirms save
- Shows reminder to reload plugin

### 5. Close UI
Click **[CLOSE]** button at bottom to hide the panel.

You can reopen it anytime with `/adminsetup`

## Features

### Smart Spawn Counting
- No need to specify spawn numbers
- Plugin automatically increments
- Side A Spawn #1, #2, #3, etc.
- Side B Spawn #1, #2, #3, etc.

### Real-Time Updates
- UI shows current spawn counts
- Selected arena is highlighted
- Info updates after each action

### Visual Feedback
- ✓ Checkmarks in chat for successful actions
- Colored spheres show positions
- Color legend in UI panel

### Easy Cleanup
Click **[Clear Spheres]** to remove all sphere markers if you want to start over.

## Sphere Colors

| Color | Position Type |
|-------|---------------|
| 🟢 Green | Gate/Entrance |
| 🟡 Yellow | Spectator View |
| 🔵 Blue | Side A (Blue Team) |
| 🔴 Red | Side B (Red Team) |

## Complete Example Workflow

```
1. Type: /adminsetup
   → UI opens

2. Click: [Arena 1]
   → Arena 1 selected (highlighted green)

3. Walk to gate location
4. Click: [Set Gate]
   → Green sphere appears
   → ✓ Gate position set for Arena 1

5. Walk to spectator platform
6. Click: [Set Spectator]
   → Yellow sphere appears
   → ✓ Spectator position set for Arena 1

7. Walk to first Side A spawn
8. Click: [Add Side A Spawn]
   → Blue sphere appears
   → ✓ Side A spawn #1 set for Arena 1

9. Walk to second Side A spawn
10. Click: [Add Side A Spawn]
    → Blue sphere appears
    → ✓ Side A spawn #2 set for Arena 1

11. Repeat for all Side A spawns...

12. Walk to first Side B spawn
13. Click: [Add Side B Spawn]
    → Red sphere appears
    → ✓ Side B spawn #1 set for Arena 1

14. Walk to second Side B spawn
15. Click: [Add Side B Spawn]
    → Red sphere appears
    → ✓ Side B spawn #2 set for Arena 1

16. Repeat for all Side B spawns...

17. Click: [Save Config]
    → ✓ Configuration saved!

18. In F1 console type: o.reload PaintballArena
    → Plugin reloads with new positions

19. Click: [CLOSE]
    → UI closes
```

## Tips

### Positioning Strategy
1. **Select arena first** - Always click an arena button before setting positions
2. **Walk before clicking** - Stand at the exact position, then click the button
3. **Check your view** - Make sure you're at ground level or desired height
4. **Use spheres as guides** - Previous spheres help you space out new spawns

### Managing Spawns
- **No spawn limit** - Add as many as you need for your game mode
- **Auto-numbering** - Plugin tracks count automatically
- **Visual spacing** - Use existing spheres to maintain consistent spacing

### Troubleshooting
- **Button not working?** - Make sure you have admin permission
- **No arena selected?** - The UI will show "No Arena Selected" - click an arena button
- **Wrong position?** - Use [Clear Spheres] and set positions again
- **UI disappeared?** - Type `/adminsetup` to reopen it

## Permission Required

You must have the admin permission:
```
paintballarena.admin
```

Grant it with:
```
o.grant user <yourname> paintballarena.admin
```

## Comparison: Old vs New

### Old Way (Chat Commands)
```
/arenaadmin selectarena 1
/arenaadmin setgate
/arenaadmin setspectator
/arenaadmin setsidea 1
/arenaadmin setsidea 2
/arenaadmin setsideb 1
/arenaadmin setsideb 2
/arenaadmin save
```

### New Way (UI Panel)
```
/adminsetup
→ Click [Arena 1]
→ Click [Set Gate]
→ Click [Set Spectator]
→ Click [Add Side A Spawn] (repeat as needed)
→ Click [Add Side B Spawn] (repeat as needed)
→ Click [Save Config]
→ Click [CLOSE]
```

**Much easier!** ✨

## Advanced Usage

### Multiple Arenas Setup
1. Open UI: `/adminsetup`
2. Click [Arena 1], set all positions, save
3. Click [Arena 2], set all positions, save
4. Click [Arena 3], set all positions, save
5. Close UI
6. Reload plugin

### Quick Adjustments
1. Open UI: `/adminsetup`
2. Click arena you want to adjust
3. Walk to new position
4. Click appropriate button
5. Old sphere is replaced with new one
6. Click [Save Config]
7. Reload plugin

### Reviewing Setup
Open the UI anytime to see:
- Which arena is selected
- How many Side A spawns
- How many Side B spawns
- Color legend for reference

The UI is your control panel - use it freely!
