# Admin Setup UI - Visual Preview

## What the UI Looks Like

When you type `/adminsetup`, a panel appears in the center of your screen:

```
╔══════════════════════════════════════════════════════════╗
║  PAINTBALL ARENA - ADMIN SETUP                           ║
╠══════════════════════════════════════════════════════════╣
║                                                          ║
║              ARENA 1 - 5v5_TDM                          ║
║                                                          ║
╠══════════════════════════════════════════════════════════╣
║  SELECT ARENA:                                           ║
║                                                          ║
║  ┌─────────┐  ┌─────────┐  ┌─────────┐                 ║
║  │Arena 1  │  │Arena 2  │  │Arena 3  │                 ║
║  │ (GREEN) │  │         │  │         │                 ║
║  └─────────┘  └─────────┘  └─────────┘                 ║
║    ^Selected                                             ║
╠══════════════════════════════════════════════════════════╣
║  SET POSITION (stand at location first):                 ║
║                                                          ║
║  ┌────────────────┐  ┌────────────────┐                 ║
║  │   Set Gate     │  │ Set Spectator  │                 ║
║  │   (GREEN)      │  │   (YELLOW)     │                 ║
║  └────────────────┘  └────────────────┘                 ║
║                                                          ║
║  ┌────────────────┐  ┌────────────────┐                 ║
║  │Add Side A Spawn│  │Add Side B Spawn│                 ║
║  │    (BLUE)      │  │     (RED)      │                 ║
║  └────────────────┘  └────────────────┘                 ║
║                                                          ║
╠══════════════════════════════════════════════════════════╣
║  UTILITIES:                                              ║
║                                                          ║
║  ┌────────────────┐  ┌────────────────┐                 ║
║  │ Clear Spheres  │  │  Save Config   │                 ║
║  │   (ORANGE)     │  │    (GREEN)     │                 ║
║  └────────────────┘  └────────────────┘                 ║
║                                                          ║
╠══════════════════════════════════════════════════════════╣
║                                                          ║
║  Arena 1: Side A Spawns: 5 | Side B Spawns: 5          ║
║                                                          ║
║  🟢 Gate | 🟡 Spectator | 🔵 Side A | 🔴 Side B        ║
║                                                          ║
║                  ┌──────────┐                            ║
║                  │  CLOSE   │                            ║
║                  └──────────┘                            ║
║                                                          ║
╚══════════════════════════════════════════════════════════╝
```

## UI Elements Explained

### Header
- **Title**: "PAINTBALL ARENA - ADMIN SETUP"
- **Current Arena**: Shows which arena is selected and its mode

### Arena Selection (3 Buttons)
- **Arena 1**: Click to configure Arena 1
- **Arena 2**: Click to configure Arena 2
- **Arena 3**: Click to configure Arena 3
- Selected arena is **highlighted in green**

### Position Setup (4 Buttons)
1. **Set Gate** (Green button)
   - Sets gate/entrance position
   - Creates green sphere

2. **Set Spectator** (Yellow button)
   - Sets spectator viewing position
   - Creates yellow sphere

3. **Add Side A Spawn** (Blue button)
   - Adds a Side A (Blue Team) spawn point
   - Creates blue sphere
   - Auto-increments number

4. **Add Side B Spawn** (Red button)
   - Adds a Side B (Red Team) spawn point
   - Creates red sphere
   - Auto-increments number

### Utilities (2 Buttons)
1. **Clear Spheres** (Orange button)
   - Removes all sphere markers
   - Clean slate for repositioning

2. **Save Config** (Green button)
   - Saves all positions to config file
   - Shows success message

### Info Display
- **Spawn Counts**: Shows how many spawns are configured
- **Color Legend**: Quick reference for sphere colors

### Close Button
- **CLOSE**: Hides the UI panel
- Can reopen anytime with `/adminsetup`

## Color Scheme

The UI uses a dark semi-transparent background:
- **Background**: Dark gray (90% opacity)
- **Title**: White text
- **Arena info**: Yellow/gold text
- **Section headers**: Light gray text
- **Buttons**: Colored based on function
- **Text**: White for readability

## Button Colors

