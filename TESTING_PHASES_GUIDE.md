# PaintballArena Testing Phases Guide

## Complete Testing Procedure for Queue-Based System

This guide provides comprehensive testing procedures for all 8 phases of the PaintballArena plugin testing process.

---

## Pre-Test Requirements

Before beginning any testing phase, ensure:

- [ ] Test server is running Rust with Oxide/uMod installed
- [ ] You have admin permissions on the server
- [ ] PaintballArena.cs and PaintballArena.json files are ready
- [ ] Console access is available for monitoring
- [ ] At least 1-2 test players available (more for later phases)

---

## Phase 1: Server Deployment & Loading

**Objective**: Successfully deploy the plugin to the test server and verify it loads without errors.

### Duration: 30 minutes

### Steps

1. **Upload Plugin Files**
   ```
   Upload to: oxide/plugins/PaintballArena.cs
   Upload to: oxide/config/PaintballArena.json
   ```

2. **Monitor Server Console**
   - Watch for plugin load message
   - Check for compilation errors
   - Look for initialization messages

3. **Verify Plugin Loaded**
   ```
   Command: oxide.plugins
   Expected: PaintballArena should be listed
   ```

4. **Grant Admin Permission**
   ```
   Command: o.grant user <yourname> paintballarena.admin
   Expected: Permission granted message
   ```

5. **Check Config Generated**
   ```
   Location: oxide/config/PaintballArena.json
   Expected: File exists with default configuration
   ```

### Success Criteria

- ✅ Plugin appears in oxide.plugins list
- ✅ No compilation errors in console
- ✅ Config file generated successfully
- ✅ Admin permission granted
- ✅ No runtime errors

### Common Issues

**Issue**: Plugin won't compile
- **Solution**: Check for syntax errors, verify file encoding is UTF-8

**Issue**: Config not generated
- **Solution**: Check file permissions, verify Oxide is running properly

**Issue**: Permission not granted
- **Solution**: Verify user name is correct, check Oxide permissions system

### Console Commands to Test

```
oxide.plugins              # List all plugins
o.grant user <name> paintballarena.admin   # Grant permission
o.reload PaintballArena   # Reload plugin
```

---

## Phase 2: Sphere Detection Testing

**Objective**: Verify that the sphere proximity detection system works correctly for team selection and arena queueing.

### Duration: 45 minutes

### Prerequisites

- [ ] Phase 1 complete
- [ ] Admin permission granted
- [ ] At least 1 test player

### Steps

#### 2A: Admin UI Verification

1. **Open Admin UI**
   ```
   Command: /adminsetup
   Expected: Admin UI panel appears
   ```

2. **Verify UI Elements**
   - [ ] Arena selection buttons (1, 2, 3) visible
   - [ ] Lobby setup buttons visible
   - [ ] Team sphere buttons visible (5 colors)
   - [ ] Arena gate buttons visible (3 gates)
   - [ ] Spawn setup buttons visible
   - [ ] Utility buttons (Clear, Save, Close) visible

#### 2B: Set Up Lobby Spheres

1. **Set Central Lobby**
   ```
   Walk to: Desired lobby spawn location
   Click: [Set Lobby] button
   Expected: Purple sphere appears, "Lobby position set" message
   ```

2. **Set Team Color Spheres** (Repeat for each color)
   ```
   Walk to: Team sphere location (space them out)
   Click: [Green Sphere] button
   Expected: Light green sphere appears
   
   Repeat for:
   - Blue Sphere (light blue marker)
   - Orange Sphere (orange marker)
   - Yellow Sphere (yellow marker)
   - Purple Sphere (purple marker)
   ```

3. **Set Arena Gate Spheres**
   ```
   Walk to: Arena 1 gate location
   Click: [Arena 1 Gate] button
   Expected: Dark grey sphere appears
   
   Repeat for Arena 2 and Arena 3 gates
   ```

4. **Save Configuration**
   ```
   Click: [Save Config] button
   Expected: "Configuration saved" message
   ```

#### 2C: Test Sphere Detection

1. **Test Team Color Sphere Detection**
   ```
   Action: Walk near Green team sphere (within 2m)
   Expected: "Team Selected: Green" message
   Expected: Player state changes to TeamSelected
   
   Test each color:
   - Green
   - Blue
   - Orange
   - Yellow
   - Purple
   ```

