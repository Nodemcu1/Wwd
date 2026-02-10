# Force Start Feature - Testing Command

## Overview

The force start feature allows admins to instantly start arena matches for testing purposes, bypassing all normal requirements like minimum player counts and countdown timers.

**Command**: `/arena forcestart <1-3>`

**Permission**: Admin only (`paintballarena.admin`)

**Purpose**: Enable solo testing and rapid iteration during development/configuration

---

## Quick Start

### Basic Usage

```bash
# Force start Arena 1
/arena forcestart 1

# Force start Arena 2
/arena forcestart 2

# Force start Arena 3
/arena forcestart 3
```

### Solo Testing Workflow

```bash
# 1. Join an arena
/arena join 1 Green

# 2. Force start the match
/arena forcestart 1

# 3. Test spawns, loadouts, timers, etc.
# (Match starts immediately)

# 4. Leave when done
/arena leave
```

---

## Features

### What It Does

✅ **Bypasses Normal Requirements**
- No minimum player count needed
- Works with 0, 1, or any number of players
- Skips the 10-second countdown timer
- Instant match start

✅ **Safety Checks**
- Admin permission required
- Validates arena ID (1-3)
- Checks if arena exists
- Prevents starting if already in progress
- Prevents starting if already counting down

✅ **Perfect for Testing**
- Test spawn positions solo
- Verify loadout distribution
- Check round timers
- Test arena configuration
- Validate scoring system
- Debug issues quickly

---

## Command Details

### Syntax

```
/arena forcestart <arena-id>
```

### Parameters

| Parameter | Type | Description | Valid Values |
|-----------|------|-------------|--------------|
| `arena-id` | Integer | Arena number to force start | 1, 2, or 3 |

### Requirements

1. **Admin Permission**: Must have `paintballarena.admin` permission
2. **Valid Arena**: Arena must exist in configuration
3. **Waiting State**: Arena must be in waiting state (not in progress or counting down)

### Examples

```bash
# Force start Arena 1 (5v5_TDM mode)
/arena forcestart 1

# Force start Arena 2 (2v2_Chamber mode)
/arena forcestart 2

# Force start Arena 3 (1v1_Chamber mode)
/arena forcestart 3
```

---

## How It Works

### Normal Match Flow

```
┌─────────────────────┐
│ Players Join Arena  │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Wait for Min Players│  ← Usually requires 2+ players
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ 10 Second Countdown │  ← Countdown timer runs
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│   Match Starts      │
└─────────────────────┘
```

### Force Start Flow

```
┌─────────────────────┐
│ Admin Command       │
│ /arena forcestart 1 │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Permission Check    │  ← Admin only
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Validation Checks   │  ← Arena exists? State valid?
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Cancel Countdown    │  ← If any countdown running
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Match Starts        │  ← Immediately!
└─────────────────────┘
```

---

## Testing Workflows

### Solo Configuration Testing

**Purpose**: Test arena setup without needing other players

```bash
# Step 1: Configure arena using admin UI
/adminsetup
# Set spawns, gates, spectator area, etc.
# Save configuration

# Step 2: Join the arena
/arena join 1 Green

# Step 3: Force start the match
/arena forcestart 1

# Step 4: Test the configuration
# - Are you spawned at the correct location?
# - Did you receive the correct loadout?
# - Do the timers work properly?
# - Does scoring work?

# Step 5: Leave and adjust if needed
/arena leave
# Make adjustments in /adminsetup if necessary
# Repeat steps 2-5 until satisfied
```

### Multi-Arena Testing

**Purpose**: Quickly test all arenas

```bash
# Test Arena 1 (5v5_TDM)
/arena join 1 Green
/arena forcestart 1
# Observe and test...
/arena leave

# Test Arena 2 (2v2_Chamber)
/arena join 2 Blue
/arena forcestart 2
# Observe and test...
/arena leave

# Test Arena 3 (1v1_Chamber)
/arena join 3 Orange
/arena forcestart 3
# Observe and test...
/arena leave
```

### Spawn Position Testing

**Purpose**: Verify all team spawn positions work

