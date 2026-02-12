# Queue-Based 1v1 Team Battle System Implementation

## Overview

This document outlines the major architectural changes needed to implement the queue-based team battle system as requested.

## Requirements

1. **1v1 Team Battles Only** - Each arena supports exactly 2 teams playing at once
2. **Queue System** - Additional teams wait in spectator area for their turn
3. **Single Central Lobby** - One lobby for all arenas with:
   - 5 team color selection spheres (Green, Blue, Orange, Yellow, Purple)
   - 3 arena gate spheres (Arena 1, 2, 3)
4. **Player Flow**:
   - Enter lobby → Select team (walk into color sphere) → Select arena (walk into gate) → Queue → Battle

## Architecture Changes

### 1. Configuration Changes

**Add to GlobalSettings:**
```csharp
public Vector3 LobbyCentral { get; set; }
public Dictionary<string, Vector3> TeamColorSpheres { get; set; }  // 5 team spheres in lobby
public List<Vector3> ArenaGateSpheres { get; set; }  // 3 arena gates in lobby
```

**Remove from ArenaConfig:**
- `TeamSelectionSpheres` (no longer per-arena, now in global lobby)

### 2. Player State Tracking

**Add PlayerState enum:**
```csharp
public enum PlayerState
{
    None,
    InLobby,
    TeamSelected,
    InQueue,
    InBattle,
    Spectating
}
```

**Add player tracking dictionaries:**
```csharp
private Dictionary<ulong, PlayerState> playerStates = new Dictionary<ulong, PlayerState>();
private Dictionary<ulong, string> playerSelectedTeams = new Dictionary<ulong, string>();  // Team before arena selection
private Dictionary<ulong, int> playerQueuedArena = new Dictionary<ulong, int>();
```

### 3. Arena Queue System

**Add to ArenaInstance:**
```csharp
public List<string> TeamQueue { get; set; } = new List<string>();  // Queue of team names
public List<string> ActiveTeams { get; set; } = new List<string>();  // 2 active teams
public const int MAX_ACTIVE_TEAMS = 2;
```

**Queue Logic:**
- When a player selects arena gate, add their team to arena's queue
- If queue has < 2 teams, add to queue
- If queue has >= 2 teams and active teams < 2, move 2 teams to active and start match
- If active teams == 2, keep additional teams in queue (spectating)

### 4. Sphere Collision Detection

**Use OnPlayerInput hook:**
```csharp
private void OnPlayerInput(BasePlayer player, InputState input)
{
    if (player == null) return;
    
    // Check proximity to team color spheres
    foreach (var kvp in config.Global.TeamColorSpheres)
    {
        if (Vector3.Distance(player.transform.position, kvp.Value) < 2f)
        {
            AssignPlayerToTeam(player, kvp.Key);  // kvp.Key = team name
            break;
        }
    }
    
    // Check proximity to arena gate spheres
    for (int i = 0; i < config.Global.ArenaGateSpheres.Count; i++)
    {
        if (Vector3.Distance(player.transform.position, config.Global.ArenaGateSpheres[i]) < 2f)
        {
            QueuePlayerForArena(player, i + 1);  // i+1 = arena number (1, 2, 3)
            break;
        }
    }
}
```

### 5. Team Assignment Flow

**AssignPlayerToTeam:**
1. Set `playerSelectedTeams[player.userID] = teamName`
2. Set `playerStates[player.userID] = PlayerState.TeamSelected`
3. Send message: "You joined [COLOR] team! Walk into an arena gate to play."

**QueuePlayerForArena:**
1. Get player's selected team from `playerSelectedTeams`
2. If no team selected, send error message
3. Add team to arena's queue
4. Set `playerQueuedArena[player.userID] = arenaId`
5. Set `playerStates[player.userID] = PlayerState.InQueue`
6. Call `CheckAndStartMatch(arenaId)`

### 6. Match Start Logic