2. **Test Arena Gate Sphere Detection**
   ```
   Prerequisites: Have selected a team first
   
   Action: Walk near Arena 1 gate sphere (within 2m)
   Expected: "Queued for Arena 1" message
   Expected: Player state changes to InQueue
   Expected: Teleported to spectator area OR battle spawn (if first 2 teams)
   
   Test each gate:
   - Arena 1 Gate
   - Arena 2 Gate
   - Arena 3 Gate
   ```

3. **Test Proximity Radius**
   ```
   Stand 3m away from sphere: Should NOT trigger
   Stand 2m away from sphere: Should trigger
   Stand 1m away from sphere: Should definitely trigger
   ```

### Success Criteria

- ✅ All sphere visual markers appear correctly
- ✅ Team color sphere detection works (5 colors)
- ✅ Arena gate sphere detection works (3 gates)
- ✅ 2m proximity radius is accurate
- ✅ State transitions happen correctly
- ✅ Chat messages display properly

### Common Issues

**Issue**: Spheres don't appear when placing
- **Solution**: Check admin permission, verify UI button clicks registered

**Issue**: Detection not triggering
- **Solution**: Verify proximity (must be within 2m), check sphere positions saved

**Issue**: Wrong sphere triggers
- **Solution**: Check sphere positions, ensure they're spaced apart (minimum 5m)

### Test Scenarios

**Scenario 1: Solo Player Flow**
1. Join server → Spawn at lobby
2. Walk to Green sphere → Team selected
3. Walk to Arena 1 gate → Queued
4. Result: Should be waiting for second team

**Scenario 2: Team Selection Change**
1. Select Green team
2. Walk to Blue sphere → Team changed to Blue
3. Verify team selection updated

**Scenario 3: Multiple Team Selections**
1. Player 1: Select Green
2. Player 2: Select Blue
3. Both walk to Arena 1 gate
4. Result: Match should start immediately

---

## Phase 3: Queue System Testing

**Objective**: Verify that the queue system enforces 2-team limit and properly manages waiting teams.

### Duration: 45 minutes

### Prerequisites

- [ ] Phase 2 complete
- [ ] Sphere detection working
- [ ] At least 3 test players available

### Steps

#### 3A: Two-Team Limit Test

1. **Setup First Two Teams**
   ```
   Player 1: Select Green, queue for Arena 1
   Player 2: Select Blue, queue for Arena 1
   
   Expected: Both teleported to battle spawns
   Expected: Match countdown starts (10 seconds)
   Expected: Arena status shows "Active: Green, Blue"
   ```

2. **Add Third Team**
   ```
   Player 3: Select Orange, queue for Arena 1
   
   Expected: Teleported to spectator area
   Expected: Message: "Queue position: 1 (2 teams active)"
   Expected: Arena status shows "Waiting: Orange"
   ```

3. **Add Fourth Team**
   ```
   Player 4: Select Yellow, queue for Arena 1
   
   Expected: Teleported to spectator area
   Expected: Message: "Queue position: 2"
   Expected: Arena status shows "Waiting: Orange, Yellow"
   ```

#### 3B: FIFO Queue Behavior

1. **Complete First Match**
   ```
   Let first match end (or force end for testing)
   
   Expected: Green and Blue teams teleported to spectator
   Expected: Orange team (first in queue) teleported to battle
   Expected: Yellow team (second in queue) teleported to battle
   Expected: Match countdown starts
   Expected: Orange vs Yellow match begins
   ```

2. **Verify Queue Order**
   ```
   Add 3 more teams in order: Purple, Green (re-queue), Blue (re-queue)
   
   Expected queue order:
   Active: Orange, Yellow
   Waiting: Purple (pos 1), Green (pos 2), Blue (pos 3)
   ```

3. **Test Queue Advancement**
   ```
   End second match
   
   Expected: Orange and Yellow to spectator
   Expected: Purple and Green to battle
   Expected: Blue still waiting (pos 1)
   ```

#### 3C: Spectator Area Testing

1. **Verify Spectator Teleportation**
   ```
   When team enters queue (position 3+):
   Expected: Teleport to arena's spectator position
   Expected: Can see arena but not participate
   ```

2. **Test Spectator Permissions**
   ```
   While in spectator:
   - Cannot damage other players
   - Cannot pick up items
   - Can see battle area
   - Receive queue position updates
   ```

