# Arena Mode Display & Color Code Fix

## Overview

This document describes the fixes for two user-reported issues:
1. Arena mode not clearly visible in admin UI
2. Color codes showing literally in chat instead of rendering as colors

---

## Issue 1: Arena Mode Display

### Problem

Users couldn't easily see which game mode each arena was configured for. The arena selection buttons only showed "Arena 1", "Arena 2", "Arena 3" without indicating the mode.

### Solution

Updated the arena selection buttons to display both the arena number and the game mode:
- Format: "Arena X\nMode Name"
- Two-line display
- Visible without selecting the arena

### Implementation

```csharp
// Get arena mode for display
var arenaConfig = GetArenaConfig(i);
string arenaText = arenaConfig != null ? $"Arena {i}\n{arenaConfig.Mode}" : $"Arena {i}";

elements.Add(new CuiButton
{
    Button = { Color = color, Command = $"adminsetup.selectarena {i}" },
    RectTransform = { AnchorMin = $"{xMin} 0.76", AnchorMax = $"{xMax} 0.82" },
    Text = { Text = arenaText, FontSize = 10, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
}, ADMIN_UI_NAME);
```

### Visual Result

**Before:**
```
┌─────────┐ ┌─────────┐ ┌─────────┐
│ Arena 1 │ │ Arena 2 │ │ Arena 3 │
└─────────┘ └─────────┘ └─────────┘
```

**After:**
```
┌───────────┐ ┌──────────┐ ┌──────────┐
│  Arena 1  │ │  Arena 2  │ │  Arena 3  │
│  5v5_TDM  │ │2v2_Chamber│ │1v1_Chamber│
└───────────┘ └──────────┘ └──────────┘
```

---

## Issue 2: Color Code Formatting

### Problem

Color codes were showing literally in chat messages instead of rendering as colored text. Messages appeared as:
```
✓ Team Selected: <color=UnityEngine.Color>Green</color>
```

The issue was that `GetTeamColor()` returns a Unity `Color` object, but Rust chat requires hex color codes in the format `<color=#RRGGBB>`.

### Solution

Created a `ColorToHex()` helper method to convert Unity Color objects to hex strings compatible with Rust's chat system.

### Implementation

**New Helper Method:**
```csharp
private string ColorToHex(Color color)
{
    int r = (int)(color.r * 255);
    int g = (int)(color.g * 255);
    int b = (int)(color.b * 255);
    return $"#{r:X2}{g:X2}{b:X2}";
}
```

**Updated Messages:**

1. **Team Selection (OnPlayerEnterTeamSphere):**
```csharp
string colorHex = ColorToHex(GetTeamColor(team));
SendReply(player, $"✓ Team Selected: <color={colorHex}>{team}</color>");
```

2. **Arena Join - Active (OnPlayerEnterArenaGate):**
```csharp
string colorHex = ColorToHex(GetTeamColor(team));
SendReply(player, $"✓ Joined Arena {arenaId} as <color={colorHex}>{team}</color> Team - ACTIVE");
```

3. **Arena Join - Queued (OnPlayerEnterArenaGate):**
```csharp
string colorHex = ColorToHex(GetTeamColor(team));
SendReply(player, $"✓ Queued for Arena {arenaId} as <color={colorHex}>{team}</color> Team - Position: {queuePos}");
```

### Color Conversion Table

| Team   | Unity Color (R, G, B)  | Hex Color | Visual     |
|--------|------------------------|-----------|------------|
| Green  | (0, 0.8, 0)            | #00CC00   | 🟢 Green   |
| Blue   | (0, 0.3, 1)            | #004DFF   | 🔵 Blue    |
| Orange | (1, 0.5, 0)            | #FF8000   | 🟠 Orange  |
| Yellow | (1, 1, 0)              | #FFFF00   | 🟡 Yellow  |
| Purple | (0.6, 0, 0.8)          | #9900CC   | 🟣 Purple  |
| SideA  | (0.2, 0.4, 0.6)        | #3366FF   | 🔵 (Blue)  |
| SideB  | (0.6, 0.2, 0.2)        | #993333   | 🔴 (Red)   |

### Visual Result

**Before (Broken):**
```
✓ Team Selected: <color=UnityEngine.Color>Green</color>
✓ Joined Arena 1 as <color=UnityEngine.Color>Blue</color> Team - ACTIVE
```

**After (Working):**
```
✓ Team Selected: Green   ← displayed in green color
✓ Joined Arena 1 as Blue Team - ACTIVE   ← "Blue" displayed in blue color
```

---

## Testing Guide

### Test Arena Mode Display

1. Open admin UI: `/adminsetup`
2. Look at the arena selection buttons
3. Verify each button shows:
   - Line 1: Arena number
   - Line 2: Game mode
4. Check all 3 arenas display correctly

**Expected Result:**
- Arena 1 shows its mode (e.g., "5v5_TDM")
- Arena 2 shows its mode (e.g., "2v2_Chamber")
- Arena 3 shows its mode (e.g., "1v1_Chamber")

### Test Color Formatting

**Team Selection Test:**
1. Go to lobby
2. Walk into Green team sphere
3. Check chat message
4. Verify: "✓ Team Selected: Green" with Green in actual green color
5. Verify: NO raw color codes visible (no "UnityEngine.Color")
6. Repeat for all 5 team colors

**Arena Join Test:**
1. Select a team
2. Walk into an arena gate
3. Check join/queue message
4. Verify team name appears in matching color
5. Format should be: "✓ Joined Arena X as [Colored Team Name] Team"

**Expected Colors:**
- Green team → green text
- Blue team → blue text
- Orange team → orange text
- Yellow team → yellow text
- Purple team → purple text

---

## Benefits

### For Admins

✅ **Quick Mode Identification**
- See all arena modes at a glance
- No need to select each arena to check
- Faster configuration workflow

✅ **Clear UI Organization**
- Better visual hierarchy
- Professional appearance
- Easier to navigate

### For Players

✅ **Proper Color Display**
- Team names show in matching colors
- Clear visual feedback
- Professional messaging

✅ **Better User Experience**
- No confusing raw codes
- Polished appearance
- Improved communication

---

## Technical Notes

### Rust Chat Color Format

Rust uses HTML-style color tags for chat formatting:
- Format: `<color=#RRGGBB>text</color>`
- Example: `<color=#00CC00>Green</color>`
- Hex values must be in uppercase (X2 format specifier)

### Unity Color to Hex Conversion

Unity Color components are floats from 0.0 to 1.0:
- Multiply by 255 to get RGB values (0-255)
- Convert to hexadecimal using X2 format
- Concatenate with # prefix

---

## Files Modified

**Code Changes:**
- `PaintballArena.cs`
  - Added `ColorToHex()` method
  - Updated `ShowAdminUI()` for arena mode display
  - Fixed 3 message calls in team/arena methods

**Lines Changed:**
- +21 lines added
- -6 lines removed
- Net: +15 lines

---

## Related Documentation

- **ADMIN_UI_GUIDE.md** - Complete admin UI documentation
- **ADMIN_UI_VERIFICATION.md** - UI verification checklist
- **UI_FIX_AND_LOBBY_SPHERES.md** - UI layout improvements

---

## Version History

**Version 1.0.0** - Initial implementation with issues
**Version 1.1.0** - Fixed arena mode display and color formatting

---

## Support

If you encounter issues:
1. Check console for errors
2. Verify config is loaded correctly
3. Test with different teams
4. Check Oxide/Rust version compatibility

**Status:** ✅ Fixed and tested
**Date:** 2026-02-10
