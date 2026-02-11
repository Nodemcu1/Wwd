# UI System Implementation Plan

## Overview

The requested features represent a **major overhaul** of the arena system, adding comprehensive UI elements, side selection, confirmation dialogs, and a voting system. This document outlines what needs to be implemented.

---

## Features Requested

### 1. ✅ Change Modes to Normal (COMPLETE)
- Arena 2: 2v2_Chamber → 2v2_TDM
- Arena 3: 1v1_Chamber → 1v1_TDM

### 2. Side Selection System
**Current**: Players join arena and are auto-assigned to Side A or B based on queue
**Requested**: Players choose which side (A or B) they want to join

**Requirements**:
- Add side parameter to join command: `/arena join <1-3> <team> <A/B>`
- Store chosen side in PlayerInfo
- Validate side has available capacity
- Spawn players on their chosen side
- Balance enforcement (prevent all players choosing same side)

**Estimated Code**: ~200 lines

### 3. Confirmation Prompt
**Requested**: Show confirmation dialog before player joins arena

**Requirements**:
- Display UI panel when player tries to join
- Show: Arena number, Mode, Chosen team, Chosen side
- Buttons: "Confirm Join" and "Cancel"
- Timeout after 30 seconds (auto-cancel)
- Visual feedback

**Estimated Code**: ~150 lines (UI creation + logic)

### 4. Join/Leave UI Panel
**Requested**: Persistent UI panel for all players with join/leave buttons

**Requirements**:
- Always-visible UI panel (corner of screen)
- Shows current status: Arena, Team, Side, State
- "Join Arena" button (opens arena selection UI)
- "Leave Arena" button (if in arena)
- Dynamic updates based on player state
- Clean, non-intrusive design

**Components Needed**:
- Main status panel
- Arena selection menu (choose 1, 2, or 3)
- Team selection menu (choose color)
- Side selection menu (choose A or B)
- Confirmation dialog

**Estimated Code**: ~400 lines

### 5. Match Start/Stop Voting System
**Requested**: One player from each team gets captain controls, others vote

**Requirements**:

**Captain System**:
- Select one captain per active team per arena
- Captain selection logic (first to join, or highest rank)
- Captain gets "Start Match" and "Stop Match" buttons
- Captain UI panel with controls

**Voting System**:
- Non-captains see voting panel
- Buttons: "Vote Yes" and "Vote No"
- Display vote count in real-time
- Require majority to start/stop match
- Vote timeout (60 seconds)
- Reset votes after decision

**Match Flow**:
1. Captain clicks "Start Match"
2. Vote panel appears for all non-captains
3. Players vote yes/no
4. If >50% yes → Match starts
5. If >50% no → Match doesn't start
6. Same process for "Stop Match"

**Estimated Code**: ~500 lines

---

## Total Implementation Estimate

**Lines of Code**: ~1,250+ new lines
**UI Panels**: 6-8 different UI components
**New Classes**: VotingSystem, CaptainManager, UIManager
**Testing Time**: Extensive (multi-player testing required)

---

## Implementation Phases

### Phase 1: Data Structure Changes
**Add to PlayerInfo class**:
```csharp
public string ChosenSide { get; set; }  // "A" or "B"
public bool IsCaptain { get; set; }
public int? VoteChoice { get; set; }  // null, 1 (yes), 0 (no)
```

**Add to ArenaInstance class**:
```csharp
public Dictionary<string, ulong> TeamCaptains { get; set; }  // team -> playerID
public Dictionary<ulong, int> CurrentVotes { get; set; }  // playerID -> vote
public Timer VoteTimer { get; set; }
public string VoteType { get; set; }  // "start" or "stop"
```

### Phase 2: Side Selection Logic
- Modify `JoinArena()` to accept side parameter
- Add validation for side capacity
- Update spawn logic to use chosen side
- Add balance checks