### Success Criteria

- ✅ Only 2 teams active at once per arena
- ✅ Additional teams wait in spectator area
- ✅ FIFO queue order maintained
- ✅ Queue positions accurate
- ✅ Automatic advancement works
- ✅ Spectator teleportation works

### Test Scenarios

**Scenario 1: Full Queue Test**
```
5 teams queue for Arena 1:
- Green, Blue → Active
- Orange, Yellow, Purple → Waiting (positions 1, 2, 3)

Match ends:
- Green, Blue → Spectator
- Orange, Yellow → Active
- Purple → Waiting (position 1)
- Green, Blue can re-queue → Waiting (positions 2, 3)
```

**Scenario 2: Multi-Arena Queue**
```
Teams split across arenas:
- Arena 1: Green vs Blue (active), Orange waiting
- Arena 2: Yellow vs Purple (active)
- Arena 3: Empty

Verify: Each arena maintains independent queue
```

**Scenario 3: Queue Position Tracking**
```
Player joins queue position 3
Message should show: "Queue position: 3"

After each match:
Message updates: "Queue position: 2", then "Queue position: 1", then "Starting match!"
```

### Common Issues

**Issue**: More than 2 teams active
- **Solution**: Check MaxActiveTeamsPerArena config, verify queue logic

**Issue**: Queue order wrong
- **Solution**: Check FIFO implementation, verify team add order

**Issue**: Teams not advancing
- **Solution**: Check match end detection, verify PullNextTeamsFromQueue call

---

## Phase 4: Match Flow Testing

**Objective**: Verify complete match flow from start to finish, including auto-start, combat, and rotation.

### Duration: 60 minutes

### Prerequisites

- [ ] Phase 3 complete
- [ ] Queue system working
- [ ] At least 2 players

### Steps

#### 4A: Match Auto-Start

1. **Two Teams Queue**
   ```
   Team 1 (Green): Enter Arena 1
   Team 2 (Blue): Enter Arena 1
   
   Expected:
   - Countdown appears: "Match starting in 10..."
   - Both teams teleported to spawns
   - Teams can see each other
   - 10 second countdown
   ```

2. **Countdown Sequence**
   ```
   Expected messages:
   "Match starting in 10..."
   "Match starting in 9..."
   ...
   "Match starting in 3..."
   "Match starting in 2..."
   "Match starting in 1..."
   "GO!"
   ```

3. **Match Start**
   ```
   After countdown:
   - Teams can move and fight
   - Scoreboard appears
   - Round 1 begins
   - Timer starts counting down
   ```

#### 4B: Combat Mechanics

1. **Test Paintball Damage**
   ```
   Team member shoots opponent:
   Expected: One-hit elimination
   Expected: Victim teleported to spectator
   Expected: Kill feed message
   Expected: Score updated
   ```

2. **Test Team Spawns**
   ```
   Players should spawn at their team's spawn points:
   - Green team → Green spawn positions
   - Blue team → Blue spawn positions
   
   Verify spacing and safety of spawns
   ```

3. **Test Elimination**
   ```
   When hit:
   - Immediate teleportation to spectator
   - Cannot return to match
   - Can see remaining combat
   - Score displayed
   ```

#### 4C: Round Progression

1. **Round Tracking**
   ```
   Complete Round 1:
   Expected: "Round 1 complete! Blue wins!"
   Expected: "Starting Round 2..."
   Expected: Score: Blue 1 - Green 0
   ```

2. **Multiple Rounds**
   ```
   Play through several rounds:
   - Score accumulates correctly
   - Round number increments
   - Teams persist across rounds
   ```

3. **Match Victory**
   ```
   When max rounds reached:
   Expected: "Match complete! Blue wins 6-4!"
   Expected: MVP announced
   Expected: Stats displayed
   ```

#### 4D: Match Rotation

1. **Match End Behavior**
   ```
   When match ends:
   Expected: Both teams teleported to spectator
   Expected: Queue checked for next teams
   Expected: If queue not empty → Next 2 teams pulled
   Expected: New match countdown begins
   ```

2. **Automatic Queue Advancement**
   ```
   Setup:
   - Active: Green vs Blue
   - Waiting: Orange, Yellow
   
   After match:
   - Green, Blue → Spectator
   - Orange, Yellow → Battle spawns
   - New countdown: "Match starting in 10..."
   ```