```bash
# Test Side A spawns
/arena join 1 Green
/arena forcestart 1
# Note spawn position
/arena leave

# Test Side B spawns
/arena join 1 Blue
/arena forcestart 1
# Note spawn position
/arena leave

# Repeat for other colors if testing team selection spheres
```

### Loadout Verification

**Purpose**: Verify players receive correct items

```bash
# Start match
/arena join 1 Green
/arena forcestart 1

# Check inventory should have:
# ✓ paintballoveralls.suit (1)
# ✓ paintballgun (1)
# ✓ ammo.paintball (100)

# If items missing or incorrect, check GiveLoadout() code
```

### Timer Testing

**Purpose**: Test round timers and match flow

```bash
# Start match
/arena join 1 Green
/arena forcestart 1

# Observe:
# - Does round timer start?
# - Does it count down correctly?
# - What happens when timer expires?
# - Does match end properly?
```

---

## Safety Features

### Permission Check

Only admins can use this command:

```csharp
if (!permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
{
    player.ChatMessage("You don't have permission to use this command");
    return;
}
```

**Result**: Players without admin permission cannot force start matches

### Arena Validation

Ensures the specified arena exists:

```csharp
if (!arenaInstances.ContainsKey(arenaId))
{
    player.ChatMessage($"Arena {arenaId} does not exist");
    return;
}
```

**Result**: Cannot force start non-existent arenas

### State Validation

Prevents force starting if arena is already active:

```csharp
if (arena.State == ArenaState.InProgress)
{
    player.ChatMessage($"Arena {arenaId} is already in progress");
    return;
}

if (arena.State == ArenaState.Countdown)
{
    player.ChatMessage($"Arena {arenaId} is already counting down");
    return;
}
```

**Result**: Cannot interrupt active matches or countdowns

### Countdown Cleanup

Properly cancels any existing countdown:

```csharp
if (arena.CountdownTimer != null)
{
    arena.CountdownTimer.Destroy();
    arena.CountdownTimer = null;
}
```

**Result**: No timer conflicts or memory leaks

---

## Error Messages

### No Permission

**Message**: `You don't have permission to use this command`

**Cause**: Player doesn't have admin permission

**Solution**: Grant `paintballarena.admin` permission to the player

---

### Invalid Arena ID

**Message**: `Invalid arena ID. Choose 1, 2, or 3`

**Cause**: Arena ID is not 1, 2, or 3

**Solution**: Use a valid arena number (1, 2, or 3)

**Examples**:
```bash
/arena forcestart 0   # ❌ Invalid
/arena forcestart 4   # ❌ Invalid
/arena forcestart abc # ❌ Invalid
/arena forcestart 1   # ✅ Valid
```

---

### Arena Doesn't Exist

**Message**: `Arena {id} does not exist`

**Cause**: Arena configuration missing or not initialized

**Solution**: 
1. Check PaintballArena.json has configuration for that arena
2. Reload the plugin: `o.reload PaintballArena`
3. Verify no errors during plugin load

---

### Already In Progress

**Message**: `Arena {id} is already in progress`

**Cause**: Arena is currently running a match

**Solution**: Wait for match to end, or use admin commands to end it first

---

### Already Counting Down

**Message**: `Arena {id} is already counting down`

**Cause**: Arena countdown timer is running

**Solution**: Wait a few seconds for countdown to complete

---

## Benefits

### For Development & Testing

✅ **Solo Testing Possible**
- No need to gather multiple players
- Test at any time
- Quick iteration cycles

✅ **Rapid Configuration Validation**
- Test spawn positions immediately
- Verify loadout system works
- Check timers and scoring
- Validate arena setup

✅ **Fast Debugging**
- Reproduce issues quickly
- Test fixes immediately
- Iterate on solutions

### For Server Administration

✅ **Pre-Opening Testing**
- Test all arenas before opening to players
- Verify everything works
- Catch configuration errors early

✅ **Quick Demonstrations**
- Show features to players
- Preview arenas
- Demonstrate gameplay

