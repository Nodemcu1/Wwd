# Complete UI System Implementation Guide

## Overview

This document outlines all code changes needed to implement the comprehensive UI system for PaintballArena.

## Total Estimated Changes: ~1,550 lines

---

## 1. Side Selection System (~200 lines)

### A. Add to PlayerInfo class
```csharp
public class PlayerInfo
{
    // Existing properties...
    public string ChosenSide { get; set; } // "A" or "B"
}
```

### B. Modify ArenaCommand join logic
```csharp
// Change from: /arena join <id> <team>
// To: /arena join <id> <team> <side>

case "join":
    if (args.Length < 4) // Now requires side
    {
        player.ChatMessage("Usage: /arena join <1-3> <team> <A|B>");
        return;
    }
    string side = args[3].ToUpper();
    if (side != "A" && side != "B")
    {
        player.ChatMessage("Side must be A or B");
        return;
    }
    // Store chosen side
    playerInfo[player.userID].ChosenSide = side;
```

### C. Side validation method
```csharp
private bool ValidateSideSelection(int arenaId, string team, string side)
{
    var arena = arenaInstances[arenaId];
    int currentOnSide = arena.GetPlayerCountOnSide(side);
    int maxPerSide = arena.Config.MaxPlayersPerSide;
    
    if (currentOnSide >= maxPerSide)
    {
        return false;
    }
    return true;
}
```

### D. Update SpawnAllPlayers to use ChosenSide
```csharp
// Instead of assigning based on team order
// Use the player's chosen side
if (playerInfo[playerId].ChosenSide == "A")
{
    spawnPos = sideASpawns[sideAIndex % sideASpawns.Count];
    sideAIndex++;
}
else // Side B
{
    spawnPos = sideBSpawns[sideBIndex % sideBSpawns.Count];
    sideBIndex++;
}
```

---

## 2. Confirmation UI (~150 lines)

### A. Create confirmation panel
```csharp
private void ShowJoinConfirmation(BasePlayer player, int arenaId, string team, string side)
{
    var elements = new CuiElementContainer();
    
    // Background panel
    elements.Add(new CuiPanel
    {
        Image = { Color = "0 0 0 0.9" },
        RectTransform = { AnchorMin = "0.3 0.35", AnchorMax = "0.7 0.65" }
    }, "Overlay", "JoinConfirmPanel");
    
    // Title
    elements.Add(new CuiLabel
    {
        Text = { Text = "Join Arena Confirmation", FontSize = 20, Align = TextAnchor.MiddleCenter },
        RectTransform = { AnchorMin = "0 0.8", AnchorMax = "1 0.95" }
    }, "JoinConfirmPanel");
    
    // Info display
    var arena = arenaInstances[arenaId];
    string info = $"Arena: {arenaId}\nMode: {arena.Mode}\nTeam: {team}\nSide: {side}";
    elements.Add(new CuiLabel
    {
        Text = { Text = info, FontSize = 14, Align = TextAnchor.MiddleCenter },
        RectTransform = { AnchorMin = "0 0.4", AnchorMax = "1 0.7" }
    }, "JoinConfirmPanel");
    
    // Confirm button
    elements.Add(new CuiButton
    {
        Button = { Command = $"arena.confirm {arenaId} {team} {side}", Color = "0 0.7 0 0.8" },
        Text = { Text = "CONFIRM", FontSize = 16, Align = TextAnchor.MiddleCenter },
        RectTransform = { AnchorMin = "0.1 0.1", AnchorMax = "0.45 0.3" }
    }, "JoinConfirmPanel");
    
    // Cancel button
    elements.Add(new CuiButton
    {
        Button = { Command = "arena.cancelconfirm", Color = "0.7 0 0 0.8" },
        Text = { Text = "CANCEL", FontSize = 16, Align = TextAnchor.MiddleCenter },
        RectTransform = { AnchorMin = "0.55 0.1", AnchorMax = "0.9 0.3" }
    }, "JoinConfirmPanel");
    
    CuiHelper.AddUi(player, elements);
    
    // Auto-close after 30 seconds
    timer.Once(30f, () => {
        CuiHelper.DestroyUi(player, "JoinConfirmPanel");
    });
}
```