3. **Empty Queue Behavior**
   ```
   If no teams waiting:
   - Match ends normally
   - Teams to spectator
   - Arena goes idle
   - Next team to queue starts new match
   ```

### Success Criteria

- ✅ Auto-start works with 2 teams
- ✅ 10-second countdown displays
- ✅ Combat mechanics work (one-hit)
- ✅ Elimination teleports to spectator
- ✅ Rounds progress correctly
- ✅ Scores accumulate properly
- ✅ Match ends trigger rotation
- ✅ Next teams auto-enter

### Test Scenarios

**Scenario 1: Complete Match**
```
1. Green vs Blue queue
2. Countdown (10s)
3. Round 1 starts
4. Combat happens
5. Round ends
6. Repeat for 10 rounds
7. Match complete
8. Teams to spectator
```

**Scenario 2: Match with Queue**
```
Active: Green vs Blue
Waiting: Orange, Yellow

Match completes:
→ Green, Blue to spectator
→ Orange, Yellow to battle
→ Countdown starts
→ New match begins
```

**Scenario 3: Continuous Rotation**
```
5 teams rotating:
Match 1: Green vs Blue, waiting: Orange, Yellow, Purple
Match 2: Orange vs Yellow, waiting: Purple, Green, Blue
Match 3: Purple vs Green, waiting: Blue, Orange, Yellow
Continues cycling...
```

---

## Phase 5: Admin UI Testing

**Objective**: Thoroughly test all admin UI functionality for sphere placement and configuration.

### Duration: 30 minutes

### Prerequisites

- [ ] Admin permission
- [ ] `/adminsetup` command works

### Steps

#### 5A: UI Display Test

1. **Open Admin UI**
   ```
   Command: /adminsetup
   Expected: Full UI panel displays
   ```

2. **Verify UI Sections**
   - [ ] Arena selection section (buttons 1, 2, 3)
   - [ ] Central lobby setup section
   - [ ] Team color spheres section (5 buttons)
   - [ ] Arena gate spheres section (3 buttons)
   - [ ] Battle spawn section (5 team buttons)
   - [ ] Arena settings section (gate, spectator)
   - [ ] Utilities section (clear, save, close)
   - [ ] Info display section

3. **Test UI Responsiveness**
   ```
   Click each button:
   - Should respond immediately
   - Should give feedback message
   - UI should update if needed
   ```

#### 5B: Sphere Placement Testing

**Test Each Button:**

1. **Set Lobby** (Global)
   ```
   Walk to position → Click button
   Expected: Purple sphere, "Lobby position set" message
   ```

2. **Team Color Spheres** (Global, 5 buttons)
   ```
   For each color (Green, Blue, Orange, Yellow, Purple):
   Walk to position → Click corresponding button
   Expected: Colored sphere, confirmation message
   ```

3. **Arena Gate Spheres** (Global, 3 buttons)
   ```
   For each arena (1, 2, 3):
   Walk to position → Click corresponding button
   Expected: Grey sphere, "Arena X gate set" message
   ```

4. **Arena-Specific Settings** (Per-Arena)
   ```
   First: Select arena (click Arena 1, 2, or 3)
   
   Then test:
   - Set Gate: Arena entrance position
   - Set Spectator: Viewing area position
   - Add [Color] Spawn: Battle spawn positions (5 colors)
   ```

#### 5C: Visual Marker Verification

1. **Check Sphere Appearance**
   ```
   Each placed sphere should:
   - Be visible and easily seen
   - Have correct color
   - Stay in place
   - Persist until cleared
   ```

2. **Verify Sphere Colors**
   - 🟣 Lobby = Purple
   - 🟢 Green Team = Light green
   - 🔵 Blue Team = Light blue
   - 🟠 Orange Team = Orange
   - 🟡 Yellow Team = Yellow
   - 🟣 Purple Team = Purple
   - ⚫ Arena Gates = Dark grey
   - 🟢 Arena Gate = Green
   - 🟡 Spectator = Yellow

#### 5D: Configuration Persistence

1. **Save Config**
   ```
   After placing spheres:
   Click [Save Config] button
   Expected: "Configuration saved successfully"
   ```

2. **Reload Plugin**
   ```
   Command: o.reload PaintballArena
   Expected: Plugin reloads
   Expected: Spheres no longer visible (markers cleared)
   ```

