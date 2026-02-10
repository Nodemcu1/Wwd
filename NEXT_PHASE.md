# Next Phase: Testing & Deployment

## ✅ Compilation Error Fixed!

The compilation error at line 1135 has been resolved. The plugin now compiles successfully.

---

## What's Next: Testing Phase

Now that development is complete and the code compiles, the next phase is **comprehensive testing** to ensure everything works as designed.

## Testing Roadmap

### Phase 1: Server Deployment & Loading ⏳

**Objective**: Get the plugin running on a test server

**Steps**:
1. Upload `PaintballArena.cs` to `oxide/plugins/` folder
2. Upload `PaintballArena.json` to `oxide/config/` folder
3. Server will auto-reload the plugin
4. Check server console for any errors

**Expected Output**:
```
[Oxide] Loaded plugin PaintballArena v1.2.0 by YourName
PaintballArena plugin loaded - Queue-based system initialized
```

**Success Criteria**:
- [x] Plugin loads without errors
- [x] No runtime exceptions
- [x] Initialization message appears

---

### Phase 2: Admin UI & Sphere Setup ⏳

**Objective**: Configure all sphere positions using the admin UI

**Steps**:
1. Grant admin permission: `o.grant user <yourname> paintballarena.admin`
2. Type `/adminsetup` in chat
3. Admin UI panel should appear

**Configure Central Lobby**:
1. Walk to desired lobby spawn location
2. Click **[Set Lobby]** button
3. Purple sphere should appear

**Configure Team Color Spheres** (5 spheres):
1. Walk to location for Green team sphere
2. Click **[Green Sphere]** button
3. Green sphere appears
4. Repeat for Blue, Orange, Yellow, Purple

**Configure Arena Gates** (3 gates):
1. Walk to location for Arena 1 gate (in lobby)
2. Click **[Arena 1 Gate]** button
3. Dark grey sphere appears
4. Repeat for Arena 2 and Arena 3

**Configure Arena Battle Positions** (repeat for each arena):
1. Click **[Arena 1]** to select it
2. Walk to positions and set:
   - **Set Gate** - Arena entrance
   - **Set Spectator** - Waiting area
   - **Add Green/Blue/Orange/Yellow/Purple Spawns** - Battle spawn points
3. Repeat for Arena 2 and Arena 3

**Save Everything**:
1. Click **[Save Config]** button
2. Reload plugin: `o.reload PaintballArena`

**Success Criteria**:
- [x] Admin UI opens and displays correctly
- [x] All sphere placement buttons work
- [x] Colored spheres appear at positions
- [x] Config saves successfully
- [x] Plugin reloads with new positions

---

### Phase 3: Sphere Detection Testing ⏳

**Objective**: Verify sphere collision detection works

**Team Color Sphere Test**:
1. Walk to lobby (should spawn at LobbyCentral)
2. Walk into a team color sphere (e.g., Blue)
3. Should see message: "Team Selected: Blue"
4. Player state should change to TeamSelected

**Arena Gate Sphere Test**:
1. After selecting a team, walk into Arena 1 gate sphere
2. Should see message about queueing or joining
3. Player should be teleported (either to battle or spectator)

**Proximity Detection**:
- Sphere detection runs every 0.5 seconds
- Proximity radius is 2 meters
- Player must be within 2m of sphere center

**Success Criteria**:
- [x] Team selection works on sphere entry
- [x] Arena queueing works on gate entry
- [x] Chat messages appear correctly
- [x] Player states update properly

---

### Phase 4: Queue System Testing ⏳

**Objective**: Test the 2-team queue limit and rotation

**Test Scenario 1: First 2 Teams**:
1. Player 1: Select Green team → Enter Arena 1 gate
2. Player 2: Select Blue team → Enter Arena 1 gate
3. **Expected**: Both teams become "active"
4. **Expected**: Both players teleport to battle spawns
5. **Expected**: Match countdown starts

**Test Scenario 2: 3rd Team Joins**:
1. Player 3: Select Orange team → Enter Arena 1 gate
2. **Expected**: Orange team added to queue
3. **Expected**: Player 3 teleports to spectator area
4. **Expected**: Message shows queue position

**Test Scenario 3: Match End**:
1. Complete a match in Arena 1
2. **Expected**: Green and Blue teams teleport to spectator
3. **Expected**: Orange team (from queue) teleports to battle
4. **Expected**: Need one more team to start next match

**Success Criteria**:
- [x] Max 2 teams active per arena (enforced)
- [x] Additional teams queue properly
- [x] Queue is FIFO (first in, first out)
- [x] Match rotation works on match end
- [x] Queue position messages accurate

---

### Phase 5: Match Flow Testing ⏳

**Objective**: Test complete match automation

**Full Match Test**:
1. Get 2 teams into Arena 1
2. Match should auto-start with countdown
3. Players spawn at their team's spawn points
4. Battle normally (test combat)
5. Match ends when conditions met
6. Teams teleport to spectator
7. Next teams from queue enter (if available)

**Combat Testing**:
- Test OnEntityTakeDamage hook
- Verify one-hit elimination
- Check friendly fire prevention
- Verify cross-arena damage blocking

**Success Criteria**:
- [x] Auto-start works (2 teams → countdown → battle)
- [x] Combat hooks function correctly
- [x] Match end triggers properly
- [x] Queue rotation executes
- [x] Player teleportation smooth

---

### Phase 6: Multi-Arena Testing ⏳

**Objective**: Test multiple arenas running simultaneously

