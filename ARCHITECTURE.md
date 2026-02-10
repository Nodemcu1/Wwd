# PaintballArena - Technical Architecture

## System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                     PaintballArena Plugin                        │
│                         Main Class                               │
└───────────────────┬─────────────────────────────────────────────┘
                    │
        ┌───────────┴───────────┐
        │   Core Data Structures │
        └───────────┬───────────┘
                    │
        ┌───────────┼───────────────────┐
        │           │                   │
        ▼           ▼                   ▼
┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│playerArenaMap│ │arenaInstances│ │   config    │
│<ulong,Arena>│ │<int,Arena>   │ │PluginConfig │
└─────────────┘ └─────────────┘ └─────────────┘
                      │
              ┌───────┼───────┐
              │       │       │
              ▼       ▼       ▼
        ┌─────────────────────────┐
        │  ArenaInstance Objects  │
        └─────────────────────────┘
        │ Arena 1 │ Arena 2 │ Arena 3 │
        └─────────────────────────┘
```

## ArenaInstance Class Structure

```
┌──────────────────────────────────────────────────────────────┐
│                     ArenaInstance                             │
├──────────────────────────────────────────────────────────────┤
│ Properties:                                                   │
│  - ArenaId: int                                              │
│  - Mode: string (5v5_TDM, 2v2_Chamber, 1v1_Chamber)         │
│  - Config: ArenaConfig                                       │
│  - State: ArenaState (Waiting, Countdown, InProgress, Ended) │
│  - Teams: Dictionary<string, List<ulong>>                    │
│  - Score: Dictionary<string, int>                            │
│  - Spectators: List<ulong>                                   │
│  - CurrentRound: int                                         │
│  - RoundTimer: Timer ⚡ (INDEPENDENT)                        │
│  - CountdownTimer: Timer ⚡ (INDEPENDENT)                    │
│  - IsActive: bool                                            │
├──────────────────────────────────────────────────────────────┤
│ Methods:                                                      │
│  + CheckAndStartMatch()                                      │
│  + StartMatch()                                              │
│  + StartRoundTimer()                                         │
│  + PauseTimer()                                              │
│  + DistributeLoadouts()                                      │
│  + GiveLoadout(player)                                       │
│  + GetAmmoForMode()                                          │
│  + SpawnAllPlayers()                                         │
│  + HandleElimination(victim, attacker)                       │
│  + EndRound(reason)                                          │
│  + EndMatch()                                                │
│  + BroadcastToArena(message)                                 │
│  + CleanupTimers()                                           │
└──────────────────────────────────────────────────────────────┘
```

## Game Flow State Machine

```
     ┌─────────────────────┐
     │ WaitingForPlayers   │ ◄─── Initial State
     └──────────┬──────────┘
                │
                │ Players >= MinPlayersToStart
                ▼
     ┌─────────────────────┐
     │     Countdown       │
     │   (10 seconds)      │
     └──────────┬──────────┘
                │
                │ Countdown Complete
                ▼
     ┌─────────────────────┐
     │    InProgress       │ ◄───┐
     │  (Round Active)     │     │
     └──────────┬──────────┘     │
                │                 │
                │ Team Eliminated │ Next Round
                │ OR Time Expired │
                ▼                 │
     ┌─────────────────────┐     │
     │   Round End         │─────┘
     │  (5 sec pause)      │
     └──────────┬──────────┘
                │
                │ Max Rounds Reached
                │ OR Winner Determined
                ▼
     ┌─────────────────────┐
     │      Ended          │
     │ (Return to Lobby)   │
     └─────────────────────┘
                │
                │ Reset (10 seconds)
                ▼
     ┌─────────────────────┐
     │ WaitingForPlayers   │
     └─────────────────────┘
```

## Combat Logic Flow

```
OnEntityTakeDamage Hook
         │
         ▼
┌─────────────────────┐
│ Is victim a player? │──No──► Return
└─────────┬───────────┘
          │ Yes
          ▼
┌──────────────────────┐
│ Is attacker a player?│──No──► Return
└─────────┬────────────┘
          │ Yes
          ▼
┌─────────────────────────────┐
│ Both in playerArenaMap?     │──No──► Return
└─────────┬───────────────────┘
          │ Yes
          ▼
┌─────────────────────────────┐
│ Same ArenaInstance?         │──No──► Cancel Damage, Return
└─────────┬───────────────────┘         (Cross-arena protection)
          │ Yes
          ▼
┌─────────────────────────────┐
│ Same Team?                  │──Yes──► Cancel Damage, Return
└─────────┬───────────────────┘         (Friendly fire protection)
          │ No
          ▼
┌─────────────────────────────┐
│ Clear Damage                │
│ Call HandleElimination()    │
└─────────────────────────────┘
          │
          ▼
┌─────────────────────────────┐
│ Move victim to spectators   │
│ Teleport to spectator pos   │
│ Award point to attacker team│
│ Refund ammo (if Chamber)    │
│ Broadcast kill message      │
│ Check round end conditions  │
│ Update UI                   │
└─────────────────────────────┘
```

## Player Mapping Architecture

```
Player Joins Arena 1, Blue Team
         │
         ▼
┌─────────────────────────────────────┐
│ playerArenaMap[userID] = arena1     │
│ arena1.Teams["Blue"].Add(userID)    │
└─────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────┐
│ All Hooks Check playerArenaMap:     │
│  - OnEntityTakeDamage               │
│  - OnPlayerVoice                    │
│  - BroadcastToArena                 │
│  - UpdateScoreboard                 │
└─────────────────────────────────────┘
```

## Multi-Arena Concurrency Model

```
Server Running 3 Concurrent Arenas:

┌────────────────┐  ┌────────────────┐  ┌────────────────┐
│   Arena 1      │  │   Arena 2      │  │   Arena 3      │
│   5v5 TDM      │  │  2v2 Chamber   │  │  1v1 Chamber   │
├────────────────┤  ├────────────────┤  ├────────────────┤
│ State: Active  │  │ State: Active  │  │ State: Waiting │
│ Round: 3/10    │  │ Round: 2/5     │  │ Round: 0/3     │
│ Blue: 5 ⚔ Red:5│  │ Blue: 2 ⚔ Red:2│  │ Blue: 0 ⚔ Red:0│
│ Score: 2-1     │  │ Score: 1-1     │  │ Score: 0-0     │
│                │  │                │  │                │
│ Timer: 120s ⏱  │  │ Timer: 45s ⏱   │  │ Timer: None    │
│ (Independent)  │  │ (Independent)  │  │ (Inactive)     │
└────────────────┘  └────────────────┘  └────────────────┘
     │                   │                   │
     │                   │                   │
     └───────────────────┴───────────────────┘
                         │
              ┌──────────┴──────────┐
              │ Resource Optimizer  │
              │  (Runs every 30s)   │
              │                     │
              │ Checks Arena 3:     │
              │ - 0 players         │
              │ - IsActive = false  │
              │ - Timers = null     │
              │ → No CPU usage      │
              └─────────────────────┘
```

## UI System Architecture

```
UpdateScoreboardForArena(arena1)
         │
         ▼
┌─────────────────────────────────────┐
│ Get all players in arena1:          │
│  - Teams["Blue"]                    │
│  - Teams["Red"]                     │
│  - Spectators                       │
└────────────┬────────────────────────┘
             │
             ▼
┌─────────────────────────────────────┐
│ For each player in arena1:          │
│   CreateScoreboard(player, arena1)  │
└────────────┬────────────────────────┘
             │
             ▼
┌─────────────────────────────────────┐
│ CUI Elements:                       │
│  ┌────────────────────────────────┐ │
│  │ Arena 1 - 5v5_TDM              │ │
│  │ Blue: 3 | Red: 2               │ │
│  │ Round 4/10                     │ │
│  └────────────────────────────────┘ │
└─────────────────────────────────────┘

Note: Players in Arena 2 see different UI:
┌─────────────────────────────────────┐
│ CUI Elements:                       │
│  ┌────────────────────────────────┐ │
│  │ Arena 2 - 2v2_Chamber          │ │
│  │ Blue: 1 | Red: 1               │ │
│  │ Round 2/5                      │ │
│  └────────────────────────────────┘ │
└─────────────────────────────────────┘
```

## Configuration Structure

```
PaintballArena.json
│
├── Arena 1 Settings
│   ├── Arena ID: 1
│   ├── Game Mode: "5v5_TDM"
│   ├── Min Players: 2
│   ├── Max Rounds: 10
│   ├── Round Time: 300s
│   ├── Gate Position: Vector3
│   ├── Spectator Position: Vector3
│   └── Team Spawns
│       ├── Blue: [Vector3, Vector3, ...]
│       └── Red: [Vector3, Vector3, ...]
│
├── Arena 2 Settings
│   └── (Same structure, different values)
│
├── Arena 3 Settings
│   └── (Same structure, different values)
│
└── Global Settings
    ├── Lobby Position: Vector3
    ├── Enable Voice Isolation: bool
    └── Voice Isolation Distance: float
```

## Loadout Distribution Logic

```
Match Starts
     │
     ▼
DistributeLoadouts()
     │
     ▼
For each player in arena:
     │
     ▼
GiveLoadout(player)
     │
     ├─► Clear inventory
     │
     ├─► Give weapon (pistol)
     │
     └─► GetAmmoForMode()
          │
          ├─► Mode contains "Chamber" → 1 ammo
          ├─► Mode contains "5v5" → 128 ammo
          ├─► Mode contains "2v2" → 1 ammo
          └─► Mode contains "1v1" → 1 ammo
```

## Performance Optimization

```
┌─────────────────────────────────────┐
│  OptimizeArenaResources()           │
│  (Called every 30 seconds)          │
├─────────────────────────────────────┤
│                                     │
│  For each arena:                    │
│    If GetTotalPlayers() == 0        │
│    AND IsActive == true             │
│    THEN:                            │
│      - Call arena.Reset()           │
│      - Destroy RoundTimer           │
│      - Destroy CountdownTimer       │
│      - Set IsActive = false         │
│      - Clear Teams                  │
│      - Clear Spectators             │
│                                     │
│  Result:                            │
│    Empty arenas = Zero CPU usage    │
│    Active arenas = Normal CPU usage │
└─────────────────────────────────────┘
```

## Key Design Principles

1. **Isolation**: Each arena is completely isolated from others
2. **Independence**: Timers, teams, scores are per-arena
3. **Scalability**: Easy to add Arena 4, 5, etc.
4. **Efficiency**: Inactive arenas don't consume resources
5. **Simplicity**: Clear data structures and logic flow
6. **Safety**: Cross-arena damage/interference prevented

## Thread Safety Note

While Rust/Oxide is primarily single-threaded, the plugin uses:
- Independent timer instances (no shared state)
- Per-arena data structures (no race conditions)
- Dictionary-based lookups (O(1) performance)

## Memory Footprint

Approximate per active arena:
- ArenaInstance object: ~2KB
- Player lists (10 players): ~1KB
- Timers: ~0.5KB
- Total per arena: ~3.5KB

With all 3 arenas active and 30 total players:
- Total plugin memory: ~15KB
- Negligible server impact

## Conclusion

This architecture provides:
✅ True concurrent multi-arena support
✅ Complete instance isolation
✅ Independent timer management
✅ Scalable design
✅ Performance optimization
✅ Clean separation of concerns