3. **Verify Config Saved**
   ```
   Check: oxide/config/PaintballArena.json
   Expected: All positions saved correctly
   Expected: Values match placed positions
   ```

4. **Test Config Load**
   ```
   Use positions:
   - Walk to saved team sphere location
   - Should detect team selection
   - Positions should work even without visual markers
   ```

#### 5E: Utility Functions

1. **Clear Spheres**
   ```
   Click [Clear Spheres] button
   Expected: All visual markers disappear
   Expected: "Spheres cleared" message
   Note: Does not delete config, just visual markers
   ```

2. **Close UI**
   ```
   Click [Close] button
   Expected: UI panel disappears
   Expected: Can re-open with /adminsetup
   ```

### Success Criteria

- ✅ All 15+ UI buttons work
- ✅ Sphere markers appear correctly
- ✅ Colors are accurate
- ✅ Positions save to config
- ✅ Config persists after reload
- ✅ Clear function works
- ✅ Close button works

### Common Issues

**Issue**: UI doesn't open
- **Solution**: Check admin permission, verify command syntax

**Issue**: Buttons don't respond
- **Solution**: Check console for errors, try closing and reopening UI

**Issue**: Spheres don't appear
- **Solution**: Verify you're at a valid position, check console

**Issue**: Config not saving
- **Solution**: Check file permissions, verify save button clicked

---

## Phase 6: Integration Testing

**Objective**: Test the system with multiple players across multiple arenas simultaneously.

### Duration: 60 minutes

### Prerequisites

- [ ] All previous phases complete
- [ ] 5-10 test players available
- [ ] All 3 arenas configured

### Steps

#### 6A: Multi-Player Testing

1. **5-Player Test**
   ```
   Setup:
   - 5 different teams (Green, Blue, Orange, Yellow, Purple)
   - All queue for Arena 1
   
   Expected:
   - First 2 teams → Battle
   - Next 3 teams → Spectator queue
   - Proper rotation after matches
   ```

2. **Team Communication**
   ```
   Verify:
   - Team members can see each other
   - Team chat works (if implemented)
   - Teams are properly isolated
   ```

#### 6B: Multi-Arena Testing

1. **Three Arenas Simultaneously**
   ```
   Arena 1: Green vs Blue (active), Orange waiting
   Arena 2: Yellow vs Purple (active)
   Arena 3: Green vs Orange (active)
   
   Verify:
   - All 3 arenas function independently
   - Queues don't interfere
   - Scores tracked separately
   - No cross-arena issues
   ```

2. **Cross-Arena Isolation**
   ```
   Test:
   - Players in Arena 1 cannot damage Arena 2 players
   - Scoreboards show only relevant arena
   - Chat/voice isolated per arena
   - No position conflicts
   ```

#### 6C: Queue Overflow Testing

1. **Full Queue Scenario**
   ```
   Setup: 10 teams try to queue for Arena 1
   
   Expected:
   - 2 teams active
   - 8 teams waiting (positions 1-8)
   - Queue positions accurate
   - Rotation works smoothly
   ```

2. **Queue Management**
   ```
   Test:
   - Players can leave queue
   - Players can switch arenas
   - Queue updates correctly
   - No position conflicts
   ```

#### 6D: Edge Case Testing

1. **Player Disconnects**
   ```
   Scenario: Player disconnects during match
   Expected: Team can continue or forfeit
   Expected: Queue advances normally
   ```

2. **AFK Players**
   ```
   Scenario: Player goes AFK in queue
   Expected: Eventually removed (if AFK detection implemented)
   OR: Takes their turn when it comes
   ```

3. **Rapid Re-queuing**
   ```
   Scenario: Player leaves and immediately re-queues
   Expected: Joins end of queue
   Expected: No position skipping
   ```

4. **Arena Switching**
   ```
   Scenario: Player in Arena 1 queue tries to queue for Arena 2
   Expected: Removed from Arena 1, added to Arena 2
   ```

### Success Criteria

- ✅ Multiple players work smoothly
- ✅ Multiple arenas independent
- ✅ Queues manage 10+ teams
- ✅ Cross-arena isolation works
- ✅ Edge cases handled gracefully
- ✅ No crashes or errors

### Test Scenarios