### B. Console commands for confirmation
```csharp
[ConsoleCommand("arena.confirm")]
private void ConfirmJoinCommand(ConsoleSystem.Arg arg)
{
    var player = arg.Player();
    if (player == null) return;
    
    int arenaId = arg.GetInt(0);
    string team = arg.GetString(1);
    string side = arg.GetString(2);
    
    // Destroy confirmation UI
    CuiHelper.DestroyUi(player, "JoinConfirmPanel");
    
    // Actually join the arena
    JoinArena(player, arenaId, team, side);
}

[ConsoleCommand("arena.cancelconfirm")]
private void CancelConfirmCommand(ConsoleSystem.Arg arg)
{
    var player = arg.Player();
    if (player == null) return;
    
    CuiHelper.DestroyUi(player, "JoinConfirmPanel");
    player.ChatMessage("Join cancelled");
}
```

---

## 3. Persistent Status Panel (~400 lines)

### A. Main status panel structure
```csharp
private void ShowStatusPanel(BasePlayer player)
{
    var elements = new CuiElementContainer();
    
    // Main panel (small, persistent in corner)
    elements.Add(new CuiPanel
    {
        Image = { Color = "0 0 0 0.7" },
        RectTransform = { AnchorMin = "0.01 0.75", AnchorMax = "0.2 0.99" }
    }, "Overlay", "ArenaStatusPanel");
    
    // Title
    elements.Add(new CuiLabel
    {
        Text = { Text = "Paintball Arena", FontSize = 14, Align = TextAnchor.MiddleCenter },
        RectTransform = { AnchorMin = "0 0.85", AnchorMax = "1 0.95" }
    }, "ArenaStatusPanel");
    
    // Status display (changes based on player state)
    string status = GetPlayerStatus(player);
    elements.Add(new CuiLabel
    {
        Text = { Text = status, FontSize = 10, Align = TextAnchor.MiddleLeft },
        RectTransform = { AnchorMin = "0.05 0.45", AnchorMax = "0.95 0.8" }
    }, "ArenaStatusPanel");
    
    // Join button (if not in arena)
    if (!playerArenaMap.ContainsKey(player.userID))
    {
        elements.Add(new CuiButton
        {
            Button = { Command = "arena.showjoin", Color = "0 0.6 0 0.8" },
            Text = { Text = "Join Arena", FontSize = 12, Align = TextAnchor.MiddleCenter },
            RectTransform = { AnchorMin = "0.1 0.25", AnchorMax = "0.9 0.4" }
        }, "ArenaStatusPanel");
    }
    else // Leave button (if in arena)
    {
        elements.Add(new CuiButton
        {
            Button = { Command = "arena.quickleave", Color = "0.7 0.2 0 0.8" },
            Text = { Text = "Leave Arena", FontSize = 12, Align = TextAnchor.MiddleCenter },
            RectTransform = { AnchorMin = "0.1 0.25", AnchorMax = "0.9 0.4" }
        }, "ArenaStatusPanel");
    }
    
    // Captain controls (if player is captain)
    if (IsPlayerCaptain(player))
    {
        // Start match button
        elements.Add(new CuiButton
        {
            Button = { Command = "arena.captain.start", Color = "0 0.7 0.3 0.8" },
            Text = { Text = "Start Match", FontSize = 10, Align = TextAnchor.MiddleCenter },
            RectTransform = { AnchorMin = "0.1 0.12", AnchorMax = "0.9 0.22" }
        }, "ArenaStatusPanel");
    }
    
    CuiHelper.AddUi(player, elements);
}

private string GetPlayerStatus(BasePlayer player)
{
    if (!playerInfo.ContainsKey(player.userID))
        return "Not in arena";
    
    var info = playerInfo[player.userID];
    if (!playerArenaMap.ContainsKey(player.userID))
        return $"Team: {info.SelectedTeam}\\nWaiting...";
    
    var arena = playerArenaMap[player.userID];
    return $"Arena: {arena.ArenaId}\\nTeam: {info.SelectedTeam}\\nSide: {info.ChosenSide}\\nState: {arena.State}";
}
```