✅ **Easy Troubleshooting**
- Debug player reports
- Test specific scenarios
- Validate fixes

### For Players (Indirect)

✅ **Better Server Experience**
- Admins can thoroughly test
- Fewer bugs and issues
- Well-configured arenas
- Smoother gameplay

---

## Example Scenarios

### Scenario 1: New Spawn Configuration

**Problem**: Just configured new spawn points, want to verify they work

**Solution**:
```bash
# 1. Join the arena
/arena join 1 Green

# 2. Force start to test
/arena forcestart 1

# 3. Check spawn position
# - Am I at the correct location?
# - Am I underground? (bad Y coordinate)
# - Am I too high? (floating)

# 4. Adjust if needed
/arena leave
/adminsetup
# Adjust spawn positions
# Save

# 5. Repeat until perfect
/arena join 1 Green
/arena forcestart 1
```

---

### Scenario 2: Testing All Game Modes

**Problem**: Have 3 arenas with different modes, want to test each

**Solution**:
```bash
# Arena 1: 5v5_TDM
/arena join 1 Green
/arena forcestart 1
# Test 5v5 mode...
/arena leave

# Arena 2: 2v2_Chamber
/arena join 2 Blue
/arena forcestart 2
# Test 2v2 mode...
/arena leave

# Arena 3: 1v1_Chamber
/arena join 3 Orange
/arena forcestart 3
# Test 1v1 mode...
/arena leave
```

---

### Scenario 3: Verifying Loadout Changes

**Problem**: Modified GiveLoadout() code, need to verify it works

**Solution**:
```bash
# 1. Join arena
/arena join 1 Green

# 2. Force start
/arena forcestart 1

# 3. Check inventory contains:
# ✓ paintballoveralls.suit (1 piece)
# ✓ paintballgun (1 piece)
# ✓ ammo.paintball (100 rounds)

# 4. Verify items work
# - Can you wear the suit?
# - Can you equip the gun?
# - Does the gun have ammo?
# - Can you shoot?
```

---

### Scenario 4: Testing Timer System

**Problem**: Want to verify round timers work correctly

**Solution**:
```bash
# 1. Start match
/arena join 1 Green
/arena forcestart 1

# 2. Observe timer
# - Does round timer start?
# - Does it count down?
# - Check console or UI for timer value

# 3. Wait for timer to expire
# - What happens at 0?
# - Does round end properly?
# - Does next round start?

# 4. Test early round end
# (Simulate elimination or victory condition)
```

---

## Comparison: Before vs After

### Before Force Start

**Testing Spawns**:
- ❌ Need at least 2 players
- ❌ Coordinate with someone to test
- ❌ Wait for countdown (10 seconds)
- ❌ Difficult to rapidly iterate

**Testing Configuration**:
- ❌ Requires multiple people
- ❌ Time-consuming setup
- ❌ Hard to test all scenarios
- ❌ Slow iteration cycles

**Debugging Issues**:
- ❌ Need to reproduce with players
- ❌ Difficult to test fixes
- ❌ Slow feedback loop

### After Force Start

**Testing Spawns**:
- ✅ Test solo anytime
- ✅ No coordination needed
- ✅ Instant start
- ✅ Rapid iteration possible

**Testing Configuration**:
- ✅ Solo testing works
- ✅ Quick setup validation
- ✅ Easy to test all scenarios
- ✅ Fast iteration cycles

**Debugging Issues**:
- ✅ Reproduce solo
- ✅ Test fixes immediately
- ✅ Fast feedback loop

---

## Implementation Details

### Code Location

**File**: `PaintballArena.cs`

**Command Handler**: Lines ~1275-1297 in `ArenaCommand()` method

**Force Start Method**: Lines ~1384-1420 in `ForceStartArena()` method

### Command Flow

```csharp
// 1. Permission check
if (!permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
    return; // Reject

// 2. Argument validation
if (args.Length < 2)
    return; // Show usage

// 3. Arena ID parsing
if (!int.TryParse(args[1], out int arenaId))
    return; // Invalid ID

// 4. Call force start
ForceStartArena(player, arenaId);
```