**CheckAndStartMatch:**
```csharp
private void CheckAndStartMatch(int arenaId)
{
    var arena = arenaInstances[arenaId];
    
    // If already 2 active teams, just add to queue
    if (arena.ActiveTeams.Count >= 2) return;
    
    // If queue has at least 2 teams, start match
    if (arena.TeamQueue.Count >= 2)
    {
        // Take first 2 teams from queue
        var team1 = arena.TeamQueue[0];
        var team2 = arena.TeamQueue[1];
        arena.TeamQueue.RemoveAt(0);
        arena.TeamQueue.RemoveAt(0);
        
        // Add to active teams
        arena.ActiveTeams.Add(team1);
        arena.ActiveTeams.Add(team2);
        
        // Teleport players to spawns
        TeleportTeamToSpawns(arenaId, team1);
        TeleportTeamToSpawns(arenaId, team2);
        
        // Start match timer
        StartMatch(arena);
        
        // Update player states
        foreach (var player in GetPlayersInTeam(team1))
            playerStates[player.userID] = PlayerState.InBattle;
        foreach (var player in GetPlayersInTeam(team2))
            playerStates[player.userID] = PlayerState.InBattle;
    }
}
```

### 7. Match End Logic

**OnMatchEnd:**
```csharp
private void OnMatchEnd(ArenaInstance arena)
{
    // Move both teams to spectator
    foreach (var teamName in arena.ActiveTeams)
    {
        TeleportTeamToSpectator(arena.ArenaId, teamName);
        
        // Update player states
        foreach (var player in GetPlayersInTeam(teamName))
            playerStates[player.userID] = PlayerState.Spectating;
    }
    
    // Clear active teams
    arena.ActiveTeams.Clear();
    
    // Check if there are teams waiting in queue
    CheckAndStartMatch(arena.ArenaId);
}
```

### 8. Admin UI Changes

**Remove:**
- Per-arena team selection sphere buttons

**Add:**
- "Set Lobby Central" button (global)
- "Set Team Color Sphere" buttons (5 buttons for Green/Blue/Orange/Yellow/Purple in global lobby)
- "Set Arena Gate" buttons (3 buttons for Arena 1/2/3 gates in global lobby)

**Console Commands:**
- `adminsetup.setlobbycent ral`
- `adminsetup.setteamgreen` (for lobby team sphere)
- `adminsetup.setteamblue` (for lobby team sphere)
- `adminsetup.setteamorange` (for lobby team sphere)
- `adminsetup.setteamyellow` (for lobby team sphere)
- `adminsetup.setteampurple` (for lobby team sphere)
- `adminsetup.setarenagate1`
- `adminsetup.setarenagate2`
- `adminsetup.setarenagate3`

### 9. Player Commands

**Remove:**
- `/arena join <arena> <team>` (players must use spheres now)

**Add:**
- `/arena leave` - Return to lobby
- `/arena queue` - Show your queue position
- `/arena status` - Show all arenas and their queue status

## Implementation Steps

1. Add new configuration fields
2. Add player state tracking dictionaries
3. Add queue system to ArenaInstance
4. Implement OnPlayerInput hook for sphere detection
5. Implement AssignPlayerToTeam and QueuePlayerForArena methods
6. Implement CheckAndStartMatch logic
7. Update match end logic to process queue
8. Update admin UI for new sphere positions
9. Update player commands
10. Update JSON config file
11. Test complete player flow

## Testing Checklist

- [ ] Player spawns at lobby central on join
- [ ] Walking into team color sphere assigns team
- [ ] Walking into arena gate queues for arena
- [ ] 2 teams enter battle immediately when queued
- [ ] 3rd+ teams wait in spectator queue
- [ ] Match end triggers next teams from queue
- [ ] Admin UI sets all sphere positions correctly
- [ ] Queue status commands work
- [ ] Leave command returns to lobby

## Notes

This is a major architectural refactoring that changes the fundamental flow of the plugin from a "free join" system to a "queue-based tournament" system.