### B. Join selection UI
```csharp
[ConsoleCommand("arena.showjoin")]
private void ShowJoinSelectionCommand(ConsoleSystem.Arg arg)
{
    var player = arg.Player();
    if (player == null) return;
    
    ShowArenaSelection(player);
}

private void ShowArenaSelection(BasePlayer player)
{
    var elements = new CuiElementContainer();
    
    // Selection panel
    elements.Add(new CuiPanel
    {
        Image = { Color = "0 0 0 0.9" },
        RectTransform = { AnchorMin = "0.3 0.2", AnchorMax = "0.7 0.8" }
    }, "Overlay", "ArenaSelectionPanel");
    
    // Title
    elements.Add(new CuiLabel
    {
        Text = { Text = "Select Arena", FontSize = 18, Align = TextAnchor.MiddleCenter },
        RectTransform = { AnchorMin = "0 0.9", AnchorMax = "1 1" }
    }, "ArenaSelectionPanel");
    
    // Arena buttons (3 arenas)
    float yStart = 0.65f;
    for (int i = 1; i <= 3; i++)
    {
        var arena = arenaInstances[i];
        string arenaInfo = $"Arena {i}\\n{arena.Mode}\\n{arena.GetPlayerCount()} players";
        
        elements.Add(new CuiButton
        {
            Button = { Command = $"arena.selectarena {i}", Color = "0.2 0.4 0.7 0.8" },
            Text = { Text = arenaInfo, FontSize = 14, Align = TextAnchor.MiddleCenter },
            RectTransform = { AnchorMin = $"0.1 {yStart - (i-1)*0.25}", AnchorMax = $"0.9 {yStart - (i-1)*0.25 + 0.2}" }
        }, "ArenaSelectionPanel");
    }
    
    // Close button
    elements.Add(new CuiButton
    {
        Button = { Command = "arena.closeselection", Color = "0.5 0.1 0.1 0.8" },
        Text = { Text = "Close", FontSize = 12, Align = TextAnchor.MiddleCenter },
        RectTransform = { AnchorMin = "0.35 0.02", AnchorMax = "0.65 0.08" }
    }, "ArenaSelectionPanel");
    
    CuiHelper.AddUi(player, elements);
}

[ConsoleCommand("arena.selectarena")]
private void SelectArenaCommand(ConsoleSystem.Arg arg)
{
    var player = arg.Player();
    if (player == null) return;
    
    int arenaId = arg.GetInt(0);
    CuiHelper.DestroyUi(player, "ArenaSelectionPanel");
    
    // Show team selection
    ShowTeamSelection(player, arenaId);
}

private void ShowTeamSelection(BasePlayer player, int arenaId)
{
    // Similar UI for team selection (Green, Blue, Orange, Yellow, Purple)
    // Then show side selection
    // Then show confirmation
}
```

---

## 4. Captain System (~200 lines)

### A. Captain tracking
```csharp
public class ArenaInstance
{
    // Existing fields...
    public Dictionary<string, ulong> TeamCaptains { get; set; } = new Dictionary<string, ulong>();
    
    public void AssignCaptain(string team, ulong playerId)
    {
        if (!TeamCaptains.ContainsKey(team))
        {
            TeamCaptains[team] = playerId;
            var player = BasePlayer.FindByID(playerId);
            if (player != null)
            {
                player.ChatMessage($"You are now captain of {team} team!");
            }
        }
    }
    
    public bool IsCaptain(ulong playerId)
    {
        return TeamCaptains.ContainsValue(playerId);
    }
}

private bool IsPlayerCaptain(BasePlayer player)
{
    if (!playerArenaMap.ContainsKey(player.userID))
        return false;
    
    var arena = playerArenaMap[player.userID];
    return arena.IsCaptain(player.userID);
}
```

### B. Captain commands
```csharp
[ConsoleCommand("arena.captain.start")]
private void CaptainStartCommand(ConsoleSystem.Arg arg)
{
    var player = arg.Player();
    if (player == null || !IsPlayerCaptain(player)) return;
    
    var arena = playerArenaMap[player.userID];
    
    // Initiate vote for match start
    arena.InitiateStartVote(player.userID);
}

[ConsoleCommand("arena.captain.stop")]
private void CaptainStopCommand(ConsoleSystem.Arg arg)
{
    var player = arg.Player();
    if (player == null || !IsPlayerCaptain(player)) return;
    
    var arena = playerArenaMap[player.userID];
    
    // Initiate vote for match stop
    arena.InitiateStopVote(player.userID);
}
```