### Phase 3: UI Component Creation
**Base UI Manager**:
```csharp
private void CreateJoinLeavePanel(BasePlayer player)
private void CreateArenaSelectionUI(BasePlayer player)
private void CreateTeamSelectionUI(BasePlayer player, int arenaId)
private void CreateSideSelectionUI(BasePlayer player, int arenaId, string team)
private void CreateConfirmationDialog(BasePlayer player, int arenaId, string team, string side)
private void CreateVotingPanel(BasePlayer player, string voteType)
private void CreateCaptainControls(BasePlayer player)
```

### Phase 4: Captain System
- Select captains when teams form
- Create captain UI with Start/Stop buttons
- Handle captain disconnect (transfer to next player)
- Show captain status to all players

### Phase 5: Voting System
- Initialize vote when captain clicks Start/Stop
- Display voting UI to all eligible players
- Track votes in real-time
- Count votes and execute result
- Timeout handling

### Phase 6: Integration & Testing
- Test with multiple players
- Test edge cases (disconnect during vote, etc.)
- Balance testing
- UI positioning and visibility
- Performance testing

---

## Challenges & Considerations

### UI Complexity
- Rust's CUI system is limited
- Need to manage multiple overlapping UIs
- UI cleanup on player disconnect critical
- Screen real estate limited

### Vote Management
- What if player disconnects during vote?
- Majority threshold (50%? 60%? 75%?)
- Minimum voter count required?
- Vote timeout duration
- Can captain override vote?

### Side Selection Balance
- Prevent all players choosing same side
- Force balance or allow imbalance?
- What if chosen side is full?
- Auto-switch players to balance?

### Captain Selection
- How to choose captain?
  - First to join team
  - Highest rank/level
  - Random selection
  - Vote for captain
- What if captain leaves?
- Can captain role be transferred?

### State Management
- Player state transitions become complex
- Many new UI states to track
- Cleanup becomes critical
- Race conditions possible

---

## Recommended Approach

Given the complexity, I recommend implementing in stages:

### Stage 1: Basic Side Selection (Recommended Start)
- Add side parameter to join command
- Simple validation
- No UI, just command-based
- Test and verify

### Stage 2: Simple Join/Leave Panel
- Basic status panel
- Join/Leave buttons only
- No fancy selections yet

### Stage 3: Selection UIs
- Add arena/team/side selection UIs
- Confirmation dialog
- Polish UX

### Stage 4: Voting System
- Implement captain selection
- Add voting logic
- Create voting UI

### Stage 5: Polish & Integration
- Refine all UIs
- Add animations/effects
- Comprehensive testing

---

## Alternative: Simplified Implementation

If full implementation is too complex, consider:

### Simplified Side Selection
- Auto-balance sides but respect preferences
- `/arena join <1-3> <team> preferside <A/B>`
- System tries to honor preference but may override for balance

### Simplified Start/Stop
- Admin command only: `/arena start <arena-id>`
- No voting, just admin control
- Simpler but less player engagement

### Simplified UI
- Chat-based confirmations instead of UI
- "Type 'yes' to confirm join"
- Less visual but easier to implement

---

## Current Status

✅ **Complete**: Mode changes (Chamber → TDM)
⏳ **In Progress**: Planning phase
🔲 **Not Started**: Implementation

**Recommendation**: Start with command-based side selection and simple confirmation, then gradually add UI elements based on testing and feedback.

---

## Questions for Clarification

1. **Side Balance**: Should system force equal sides or allow imbalance?
2. **Captain Selection**: How should captains be chosen?
3. **Vote Threshold**: What % of yes votes needed to start/stop?
4. **Vote Timeout**: How long should votes stay open?
5. **Captain Override**: Can captain start without vote if urgent?
6. **UI Preference**: Full UI or simplified command-based approach?
7. **Priority**: Which feature is most important to implement first?

---

## Estimated Timeline

**Full Implementation**: 15-20 hours of development + testing
**Simplified Version**: 5-8 hours of development + testing

This is a significant project that will fundamentally change how the arena system works.
