# Queue System - Player Guide

## How the New System Works

PaintballArena now uses a **queue-based tournament system** where only 2 teams can battle at once per arena. Additional teams wait their turn in a queue.

## Player Flow

### Step 1: Spawn at Central Lobby

When you join the server, you spawn at the **Central Lobby**.

```
[YOU] Spawn → Central Lobby
```

### Step 2: Select Your Team Color

Walk into one of the **5 colored spheres** in the lobby:

- 🟢 **Green Sphere** - Join Green Team
- 🔵 **Blue Sphere** - Join Blue Team
- 🟠 **Orange Sphere** - Join Orange Team
- 🟡 **Yellow Sphere** - Join Yellow Team  
- 🟣 **Purple Sphere** - Join Purple Team

```
[YOU] Walk into Blue Sphere
✓ Team Selected: Blue
Now walk into an Arena Gate to join a match!
```

### Step 3: Choose Your Arena

Walk into one of the **3 Arena Gate spheres**:

- **Arena 1 Gate** - 5v5 Team Deathmatch
- **Arena 2 Gate** - 2v2 One in the Chamber
- **Arena 3 Gate** - 1v1 One in the Chamber

```
[YOU] Walk into Arena 1 Gate
✓ Joined Arena 1 as Blue Team - ACTIVE
```

or

```
✓ Queued for Arena 1 as Blue Team - Position: 2
```

### Step 4: Wait for Your Turn (if queued)

- If **2 teams or less** are in the arena, you're **ACTIVE** and can play immediately
- If **more than 2 teams**, you wait in the **spectator area**
- You'll be notified when it's your turn

```
Waiting in spectator area...
Queue position: 2

[Match ends]

Next match: Blue vs Green!
[Auto-teleported to battle spawn]
```

### Step 5: Battle!

- You're teleported to your team's spawn point
- Match starts automatically
- Play normally with paintball mechanics
- One hit = elimination

### Step 6: After Match

- Match ends when rounds complete
- Your team moves to spectator area
- Next 2 teams from queue enter battle
- You can watch or return to lobby

## Queue Mechanics

### Active Teams (Playing Now)
- **Maximum 2 teams** can battle at once
- These teams are on the battlefield

### Waiting Teams (In Queue)
- Additional teams wait in **spectator area**
- Teams are served in **first-come, first-served** order
- Queue position is shown when you join

### Match Rotation
- When a match ends, active teams move to spectator
- Next 2 teams from queue enter battle
- Automatic teleportation and countdown
- Process repeats

## Example Scenario

**Arena 1 has 5 teams total:**

```
ACTIVE (Playing):
- Green Team (4 players)
- Blue Team (3 players)

QUEUE (Waiting):
- Orange Team (Position 1)
- Yellow Team (Position 2)
- Purple Team (Position 3)
```

**Match ends:**

```
Green & Blue → Move to spectator
Orange & Yellow → Enter battle (auto-teleport)

NEW STATUS:
ACTIVE:
- Orange Team
- Yellow Team

QUEUE:
- Purple Team (Position 1)
- Green Team (Position 2)
- Blue Team (Position 3)
```

## Key Differences from Old System

**Before:**
- Type `/arena join 1 Blue` in chat
- Unlimited teams could play simultaneously
- No queue system

**Now:**
- Walk into colored sphere → Pick team
- Walk into arena gate → Join queue
- Only 2 teams active, others wait
- Physical sphere-based interaction

## Tips

1. **Select team first** - You must pick a team color before entering an arena gate
2. **Be patient** - If queue is long, watch the current match while waiting
3. **Team coordination** - Multiple players can join the same team color
4. **Arena choice** - Check arena modes before selecting your gate
5. **Return to lobby** - If you want to switch arenas/teams, return to central lobby

## Arena Modes

- **Arena 1**: 5v5 Team Deathmatch (10 rounds, 300 seconds per round)
- **Arena 2**: 2v2 One in the Chamber (5 rounds, 180 seconds per round)
- **Arena 3**: 1v1 One in the Chamber (3 rounds, 120 seconds per round)

## Benefits of Queue System

✅ **Fair Play** - Everyone gets a turn
✅ **Organized Matches** - Clear teams, no confusion
✅ **Spectator Mode** - Watch while waiting
✅ **Tournament Feel** - Structured competition
✅ **No Overcrowding** - Max 2 teams prevents chaos

## Questions?

**Q: Can I change teams?**
A: Yes, return to central lobby and walk into a different colored sphere

**Q: How long do I wait in queue?**
A: Depends on match length and queue size. Matches typically last 5-15 minutes

**Q: Can I leave the queue?**
A: Yes, return to central lobby

**Q: What if my team loses?**
A: You move to spectator and can re-queue by walking into the arena gate again

**Q: Can teammates join me?**
A: Yes! Multiple players can select the same team color and enter the same arena

---

**Enjoy the new tournament-style paintball experience!** 🎯🎨