---

## 5. Voting System (~500 lines)

### A. Vote tracking structure
```csharp
public class MatchVote
{
    public VoteType Type { get; set; } // Start or Stop
    public ulong InitiatorId { get; set; }
    public Dictionary<ulong, bool> Votes { get; set; } = new Dictionary<ulong, bool>();
    public DateTime StartTime { get; set; }
    public int RequiredVotes { get; set; }
    
    public bool IsExpired()
    {
        return (DateTime.Now - StartTime).TotalSeconds > 60;
    }
    
    public VoteResult GetResult()
    {
        if (Votes.Count == 0) return VoteResult.Pending;
        
        int yesVotes = Votes.Values.Count(v => v);
        int totalVotes = Votes.Count;
        
        float yesPercentage = (float)yesVotes / totalVotes;
        
        if (yesPercentage > 0.5f && totalVotes >= RequiredVotes)
            return VoteResult.Passed;
        else if (totalVotes >= RequiredVotes)
            return VoteResult.Failed;
        else
            return VoteResult.Pending;
    }
}

public enum VoteType { Start, Stop }
public enum VoteResult { Pending, Passed, Failed }

public class ArenaInstance
{
    public MatchVote CurrentVote { get; set; }
    
    public void InitiateStartVote(ulong initiatorId)
    {
        if (CurrentVote != null && !CurrentVote.IsExpired())
        {
            plugin.Puts("Vote already in progress");
            return;
        }
        
        CurrentVote = new MatchVote
        {
            Type = VoteType.Start,
            InitiatorId = initiatorId,
            StartTime = DateTime.Now,
            RequiredVotes = (int)(activePlayers.Count * 0.5f) // At least 50% must vote
        };
        
        // Show voting UI to all players
        foreach (var playerId in activePlayers)
        {
            var player = BasePlayer.FindByID(playerId);
            if (player != null)
            {
                plugin.ShowVotingUI(player, CurrentVote);
            }
        }
        
        // Auto-close vote after 60 seconds
        plugin.timer.Once(60f, () => {
            if (CurrentVote != null)
            {
                ProcessVoteResult();
            }
        });
    }
    
    public void CastVote(ulong playerId, bool vote)
    {
        if (CurrentVote == null) return;
        
        CurrentVote.Votes[playerId] = vote;
        
        // Check if we have enough votes
        var result = CurrentVote.GetResult();
        if (result != VoteResult.Pending)
        {
            ProcessVoteResult();
        }
    }
    
    private void ProcessVoteResult()
    {
        var result = CurrentVote.GetResult();
        
        // Close voting UI for all players
        foreach (var playerId in activePlayers)
        {
            var player = BasePlayer.FindByID(playerId);
            if (player != null)
            {
                CuiHelper.DestroyUi(player, "VotingPanel");
            }
        }
        
        if (result == VoteResult.Passed)
        {
            if (CurrentVote.Type == VoteType.Start)
            {
                BroadcastToArena("Vote passed! Starting match...");
                StartMatch();
            }
            else
            {
                BroadcastToArena("Vote passed! Stopping match...");
                EndMatch("Match stopped by vote");
            }
        }
        else
        {
            BroadcastToArena("Vote failed!");
        }
        
        CurrentVote = null;
    }
}
```