### Force Start Logic

```csharp
// 1. Arena existence check
if (!arenaInstances.ContainsKey(arenaId))
    return; // Arena doesn't exist

// 2. State validation
if (arena.State == ArenaState.InProgress)
    return; // Already running

if (arena.State == ArenaState.Countdown)
    return; // Already counting down

// 3. Cancel any existing countdown
if (arena.CountdownTimer != null)
{
    arena.CountdownTimer.Destroy();
    arena.CountdownTimer = null;
}

// 4. Direct start
arena.StartMatch();
```

---

## Troubleshooting

### Command Not Working

**Issue**: Typing `/arena forcestart 1` does nothing

**Checks**:
1. Do you have admin permission?
   ```
   o.grant user YourName paintballarena.admin
   ```

2. Is the plugin loaded?
   ```
   oxide.plugins
   # Should show PaintballArena
   ```

3. Are there any errors in console?
   ```
   Check server console for errors
   ```

### Match Doesn't Start

**Issue**: Command executes but match doesn't start

**Checks**:
1. Check arena state:
   ```
   /arena status
   # Should show arena state
   ```

2. Check console for errors

3. Verify arena configuration exists in PaintballArena.json

### Spawning Issues

**Issue**: Match starts but spawning doesn't work

**Checks**:
1. Are spawn positions configured?
   ```
   /adminsetup
   # Check if spawns are set
   ```

2. Check Y coordinates aren't 0 (underground)

3. Verify spawn count > 0 for team

### Loadout Not Given

**Issue**: Match starts but no items received

**Checks**:
1. Check console for item creation errors

2. Verify server has paintball items:
   - paintballoveralls.suit
   - paintballgun
   - ammo.paintball

3. Check GiveLoadout() method for errors

---

## Best Practices

### Testing New Configuration

```bash
# 1. Configure in stages
/adminsetup
# Set lobby first
# Set one arena at a time
# Test each change

# 2. Test after each change
/arena join 1 Green
/arena forcestart 1
# Verify change worked
/arena leave

# 3. Document what works
# Note good spawn positions
# Record any issues
```

### Rapid Iteration

```bash
# Quick test cycle:
1. Make change in /adminsetup
2. /arena forcestart 1
3. Test
4. /arena leave
5. Repeat

# Use same team/arena for consistency
# Only change one thing at a time
# Note what each change does
```

### Pre-Production Testing

```bash
# Before opening to players:

# 1. Test all arenas
/arena forcestart 1
/arena forcestart 2
/arena forcestart 3

# 2. Test all teams/sides
# Join each team color
# Force start and verify spawns

# 3. Test all features
# - Spawns
# - Loadouts
# - Timers
# - Scoring
# - Round progression

# 4. Test edge cases
# - Solo player
# - Multiple players
# - Different game modes
```

---

## Related Commands

### Player Commands

```bash
/arena join <1-3> <team>  # Join arena and team
/arena leave              # Leave current arena
/arena status             # View all arena states
```

### Admin Commands

```bash
/adminsetup                    # Open admin configuration UI
/arena forcestart <1-3>        # Force start arena (testing)
o.grant user <name> paintballarena.admin  # Grant admin permission
```

---

## Summary

The force start feature is a powerful testing tool that enables:

✅ **Solo Testing**
- No need for multiple players
- Test anytime
- Quick iteration

✅ **Rapid Development**
- Validate changes immediately
- Fast debugging
- Easy configuration testing

✅ **Better Server**
- Thorough pre-testing
- Fewer bugs
- Well-configured arenas

**Command**: `/arena forcestart <1-3>`

**Permission**: `paintballarena.admin`

**Status**: Ready for use! 🚀

---

## Change Log

### v1.0.0 - Force Start Feature Added

**Added**:
- `/arena forcestart <1-3>` command
- `ForceStartArena()` method
- Admin permission check
- State validation
- Countdown cleanup
- Help text update

**Benefits**:
- Solo testing possible
- Rapid configuration validation
- Easy debugging

**Status**: Production ready
