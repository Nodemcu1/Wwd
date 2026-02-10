# Queue-Based Arena System - Visual Flow Diagram

## Current System vs. Requested System

### CURRENT SYSTEM (Multi-team Free Join)

```
┌──────────────────────────────────────────────────┐
│              PLAYER JOINS SERVER                 │
└─────────────────┬────────────────────────────────┘
                  │
                  ▼
         ┌─────────────────┐
         │ Types Command:  │
         │ /arena join 1   │
         │      Blue       │
         └────────┬────────┘
                  │
                  ▼
         ┌─────────────────┐
         │ INSTANT ENTRY   │
         │   Arena 1       │
         │   Blue Team     │
         └─────────────────┘
                  
Multiple teams can play simultaneously in each arena
No waiting, no queue, instant action
```

### REQUESTED SYSTEM (Queue-Based Tournament)

```
┌──────────────────────────────────────────────────┐
│              PLAYER JOINS SERVER                 │
└─────────────────┬────────────────────────────────┘
                  │
                  ▼
         ┌─────────────────┐
         │  CENTRAL LOBBY  │
         │   (Spawn Here)  │
         └────────┬────────┘
                  │
         ┌────────┴────────────────────────┐
         │  Walk to TEAM COLOR SPHERE:     │
         │  🟢 Green  🔵 Blue  🟠 Orange   │
         │  🟡 Yellow  🟣 Purple            │
         └────────┬────────────────────────┘
                  │
                  ▼
         ┌─────────────────┐
         │ TEAM SELECTED   │
         │ (e.g., Blue)    │
         └────────┬────────┘
                  │
         ┌────────┴────────────────────────┐
         │  Walk to ARENA GATE SPHERE:     │
         │  Gate 1  Gate 2  Gate 3         │
         └────────┬────────────────────────┘
                  │
                  ▼
         ┌─────────────────┐
         │ Added to Queue  │
         │   Arena 1       │
         │   Blue Team     │
         └────────┬────────┘
                  │
         ┌────────┴────────┐
         │                 │
         ▼                 ▼
   ┌─────────┐      ┌─────────────┐
   │ < 2 Teams│      │ >= 2 Teams  │
   │in Queue │      │  Active     │
   └────┬────┘      └──────┬──────┘
        │                   │
        ▼                   ▼
   ┌─────────┐      ┌─────────────┐
   │ START   │      │ WAIT IN     │
   │ MATCH   │      │ SPECTATOR   │
   │ NOW!    │      │ AREA        │
   └────┬────┘      └──────┬──────┘
        │                   │
        │                   │
        ▼                   ▼
   ┌─────────────────────────────┐
   │      BATTLE ARENA          │
   │   (2 teams fighting)        │
   └────────────┬────────────────┘
                │
                ▼
   ┌─────────────────────────────┐
   │      MATCH ENDS             │
   │   (One team wins round)     │
   └────────────┬────────────────┘
                │
         ┌──────┴──────┐
         │             │
         ▼             ▼
   ┌──────────┐   ┌──────────┐
   │ Both teams│   │Pull next │
   │   Go to   │   │2 teams   │
   │ Spectator │──▶│from queue│
   └──────────┘   └─────┬────┘
                        │
                        ▼
                  ┌──────────┐
                  │ NEW MATCH│
                  │  STARTS  │
                  └──────────┘
```

## Lobby Layout

### Physical Sphere Arrangement

```
                CENTRAL LOBBY
                =============
                
                    ⭐
                   SPAWN
                    │
        ┌───────────┼───────────┐
        │           │           │
        │    TEAM SELECTION     │
        │    ───────────────    │
        │                       │
        │   🟢      🔵      🟠  │
        │  Green   Blue  Orange │
        │                       │
        │   🟡      🟣          │
        │  Yellow Purple        │
        │                       │
        └───────────────────────┘
                    │
                    │
        ┌───────────┼───────────┐
        │           │           │
        │   ARENA GATE SELECTION│
        │   ──────────────────  │
        │                       │
        │   🚪        🚪        🚪│
        │  Arena 1  Arena 2  Arena 3│
        │  (5v5)    (2v2)    (1v1) │
        │                       │
        └───────────────────────┘
```