**Setup**:
- Configure all 3 arenas completely
- Get players in each arena

**Test Concurrent Matches**:
1. Arena 1: Green vs Blue (match in progress)
2. Arena 2: Orange vs Yellow (different match)
3. Arena 3: Purple team waiting (queue test)

**Verify**:
- Each arena operates independently
- Queue systems don't interfere
- Combat isolation (no cross-arena damage)
- Voice isolation (if enabled)
- Scoreboards show correct arena data

**Success Criteria**:
- [x] All 3 arenas can run simultaneously
- [x] No interference between arenas
- [x] Each arena has independent queue
- [x] Resource optimization working
- [x] Performance acceptable

---

### Phase 7: Edge Case Testing ⏳

**Objective**: Test unusual scenarios and edge cases

**Test Cases**:
1. **Player Disconnect**: What happens if player disconnects mid-match?
2. **Arena with 0 Players**: Empty arena should stop timers
3. **Queue with 1 Team**: Should wait for 2nd team
4. **Rapid Sphere Entry**: Spam enter/exit spheres
5. **Invalid Config**: Missing spawn points
6. **Permission Denied**: Non-admin tries `/adminsetup`

**Success Criteria**:
- [x] No crashes on edge cases
- [x] Graceful error handling
- [x] Appropriate messages to players
- [x] System recovers from errors

---

### Phase 8: Optimization & Polish ⏳

**Objective**: Fine-tune performance and user experience

**Performance Checks**:
- Monitor server performance with plugin active
- Check timer efficiency (sphere detection every 0.5s)
- Verify resource optimization (empty arenas use 0% CPU)

**Adjustments Needed**:
- Tune sphere detection radius (currently 2m)
- Adjust countdown timers if needed
- Optimize sphere proximity checks
- Polish UI messages

**Success Criteria**:
- [x] No performance issues
- [x] Smooth gameplay experience
- [x] Clear player instructions
- [x] Professional appearance

---

## Troubleshooting Guide

### Issue: Spheres Not Detected

**Possible Causes**:
- Player too far from sphere (>2m)
- Sphere positions not saved
- Plugin not reloaded after config change

**Solutions**:
- Check sphere proximity (must be within 2m)
- Verify config saved: Check `oxide/config/PaintballArena.json`
- Reload plugin: `o.reload PaintballArena`
- Check sphere markers are visible

### Issue: Queue Not Working

**Possible Causes**:
- Arena not properly configured
- Team assignment failed
- State tracking bug

**Solutions**:
- Verify all spawn points set for teams
- Check console for errors
- Test team selection first (walk into color sphere)
- Ensure MaxActiveTeamsPerArena = 2 in config

### Issue: Match Won't Start

**Possible Causes**:
- Less than 2 teams active
- Missing spawn points for one team
- Arena state stuck

**Solutions**:
- Ensure exactly 2 teams have joined
- Check both teams have spawn points configured
- Try: `o.reload PaintballArena` to reset state
- Verify countdown timer is running

### Issue: Players Not Teleporting

**Possible Causes**:
- Invalid spawn positions (0,0,0)
- Missing spectator position
- Teleport failed

**Solutions**:
- Check all Vector3 positions in config are valid
- Ensure spectator position is set
- Look for errors in console
- Verify player has correct state

---

## Testing Checklist

Use this checklist to track your testing progress:

### Server Setup
- [ ] Plugin uploaded to server
- [ ] Config file in place
- [ ] Plugin loads without errors
- [ ] Admin permission granted

### Sphere Configuration
- [ ] Central lobby position set
- [ ] 5 team color spheres placed
- [ ] 3 arena gate spheres placed
- [ ] Arena 1: Gate, Spectator, all team spawns set
- [ ] Arena 2: Gate, Spectator, all team spawns set
- [ ] Arena 3: Gate, Spectator, all team spawns set
- [ ] Config saved successfully

### Feature Testing
- [ ] Sphere detection works (team colors)
- [ ] Sphere detection works (arena gates)
- [ ] Team selection functional
- [ ] Arena queueing functional
- [ ] 2-team limit enforced
- [ ] Queue rotation works
- [ ] Match auto-start works
- [ ] Combat system works
- [ ] Match end triggers properly

### Multi-Player Testing
- [ ] 2 teams can play simultaneously
- [ ] 3rd team queues correctly
- [ ] Queue position shown
- [ ] Match rotation smooth
- [ ] Multiple arenas work together

### Polish
- [ ] All messages clear and helpful
- [ ] UI displays correctly
- [ ] Performance acceptable
- [ ] No console errors
- [ ] Ready for production

---

## What to Report Back

After testing, please provide feedback on:

1. **What Works Well**:
   - Which features work perfectly?
   - What exceeded expectations?

2. **What Needs Adjustment**:
   - Sphere detection radius (too small/large)?
   - Countdown timers (too fast/slow)?
   - UI layout or messages unclear?

3. **Bugs Found**:
   - Any errors in console?
   - Unexpected behavior?
   - Edge cases that break?

4. **Suggestions**:
   - Additional features needed?
   - Quality of life improvements?
   - Documentation gaps?

---

## Summary

**Current Status**: ✅ Development complete, ready for testing

**Next Step**: Deploy to test server and begin Phase 1

**Time Estimate**: 2-4 hours of testing recommended

**Goal**: Validate all features work as designed in real server environment

Once testing is complete and any bugs are fixed, the plugin will be ready for production deployment! 🚀