**Scenario 1: Full Server**
```
10 players:
- 3 in Arena 1 (2 active, 1 waiting)
- 4 in Arena 2 (2 active, 2 waiting)
- 3 in Arena 3 (2 active, 1 waiting)

Verify all arenas function correctly
```

**Scenario 2: Queue Migration**
```
5 teams in Arena 1 queue
Arena 2 and 3 empty

Players realize: "Arena 2 is available!"
Some switch to Arena 2

Verify queues update correctly
```

**Scenario 3: Continuous Play**
```
Run for 30 minutes with 5-10 players
Monitor for:
- Memory leaks
- Performance degradation
- Queue errors
- Match issues
```

---

## Phase 7: Performance & Optimization

**Objective**: Monitor server performance and identify optimization opportunities.

### Duration: 45 minutes

### Prerequisites

- [ ] Integration testing complete
- [ ] Server monitoring tools available

### Steps

#### 7A: Resource Monitoring

1. **CPU Usage**
   ```
   Monitor: Server CPU usage
   Test with: 0 players, 5 players, 10 players
   
   Expected:
   - Idle: Minimal CPU (<5%)
   - Active: Reasonable CPU (<20%)
   - No CPU spikes
   ```

2. **Memory Usage**
   ```
   Monitor: Plugin memory usage
   Test over time: 30 minutes
   
   Expected:
   - Stable memory usage
   - No memory leaks
   - Reasonable footprint
   ```

3. **Network Traffic**
   ```
   Monitor: Network bandwidth
   Expected: Minimal overhead from plugin
   ```

#### 7B: Timer Efficiency

1. **Sphere Detection Timer**
   ```
   Current: Runs every 0.5 seconds
   Monitor: CPU impact
   
   Test:
   - With 0 players: Should not run
   - With players: Should run efficiently
   ```

2. **Arena Update Timers**
   ```
   Monitor: Round timers, countdown timers
   Expected: Accurate timing, no drift
   ```

3. **Optimization Opportunities**
   ```
   Consider:
   - Increase sphere check interval to 1 second?
   - Only check players in lobby?
   - Cache sphere positions?
   - Reduce sphere marker count?
   ```

#### 7C: Database/Config Operations

1. **Config Save Performance**
   ```
   Test: Saving config with many spheres
   Expected: Quick save (<1 second)
   ```

2. **Config Load Performance**
   ```
   Test: Plugin reload time
   Expected: Fast load (<2 seconds)
   ```

#### 7D: Scalability Testing

1. **Player Count Testing**
   ```
   Test with:
   - 5 players
   - 10 players
   - 20 players (if available)
   
   Monitor:
   - Response time
   - Lag
   - Frame rate
   - Server performance
   ```

2. **Match Duration**
   ```
   Test: Long-running matches
   Expected: No performance degradation over time
   ```

### Success Criteria

- ✅ CPU usage reasonable
- ✅ Memory stable (no leaks)
- ✅ Timers accurate
- ✅ No lag or stuttering
- ✅ Scales to 20+ players
- ✅ No performance degradation

### Performance Benchmarks

**Acceptable Performance**:
- CPU usage: <20% with 10 players
- Memory: <50MB plugin footprint
- Sphere detection: <10ms per check
- Match start: <2 seconds
- Config save: <1 second

### Optimization Recommendations

If performance issues found:

1. **Increase Timer Intervals**
   - Sphere check: 0.5s → 1s
   - Arena update: Evaluate frequency

2. **Reduce Visual Markers**
   - Only show spheres to admins
   - Clear markers after setup

3. **Optimize Proximity Checks**
   - Only check players in lobby state
   - Use squared distance (avoid sqrt)

4. **Cache Calculations**
   - Cache sphere positions
   - Cache player states

---

## Phase 8: Final Documentation & Release

**Objective**: Document findings, create final report, and prepare for production release.

### Duration: 30 minutes

### Steps

#### 8A: Bug Documentation

1. **Create Bug Report**
   ```
   For each issue found:
   - Description
   - Steps to reproduce
   - Expected vs actual behavior
   - Severity (Critical/High/Medium/Low)
   - Screenshots if applicable
   ```

2. **Bug Priority**
   ```
   Critical: Must fix before release
   High: Should fix before release
   Medium: Can fix in update
   Low: Nice to have
   ```

#### 8B: Feature Validation