## Example Player Journey

### Scenario: 6 Players Want to Play Arena 1

```
Player 1: Walks into 🟢 Green sphere  → Team: Green
Player 2: Walks into 🟢 Green sphere  → Team: Green  
Player 3: Walks into 🔵 Blue sphere   → Team: Blue
Player 4: Walks into 🔵 Blue sphere   → Team: Blue
Player 5: Walks into 🟠 Orange sphere → Team: Orange
Player 6: Walks into 🟡 Yellow sphere → Team: Yellow

All walk into Arena 1 gate 🚪

Queue Status:
┌─────────────────────────────┐
│ Arena 1 Queue:              │
├─────────────────────────────┤
│ 1. Green Team  ← PLAYING    │
│ 2. Blue Team   ← PLAYING    │
│ 3. Orange Team ← WAITING    │
│ 4. Yellow Team ← WAITING    │
└─────────────────────────────┘

Match 1: Green vs Blue (playing now)
         Orange and Yellow wait in spectator

Match 1 Ends → Green and Blue go to spectator

Match 2: Orange vs Yellow (auto-starts)
         Green and Blue watch from spectator

Match 2 Ends → Orange and Yellow go to spectator

Match 3: Green vs Blue (auto-starts again)
         And so on...
```

## Queue Mechanics

### Active Teams vs Queue

```
Each Arena Has:
├── Active Teams (max 2)
│   ├── Team 1 (currently fighting)
│   └── Team 2 (currently fighting)
│
└── Queue (unlimited)
    ├── Team 3 (waiting in spectator)
    ├── Team 4 (waiting in spectator)
    ├── Team 5 (waiting in spectator)
    └── ...

When Match Ends:
1. Active Teams (1 & 2) → Spectator Area
2. Queue Teams (3 & 4) → Become Active
3. Remaining Queue (5+) → Still waiting
4. New Match Auto-Starts
```

## Admin Configuration

### What Admins Must Set Up

```
Using /adminsetup UI:

GLOBAL SETTINGS:
├── Set Lobby Central Position (1 sphere) 🟣
│
├── Set Team Color Spheres (5 spheres)
│   ├── Set Green Sphere 🟢
│   ├── Set Blue Sphere 🔵
│   ├── Set Orange Sphere 🟠
│   ├── Set Yellow Sphere 🟡
│   └── Set Purple Sphere 🟣
│
└── Set Arena Gate Spheres (3 spheres)
    ├── Set Arena 1 Gate 🚪
    ├── Set Arena 2 Gate 🚪
    └── Set Arena 3 Gate 🚪

PER-ARENA SETTINGS (for each arena 1, 2, 3):
├── Set Spectator Position 🟡
│
└── Set Team Spawn Points
    ├── Add Green Spawn 🟢
    ├── Add Blue Spawn 🔵
    ├── Add Orange Spawn 🟠
    ├── Add Yellow Spawn 🟡
    └── Add Purple Spawn 🟣
```

## Key Differences from Current System

| Feature | Current System | New Queue System |
|---------|---------------|------------------|
| Teams per Arena | Unlimited | 2 active + queue |
| Join Method | `/arena join` command | Walk into spheres |
| Lobby Structure | Per-arena | Single central |
| Entry Speed | Instant | Queue-based |
| Team Selection | At arena | In lobby first |
| Waiting Area | None | Spectator queue |
| Match Rotation | Manual | Automatic |

## Benefits of Queue System

✅ **Fair Play**: Everyone gets a turn, no overcrowding
✅ **Tournament Style**: Professional match rotation
✅ **Clear Flow**: Physical spheres guide players
✅ **Spectating**: Teams watch while waiting
✅ **Automatic**: Matches start/rotate without admin intervention
✅ **Organized**: Queue order preserved

## Drawbacks to Consider

⚠️ **Waiting Time**: Players may wait in queue
⚠️ **Slower Entry**: Can't instantly jump into battle
⚠️ **Complexity**: More steps to join match
⚠️ **Learning Curve**: Players must learn sphere locations

---

This visual guide shows exactly how the new system will work. The fundamental change is from **instant free-join** to **queue-based tournament rotation**.