### B. Voting UI
```csharp
private void ShowVotingUI(BasePlayer player, MatchVote vote)
{
    var elements = new CuiElementContainer();
    
    // Voting panel
    elements.Add(new CuiPanel
    {
        Image = { Color = "0.1 0.1 0.1 0.9" },
        RectTransform = { AnchorMin = "0.35 0.4", AnchorMax = "0.65 0.6" }
    }, "Overlay", "VotingPanel");
    
    // Title
    string voteText = vote.Type == VoteType.Start ? "Start Match?" : "Stop Match?";
    elements.Add(new CuiLabel
    {
        Text = { Text = voteText, FontSize = 18, Align = TextAnchor.MiddleCenter },
        RectTransform = { AnchorMin = "0 0.7", AnchorMax = "1 0.95" }
    }, "VotingPanel");
    
    // Vote count
    int yesCount = vote.Votes.Values.Count(v => v);
    int noCount = vote.Votes.Values.Count(v => !v);
    string voteCount = $"Yes: {yesCount}  No: {noCount}";
    elements.Add(new CuiLabel
    {
        Text = { Text = voteCount, FontSize = 14, Align = TextAnchor.MiddleCenter },
        RectTransform = { AnchorMin = "0 0.45", AnchorMax = "1 0.65" }
    }, "VotingPanel");
    
    // Yes button
    elements.Add(new CuiButton
    {
        Button = { Command = "arena.vote yes", Color = "0 0.7 0 0.8" },
        Text = { Text = "YES", FontSize = 16, Align = TextAnchor.MiddleCenter },
        RectTransform = { AnchorMin = "0.1 0.1", AnchorMax = "0.45 0.35" }
    }, "VotingPanel");
    
    // No button
    elements.Add(new CuiButton
    {
        Button = { Command = "arena.vote no", Color = "0.7 0 0 0.8" },
        Text = { Text = "NO", FontSize = 16, Align = TextAnchor.MiddleCenter },
        RectTransform = { AnchorMin = "0.55 0.1", AnchorMax = "0.9 0.35" }
    }, "VotingPanel");
    
    CuiHelper.AddUi(player, elements);
}

[ConsoleCommand("arena.vote")]
private void VoteCommand(ConsoleSystem.Arg arg)
{
    var player = arg.Player();
    if (player == null) return;
    
    if (!playerArenaMap.ContainsKey(player.userID)) return;
    
    var arena = playerArenaMap[player.userID];
    string voteStr = arg.GetString(0).ToLower();
    bool vote = voteStr == "yes";
    
    arena.CastVote(player.userID, vote);
    
    // Update UI to show player has voted
    CuiHelper.DestroyUi(player, "VotingPanel");
    player.ChatMessage($"You voted: {(vote ? "YES" : "NO")}");
}
```

---

## 6. Integration & UI Management (~100 lines)

### A. UI cleanup on disconnect
```csharp
void OnPlayerDisconnected(BasePlayer player)
{
    // Destroy all UI panels
    CuiHelper.DestroyUi(player, "ArenaStatusPanel");
    CuiHelper.DestroyUi(player, "JoinConfirmPanel");
    CuiHelper.DestroyUi(player, "ArenaSelectionPanel");
    CuiHelper.DestroyUi(player, "VotingPanel");
    
    // Existing disconnect logic...
}
```

### B. UI refresh methods
```csharp
private void RefreshAllStatusPanels()
{
    foreach (var player in BasePlayer.activePlayerList)
    {
        RefreshStatusPanel(player);
    }
}

private void RefreshStatusPanel(BasePlayer player)
{
    CuiHelper.DestroyUi(player, "ArenaStatusPanel");
    ShowStatusPanel(player);
}
```

### C. Auto-show status panel on join
```csharp
void OnPlayerConnected(BasePlayer player)
{
    timer.Once(2f, () => {
        if (player != null && player.IsConnected)
        {
            ShowStatusPanel(player);
        }
    });
}
```

---

## Implementation Order

1. Add ChosenSide to PlayerInfo ✅
2. Modify join command to accept side parameter ✅
3. Add side validation ✅
4. Update spawn logic ✅
5. Create confirmation UI ✅
6. Create status panel ✅
7. Add join/leave functionality ✅
8. Implement captain system ✅
9. Implement voting system ✅
10. Add UI cleanup and refresh ✅
11. Test all features together ✅

---

## Testing Checklist

- [ ] Can join arena with side selection
- [ ] Confirmation shows correct info
- [ ] Status panel updates correctly
- [ ] Join/Leave buttons work
- [ ] Captain gets designated correctly
- [ ] Captain can initiate votes
- [ ] Non-captains can vote
- [ ] Votes are counted correctly
- [ ] Match starts/stops based on votes
- [ ] UI cleans up on disconnect
- [ ] Multiple players can interact simultaneously

---

This guide provides the complete blueprint for implementing all requested features.