1. **Complete Checklist**
   ```
   ✅ Queue system works
   ✅ 2-team limit enforced
   ✅ Sphere detection accurate
   ✅ Match flow correct
   ✅ Admin UI functional
   ✅ Multi-arena support
   ✅ Performance acceptable
   ✅ Edge cases handled
   ```

2. **Feature Sign-off**
   ```
   Each major feature:
   - Tested thoroughly
   - Works as designed
   - Documented
   - Ready for production
   ```

#### 8C: Documentation Updates

1. **Update User Guide**
   ```
   Based on testing:
   - Add tips discovered
   - Clarify confusing points
   - Add troubleshooting
   - Include best practices
   ```

2. **Update Admin Guide**
   ```
   - Recommended sphere positions
   - Configuration tips
   - Common issues and solutions
   - Performance tips
   ```

#### 8D: Production Checklist

**Pre-Release Checklist**:

- [ ] All critical bugs fixed
- [ ] All features tested and working
- [ ] Documentation complete and accurate
- [ ] Performance acceptable
- [ ] Config file finalized
- [ ] Installation instructions clear
- [ ] Backup/rollback plan ready
- [ ] Support plan established

**Release Preparation**:

- [ ] Create release notes
- [ ] Version number finalized
- [ ] Change log created
- [ ] Known issues documented
- [ ] Support channels established

**Post-Release Plan**:

- [ ] Monitor first 24 hours closely
- [ ] Quick response to issues
- [ ] Gather user feedback
- [ ] Plan for updates

#### 8E: Testing Report

**Create Final Report Including**:

1. **Executive Summary**
   - Overall status
   - Key findings
   - Recommendation (Go/No-Go)

2. **Test Results**
   - Phases completed
   - Pass/fail summary
   - Issues found and resolved

3. **Performance Metrics**
   - CPU usage
   - Memory usage
   - Response times
   - Scalability results

4. **Known Issues**
   - Remaining bugs
   - Workarounds
   - Fix timeline

5. **Recommendations**
   - Configuration suggestions
   - Best practices
   - Future improvements

### Success Criteria

- ✅ All tests documented
- ✅ Bugs categorized and prioritized
- ✅ Documentation updated
- ✅ Production checklist complete
- ✅ Release decision made

---

## Test Summary Template

Use this template to document your testing:

```markdown
# PaintballArena Testing Summary

## Test Information
- **Date**: [Date]
- **Tester**: [Name]
- **Server**: [Server name/IP]
- **Plugin Version**: [Version]

## Phase Results

### Phase 1: Deployment ✅/❌
- Status: [Pass/Fail]
- Issues: [List]
- Notes: [Notes]

### Phase 2: Sphere Detection ✅/❌
- Status: [Pass/Fail]
- Issues: [List]
- Notes: [Notes]

[Continue for all 8 phases...]

## Critical Issues Found
1. [Issue 1]
2. [Issue 2]

## Non-Critical Issues
1. [Issue 1]
2. [Issue 2]

## Performance Metrics
- CPU Usage: [X%]
- Memory Usage: [XMB]
- Max Players Tested: [X]

## Recommendations
- [Recommendation 1]
- [Recommendation 2]

## Release Decision
[Go / No-Go / Conditional]

## Next Steps
1. [Step 1]
2. [Step 2]
```

---

## Quick Reference

### Essential Commands

**Admin**:
```
/adminsetup          - Open admin UI
o.grant user <name> paintballarena.admin
o.reload PaintballArena
oxide.plugins
```

**Player**:
```
/arena status        - Check arena status
/arena leave         - Leave current arena
```

**Console**:
```
All admin UI buttons use: adminsetup.[action]
```

### Expected Timings

- Sphere detection check: Every 0.5 seconds
- Match countdown: 10 seconds
- Round duration: Configurable (default 300s)
- Queue update: Immediate

### File Locations

```
oxide/plugins/PaintballArena.cs
oxide/config/PaintballArena.json
```

### Support

If issues found during testing:
1. Document thoroughly
2. Check console for errors
3. Verify configuration
4. Test in isolation
5. Report with details

---

## Conclusion

This comprehensive testing guide ensures the PaintballArena queue-based system is thoroughly validated before production deployment. Follow each phase systematically, document all findings, and only proceed to production when all critical issues are resolved.

**Good luck with testing!** 🚀