| Button | Background Color | Purpose |
|--------|------------------|---------|
| **Arena 1/2/3** | Gray (Green when selected) | Arena selection |
| **Set Gate** | Dark green | Gate position |
| **Set Spectator** | Dark yellow | Spectator position |
| **Add Side A** | Dark blue | Side A spawns |
| **Add Side B** | Dark red | Side B spawns |
| **Clear Spheres** | Brown/orange | Utility |
| **Save Config** | Dark green | Utility |
| **CLOSE** | Dark red | Close UI |

## Interaction Flow

```
Player opens UI
    ↓
Clicks Arena 1
    ↓
Arena 1 highlights in green
Info updates: "ARENA 1 - 5v5_TDM"
    ↓
Player walks to gate location
    ↓
Clicks "Set Gate" button
    ↓
Green sphere appears at player position
Chat message: "✓ Gate position set for Arena 1"
UI remains open
    ↓
Player walks to next position
    ↓
Clicks "Add Side A Spawn" button
    ↓
Blue sphere appears
Chat message: "✓ Side A spawn #1 set for Arena 1"
Info updates: "Side A Spawns: 1"
UI remains open
    ↓
Player continues setting positions
    ↓
When done, clicks "Save Config"
    ↓
Chat message: "✓ Configuration saved!"
UI remains open
    ↓
Player clicks "CLOSE"
    ↓
UI disappears
```

## Screen Position

The UI panel is centered on screen:
- **Horizontal**: 30% to 70% of screen width (40% width)
- **Vertical**: 20% to 80% of screen height (60% height)
- **Cursor**: Enabled (can click buttons)

This ensures:
- ✅ Doesn't block too much view
- ✅ Easy to read and interact with
- ✅ All buttons are easily clickable
- ✅ Can still see game world around edges

## Keyboard/Mouse

- **Mouse cursor** appears when UI is open
- **Click buttons** with left mouse click
- **Close UI** by clicking CLOSE or typing `/adminsetup` again
- **Chat messages** show confirmations
- **Spheres** appear in the game world

## Example Chat Messages

When using the UI, you'll see:
```
✓ Gate position set for Arena 1
✓ Spectator position set for Arena 1
✓ Side A spawn #1 set for Arena 1
✓ Side A spawn #2 set for Arena 1
✓ Side B spawn #1 set for Arena 1
✓ All sphere markers cleared
✓ Configuration saved!
Use 'o.reload PaintballArena' to apply changes
```

Clean, simple feedback with checkmarks!

## Mobile/Console Players

The UI works on:
- ✅ PC (keyboard + mouse)
- ✅ PC (controller)
- ✅ Console (Xbox/PlayStation controllers)

Button navigation uses:
- Mouse cursor (PC)
- Joystick cursor (controller/console)

## Customization

Server admins can modify the UI in the code:
- Button positions (RectTransform coordinates)
- Colors (RGBA values)
- Panel size (AnchorMin/AnchorMax)
- Font sizes
- Text content

Located in the `ShowAdminUI()` method in PaintballArena.cs

## Comparison

### Old System (Chat Commands)
```
Player: /arenaadmin selectarena 1
Plugin: Selected Arena 1 for setup

Player: /arenaadmin setgate
Plugin: Gate position set for Arena 1 at (100.0, 0.0, 100.0)

Player: /arenaadmin setsidea 1
Plugin: Side A (Blue) spawn #1 set for Arena 1 at (150.0, 0.0, 150.0)
```
❌ Lots of typing
❌ Have to remember commands
❌ Easy to make typos
❌ Must specify spawn numbers

### New System (UI Panel)
```
Player: /adminsetup
→ UI opens

[Clicks Arena 1 button]
→ Arena highlighted

[Clicks Set Gate button]
Plugin: ✓ Gate position set for Arena 1

[Clicks Add Side A Spawn]
Plugin: ✓ Side A spawn #1 set for Arena 1
```
✅ One command to open UI
✅ Click buttons (no typing)
✅ Visual feedback
✅ Auto-numbering
✅ Faster workflow

## Summary

The admin setup UI provides:
- **Visual interface** instead of text commands
- **One-click actions** for all setup tasks
- **Real-time feedback** in chat and UI
- **Color-coded buttons** for easy identification
- **Smart features** like auto-incrementing spawn numbers
- **Professional appearance** with modern UI design

**Result**: Setting up arenas is now 10x easier and faster! 🚀
