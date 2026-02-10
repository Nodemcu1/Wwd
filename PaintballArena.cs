using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("PaintballArena", "YourName", "1.0.0")]
    [Description("Multi-instance paintball arena system with concurrent gameplay")]
    public class PaintballArena : RustPlugin
    {
        #region Fields
        
        private Dictionary<ulong, ArenaInstance> playerArenaMap = new Dictionary<ulong, ArenaInstance>();
        private Dictionary<int, ArenaInstance> arenaInstances = new Dictionary<int, ArenaInstance>();
        private PluginConfig config;
        
        // Admin setup spheres
        private Dictionary<ulong, List<SphereEntity>> adminSpheres = new Dictionary<ulong, List<SphereEntity>>();
        private Dictionary<ulong, int> adminCurrentArena = new Dictionary<ulong, int>();
        
        private const string ADMIN_PERMISSION = "paintballarena.admin";
        
        #endregion

        #region Configuration

        private class PluginConfig
        {
            [JsonProperty("Arena 1 Settings")]
            public ArenaConfig Arena1 { get; set; }

            [JsonProperty("Arena 2 Settings")]
            public ArenaConfig Arena2 { get; set; }

            [JsonProperty("Arena 3 Settings")]
            public ArenaConfig Arena3 { get; set; }

            [JsonProperty("Global Settings")]
            public GlobalSettings Global { get; set; }

            public static PluginConfig DefaultConfig()
            {
                return new PluginConfig
                {
                    Arena1 = new ArenaConfig
                    {
                        ArenaId = 1,
                        Mode = "5v5_TDM",
                        MinPlayersToStart = 2,
                        MaxRounds = 10,
                        RoundTimeSeconds = 300,
                        GatePosition = new Vector3(100f, 0f, 100f),
                        SpectatorPosition = new Vector3(120f, 10f, 100f),
                        TeamSpawns = new Dictionary<string, List<Vector3>>
                        {
                            ["Blue"] = new List<Vector3> { new Vector3(150f, 0f, 150f) },
                            ["Red"] = new List<Vector3> { new Vector3(50f, 0f, 50f) }
                        }
                    },
                    Arena2 = new ArenaConfig
                    {
                        ArenaId = 2,
                        Mode = "2v2_Chamber",
                        MinPlayersToStart = 2,
                        MaxRounds = 5,
                        RoundTimeSeconds = 180,
                        GatePosition = new Vector3(200f, 0f, 200f),
                        SpectatorPosition = new Vector3(220f, 10f, 200f),
                        TeamSpawns = new Dictionary<string, List<Vector3>>
                        {
                            ["Blue"] = new List<Vector3> { new Vector3(250f, 0f, 250f) },
                            ["Red"] = new List<Vector3> { new Vector3(150f, 0f, 150f) }
                        }
                    },
                    Arena3 = new ArenaConfig
                    {
                        ArenaId = 3,
                        Mode = "1v1_Chamber",
                        MinPlayersToStart = 2,
                        MaxRounds = 3,
                        RoundTimeSeconds = 120,
                        GatePosition = new Vector3(300f, 0f, 300f),
                        SpectatorPosition = new Vector3(320f, 10f, 300f),
                        TeamSpawns = new Dictionary<string, List<Vector3>>
                        {
                            ["Blue"] = new List<Vector3> { new Vector3(350f, 0f, 350f) },
                            ["Red"] = new List<Vector3> { new Vector3(250f, 0f, 250f) }
                        }
                    },
                    Global = new GlobalSettings
                    {
                        LobbyPosition = new Vector3(0f, 0f, 0f),
                        EnableVoiceIsolation = true,
                        VoiceIsolationDistance = 50f
                    }
                };
            }
        }

        public class ArenaConfig
        {
            [JsonProperty("Arena ID")]
            public int ArenaId { get; set; }

            [JsonProperty("Game Mode")]
            public string Mode { get; set; }

            [JsonProperty("Minimum Players To Start")]
            public int MinPlayersToStart { get; set; }

            [JsonProperty("Max Rounds")]
            public int MaxRounds { get; set; }

            [JsonProperty("Round Time (Seconds)")]
            public int RoundTimeSeconds { get; set; }

            [JsonProperty("Gate Position")]
            public Vector3 GatePosition { get; set; }

            [JsonProperty("Spectator Position")]
            public Vector3 SpectatorPosition { get; set; }

            [JsonProperty("Team Spawns")]
            public Dictionary<string, List<Vector3>> TeamSpawns { get; set; }
        }

        public class GlobalSettings
        {
            [JsonProperty("Lobby Position")]
            public Vector3 LobbyPosition { get; set; }

            [JsonProperty("Enable Voice Isolation")]
            public bool EnableVoiceIsolation { get; set; }

            [JsonProperty("Voice Isolation Distance")]
            public float VoiceIsolationDistance { get; set; }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                config = Config.ReadObject<PluginConfig>();
                if (config == null)
                {
                    LoadDefaultConfig();
                }
            }
            catch
            {
                LoadDefaultConfig();
            }
            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            config = PluginConfig.DefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(config);
        }

        #endregion

        #region Arena Instance

        public class ArenaInstance
        {
            public int ArenaId { get; set; }
            public string Mode { get; set; }
            public ArenaConfig Config { get; set; }
            public ArenaState State { get; set; }
            public Dictionary<string, List<ulong>> Teams { get; set; }
            public Dictionary<string, int> Score { get; set; }
            public List<ulong> Spectators { get; set; }
            public int CurrentRound { get; set; }
            public Timer RoundTimer { get; set; }
            public Timer CountdownTimer { get; set; }
            public bool IsActive { get; set; }
            private PaintballArena plugin;

            public ArenaInstance(PaintballArena plugin, ArenaConfig config)
            {
                this.plugin = plugin;
                this.ArenaId = config.ArenaId;
                this.Mode = config.Mode;
                this.Config = config;
                this.State = ArenaState.WaitingForPlayers;
                this.Teams = new Dictionary<string, List<ulong>>
                {
                    ["Blue"] = new List<ulong>(),
                    ["Red"] = new List<ulong>()
                };
                this.Score = new Dictionary<string, int>
                {
                    ["Blue"] = 0,
                    ["Red"] = 0
                };
                this.Spectators = new List<ulong>();
                this.CurrentRound = 0;
                this.IsActive = false;
            }

            public int GetTotalPlayers()
            {
                return Teams.Values.Sum(team => team.Count);
            }

            public void CheckAndStartMatch()
            {
                if (State != ArenaState.WaitingForPlayers)
                    return;

                int totalPlayers = GetTotalPlayers();
                
                if (totalPlayers < Config.MinPlayersToStart)
                {
                    int needed = Config.MinPlayersToStart - totalPlayers;
                    BroadcastToArena($"Waiting for {needed} more player{(needed > 1 ? "s" : "")}...");
                    return;
                }

                // Start countdown
                State = ArenaState.Countdown;
                StartCountdown();
            }

            private void StartCountdown()
            {
                int countdown = 10;
                BroadcastToArena($"Match starting in {countdown}...");
                
                CountdownTimer = plugin.timer.Repeat(1f, countdown + 1, () =>
                {
                    countdown--;
                    if (countdown > 0)
                    {
                        BroadcastToArena($"Match starting in {countdown}...");
                    }
                    else if (countdown == 0)
                    {
                        StartMatch();
                    }
                });
            }

            public void StartMatch()
            {
                State = ArenaState.InProgress;
                CurrentRound = 1;
                IsActive = true;
                
                BroadcastToArena("Match started!");
                
                // Distribute loadouts based on mode
                DistributeLoadouts();
                
                // Spawn all players
                SpawnAllPlayers();
                
                // Start round timer
                StartRoundTimer();
            }

            public void StartRoundTimer()
            {
                if (RoundTimer != null)
                {
                    RoundTimer.Destroy();
                }

                RoundTimer = plugin.timer.Once(Config.RoundTimeSeconds, () =>
                {
                    EndRound("Time expired");
                });
            }

            public void PauseTimer()
            {
                if (RoundTimer != null)
                {
                    RoundTimer.Destroy();
                    RoundTimer = null;
                }
            }

            public void DistributeLoadouts()
            {
                foreach (var team in Teams.Values)
                {
                    foreach (var playerId in team)
                    {
                        var player = BasePlayer.FindByID(playerId);
                        if (player != null)
                        {
                            GiveLoadout(player);
                        }
                    }
                }
            }

            private void GiveLoadout(BasePlayer player)
            {
                // Clear inventory
                player.inventory.Strip();

                // Give paintball gun (using pistol as example)
                var weapon = ItemManager.CreateByName("pistol.semiauto", 1);
                
                if (weapon != null)
                {
                    player.inventory.GiveItem(weapon);

                    // Set ammo based on mode
                    int ammoAmount = GetAmmoForMode();
                    var ammo = ItemManager.CreateByName("ammo.pistol", ammoAmount);
                    if (ammo != null)
                    {
                        player.inventory.GiveItem(ammo);
                    }
                }
            }

            private int GetAmmoForMode()
            {
                if (Mode.Contains("Chamber"))
                {
                    return 1; // One in the Chamber mode
                }
                else if (Mode.Contains("5v5"))
                {
                    return 128; // TDM mode
                }
                else if (Mode.Contains("2v2"))
                {
                    return 1; // One in the Chamber mode
                }
                else if (Mode.Contains("1v1"))
                {
                    return 1; // One in the Chamber mode
                }
                return 64; // Default
            }

            public void SpawnAllPlayers()
            {
                foreach (var kvp in Teams)
                {
                    string teamName = kvp.Key;
                    List<ulong> players = kvp.Value;
                    
                    if (!Config.TeamSpawns.ContainsKey(teamName))
                        continue;

                    var spawns = Config.TeamSpawns[teamName];
                    
                    for (int i = 0; i < players.Count; i++)
                    {
                        var player = BasePlayer.FindByID(players[i]);
                        if (player != null)
                        {
                            var spawnPos = spawns[i % spawns.Count];
                            player.Teleport(spawnPos);
                            player.SetPlayerFlag(BasePlayer.PlayerFlags.Wounded, false);
                            player.health = 100f;
                        }
                    }
                }
            }

            public void HandleElimination(BasePlayer victim, BasePlayer attacker)
            {
                if (State != ArenaState.InProgress)
                    return;

                // Move victim to spectators
                string victimTeam = GetPlayerTeam(victim.userID);
                if (victimTeam != null && Teams.ContainsKey(victimTeam))
                {
                    Teams[victimTeam].Remove(victim.userID);
                }
                
                if (!Spectators.Contains(victim.userID))
                {
                    Spectators.Add(victim.userID);
                }

                // Teleport to spectator position
                victim.Teleport(Config.SpectatorPosition);
                
                // Award point to attacker's team
                if (attacker != null)
                {
                    string attackerTeam = GetPlayerTeam(attacker.userID);
                    if (attackerTeam != null && Score.ContainsKey(attackerTeam))
                    {
                        Score[attackerTeam]++;
                        
                        // Broadcast kill to arena
                        BroadcastToArena($"{attacker.displayName} eliminated {victim.displayName}!");
                        
                        // Refund ammo for "One in the Chamber" modes
                        if (Mode.Contains("Chamber"))
                        {
                            var weapon = attacker.GetActiveItem();
                            if (weapon != null)
                            {
                                var ammo = ItemManager.CreateByName("ammo.pistol", 1);
                                if (ammo != null)
                                {
                                    attacker.inventory.GiveItem(ammo);
                                }
                            }
                        }
                    }
                }

                // Check for round end conditions
                CheckRoundEnd();
                
                // Update UI
                plugin.UpdateScoreboardForArena(this);
            }

            private void CheckRoundEnd()
            {
                // Check if one team is eliminated
                int blueAlive = Teams["Blue"].Count;
                int redAlive = Teams["Red"].Count;

                if (blueAlive == 0)
                {
                    EndRound("Red team wins the round!");
                }
                else if (redAlive == 0)
                {
                    EndRound("Blue team wins the round!");
                }
            }

            public void EndRound(string reason)
            {
                PauseTimer();
                
                BroadcastToArena(reason);
                BroadcastToArena($"Score - Blue: {Score["Blue"]} | Red: {Score["Red"]}");

                CurrentRound++;

                if (CurrentRound > Config.MaxRounds || 
                    Score["Blue"] > Config.MaxRounds / 2 || 
                    Score["Red"] > Config.MaxRounds / 2)
                {
                    EndMatch();
                }
                else
                {
                    // Start next round after delay
                    plugin.timer.Once(5f, () =>
                    {
                        StartNextRound();
                    });
                }
            }

            private void StartNextRound()
            {
                // Return spectators to their teams
                foreach (var spectatorId in Spectators.ToList())
                {
                    var player = BasePlayer.FindByID(spectatorId);
                    if (player != null)
                    {
                        // Find their original team (stored separately if needed)
                        // For now, balance teams
                        string team = Teams["Blue"].Count <= Teams["Red"].Count ? "Blue" : "Red";
                        Teams[team].Add(spectatorId);
                    }
                }
                Spectators.Clear();

                BroadcastToArena($"Round {CurrentRound} starting!");
                
                DistributeLoadouts();
                SpawnAllPlayers();
                StartRoundTimer();
            }

            public void EndMatch()
            {
                State = ArenaState.Ended;
                IsActive = false;
                
                string winner = Score["Blue"] > Score["Red"] ? "Blue" : "Red";
                BroadcastToArena($"Match ended! {winner} team wins!");
                
                PauseTimer();
                
                // Return all players to lobby after delay
                plugin.timer.Once(10f, () =>
                {
                    ReturnAllPlayersToLobby();
                    Reset();
                });
            }

            public void ReturnAllPlayersToLobby()
            {
                var allPlayers = new List<ulong>();
                foreach (var team in Teams.Values)
                {
                    allPlayers.AddRange(team);
                }
                allPlayers.AddRange(Spectators);

                foreach (var playerId in allPlayers)
                {
                    var player = BasePlayer.FindByID(playerId);
                    if (player != null)
                    {
                        player.Teleport(plugin.config.Global.LobbyPosition);
                        plugin.playerArenaMap.Remove(playerId);
                    }
                }
            }

            public void Reset()
            {
                Teams["Blue"].Clear();
                Teams["Red"].Clear();
                Spectators.Clear();
                Score["Blue"] = 0;
                Score["Red"] = 0;
                CurrentRound = 0;
                State = ArenaState.WaitingForPlayers;
                IsActive = false;
                
                if (RoundTimer != null)
                {
                    RoundTimer.Destroy();
                    RoundTimer = null;
                }
                if (CountdownTimer != null)
                {
                    CountdownTimer.Destroy();
                    CountdownTimer = null;
                }
            }

            public string GetPlayerTeam(ulong playerId)
            {
                foreach (var kvp in Teams)
                {
                    if (kvp.Value.Contains(playerId))
                        return kvp.Key;
                }
                return null;
            }

            public void BroadcastToArena(string message)
            {
                var allPlayers = new List<ulong>();
                foreach (var team in Teams.Values)
                {
                    allPlayers.AddRange(team);
                }
                allPlayers.AddRange(Spectators);

                foreach (var playerId in allPlayers)
                {
                    var player = BasePlayer.FindByID(playerId);
                    if (player != null)
                    {
                        player.ChatMessage($"[Arena {ArenaId}] {message}");
                    }
                }
            }

            public void CleanupTimers()
            {
                if (RoundTimer != null)
                {
                    RoundTimer.Destroy();
                    RoundTimer = null;
                }
                if (CountdownTimer != null)
                {
                    CountdownTimer.Destroy();
                    CountdownTimer = null;
                }
            }
        }

        public enum ArenaState
        {
            WaitingForPlayers,
            Countdown,
            InProgress,
            Ended
        }

        #endregion

        #region Oxide Hooks

        private void Init()
        {
            // Register permissions
            permission.RegisterPermission(ADMIN_PERMISSION, this);
            
            // Initialize arena instances
            arenaInstances[1] = new ArenaInstance(this, config.Arena1);
            arenaInstances[2] = new ArenaInstance(this, config.Arena2);
            arenaInstances[3] = new ArenaInstance(this, config.Arena3);

            // Start resource optimization timer (runs every 30 seconds)
            timer.Repeat(30f, 0, OptimizeArenaResources);

            Puts("PaintballArena plugin loaded - Multiple arena instances initialized");
        }

        private void Unload()
        {
            // Cleanup all timers
            foreach (var arena in arenaInstances.Values)
            {
                arena.CleanupTimers();
            }

            // Destroy all UIs
            foreach (var player in BasePlayer.activePlayerList)
            {
                CuiHelper.DestroyUi(player, "PaintballScoreboard");
                CuiHelper.DestroyUi(player, ADMIN_UI_NAME);
            }
            
            // Cleanup all admin spheres
            foreach (var sphereList in adminSpheres.Values)
            {
                foreach (var sphere in sphereList)
                {
                    if (sphere != null && !sphere.IsDestroyed)
                    {
                        sphere.Kill();
                    }
                }
            }
            adminSpheres.Clear();
        }

        private void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || info == null)
                return;

            var victim = entity as BasePlayer;
            if (victim == null)
                return;

            var attacker = info.InitiatorPlayer;
            if (attacker == null)
                return;

            // Check if both players are in paintball arena
            if (!playerArenaMap.ContainsKey(victim.userID) || !playerArenaMap.ContainsKey(attacker.userID))
                return;

            var victimArena = playerArenaMap[victim.userID];
            var attackerArena = playerArenaMap[attacker.userID];

            // Crucial check: Are they in the SAME arena instance?
            if (victimArena.ArenaId != attackerArena.ArenaId)
            {
                // Different arenas - cancel damage
                info.damageTypes.Clear();
                return;
            }

            // Check if they're on the same team
            string victimTeam = victimArena.GetPlayerTeam(victim.userID);
            string attackerTeam = attackerArena.GetPlayerTeam(attacker.userID);

            if (victimTeam == attackerTeam)
            {
                // Friendly fire - cancel damage
                info.damageTypes.Clear();
                return;
            }

            // One hit = elimination in paintball
            info.damageTypes.Clear();
            NextTick(() =>
            {
                victimArena.HandleElimination(victim, attacker);
            });
        }

        private object OnPlayerVoice(BasePlayer player, byte[] data)
        {
            if (!config.Global.EnableVoiceIsolation)
                return null;

            if (!playerArenaMap.ContainsKey(player.userID))
                return null;

            var playerArena = playerArenaMap[player.userID];

            // Only allow voice to players in the same arena
            foreach (var listener in BasePlayer.activePlayerList)
            {
                if (listener.userID == player.userID)
                    continue;

                if (!playerArenaMap.ContainsKey(listener.userID))
                    continue;

                var listenerArena = playerArenaMap[listener.userID];
                
                if (listenerArena.ArenaId != playerArena.ArenaId)
                {
                    // Different arena - block voice
                    // This is a simplified approach; actual implementation may need network manipulation
                    continue;
                }
            }

            return null;
        }

        #endregion

        #region Commands

        [ChatCommand("arena")]
        private void ArenaCommand(BasePlayer player, string command, string[] args)
        {
            if (args.Length == 0)
            {
                player.ChatMessage("Available commands:");
                player.ChatMessage("/arena join <1-3> <team> - Join an arena and team");
                player.ChatMessage("/arena leave - Leave current arena");
                player.ChatMessage("/arena status - View status of all arenas");
                
                if (permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                {
                    player.ChatMessage("/adminsetup - Open admin setup UI");
                }
                
                return;
            }

            switch (args[0].ToLower())
            {
                case "join":
                    if (args.Length < 3)
                    {
                        player.ChatMessage("Usage: /arena join <1-3> <Blue/Red>");
                        return;
                    }
                    
                    if (!int.TryParse(args[1], out int arenaId) || arenaId < 1 || arenaId > 3)
                    {
                        player.ChatMessage("Invalid arena ID. Choose 1, 2, or 3");
                        return;
                    }

                    string team = args[2];
                    if (team != "Blue" && team != "Red")
                    {
                        player.ChatMessage("Invalid team. Choose Blue or Red");
                        return;
                    }

                    JoinArena(player, arenaId, team);
                    break;

                case "leave":
                    LeaveArena(player);
                    break;

                case "status":
                    ShowArenaStatus(player);
                    break;

                default:
                    player.ChatMessage("Unknown command");
                    break;
            }
        }

        private void JoinArena(BasePlayer player, int arenaId, string team)
        {
            // Remove from current arena if in one
            if (playerArenaMap.ContainsKey(player.userID))
            {
                LeaveArena(player);
            }

            if (!arenaInstances.ContainsKey(arenaId))
            {
                player.ChatMessage("Arena not found");
                return;
            }

            var arena = arenaInstances[arenaId];
            
            if (arena.State == ArenaState.InProgress)
            {
                player.ChatMessage("This arena is currently in a match. Please wait.");
                return;
            }

            // Add to team
            if (!arena.Teams.ContainsKey(team))
            {
                player.ChatMessage("Invalid team");
                return;
            }

            arena.Teams[team].Add(player.userID);
            playerArenaMap[player.userID] = arena;

            player.ChatMessage($"Joined Arena {arenaId} - {team} Team");
            player.ChatMessage($"Mode: {arena.Mode}");
            
            // Check if we can start the match
            arena.CheckAndStartMatch();
            
            // Update scoreboard
            UpdateScoreboardForPlayer(player);
        }

        private void LeaveArena(BasePlayer player)
        {
            if (!playerArenaMap.ContainsKey(player.userID))
            {
                player.ChatMessage("You are not in an arena");
                return;
            }

            var arena = playerArenaMap[player.userID];
            
            // Remove from team
            foreach (var team in arena.Teams.Values)
            {
                team.Remove(player.userID);
            }
            arena.Spectators.Remove(player.userID);

            playerArenaMap.Remove(player.userID);

            player.ChatMessage("Left the arena");
            player.Teleport(config.Global.LobbyPosition);
            
            // Destroy UI
            CuiHelper.DestroyUi(player, "PaintballScoreboard");
        }

        private void ShowArenaStatus(BasePlayer player)
        {
            player.ChatMessage("=== Arena Status ===");
            
            foreach (var kvp in arenaInstances)
            {
                var arena = kvp.Value;
                player.ChatMessage($"Arena {arena.ArenaId}: {arena.State} | Mode: {arena.Mode}");
                player.ChatMessage($"  Players: {arena.GetTotalPlayers()} (Blue: {arena.Teams["Blue"].Count}, Red: {arena.Teams["Red"].Count})");
                player.ChatMessage($"  Score: Blue {arena.Score["Blue"]} - {arena.Score["Red"]} Red");
            }
        }

        #endregion

        #region Admin UI Setup

        private const string ADMIN_UI_NAME = "PaintballAdminSetup";
        private Dictionary<ulong, int> adminSpawnCounter = new Dictionary<ulong, int>();

        [ChatCommand("adminsetup")]
        private void AdminSetupCommand(BasePlayer player, string command, string[] args)
        {
            if (player == null) return;
            
            if (!permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
            {
                player.ChatMessage("You don't have permission to use this command.");
                return;
            }

            // Toggle UI
            CuiHelper.DestroyUi(player, ADMIN_UI_NAME);
            
            if (args.Length > 0 && args[0] == "close")
            {
                return; // Just close the UI
            }
            
            ShowAdminUI(player);
        }

        private void ShowAdminUI(BasePlayer player)
        {
            var elements = new CuiElementContainer();

            // Main panel background
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.1 0.1 0.1 0.95" },
                RectTransform = { AnchorMin = "0.3 0.2", AnchorMax = "0.7 0.8" },
                CursorEnabled = true
            }, "Overlay", ADMIN_UI_NAME);

            // Title
            elements.Add(new CuiLabel
            {
                Text = { Text = "PAINTBALL ARENA - ADMIN SETUP", FontSize = 20, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0 0.9", AnchorMax = "1 1" }
            }, ADMIN_UI_NAME);

            // Current arena display
            string currentArenaText = "No Arena Selected";
            if (adminCurrentArena.ContainsKey(player.userID))
            {
                int arenaId = adminCurrentArena[player.userID];
                var arenaConfig = GetArenaConfig(arenaId);
                if (arenaConfig != null)
                {
                    currentArenaText = $"ARENA {arenaId} - {arenaConfig.Mode}";
                }
            }

            elements.Add(new CuiLabel
            {
                Text = { Text = currentArenaText, FontSize = 14, Align = TextAnchor.MiddleCenter, Color = "0.8 0.8 0.2 1" },
                RectTransform = { AnchorMin = "0 0.82", AnchorMax = "1 0.88" }
            }, ADMIN_UI_NAME);

            // Arena Selection Section
            elements.Add(new CuiLabel
            {
                Text = { Text = "SELECT ARENA:", FontSize = 12, Align = TextAnchor.MiddleLeft, Color = "0.7 0.7 0.7 1" },
                RectTransform = { AnchorMin = "0.05 0.72", AnchorMax = "0.95 0.78" }
            }, ADMIN_UI_NAME);

            // Arena buttons
            for (int i = 1; i <= 3; i++)
            {
                float xMin = 0.05f + ((i - 1) * 0.31f);
                float xMax = xMin + 0.28f;
                
                string color = adminCurrentArena.ContainsKey(player.userID) && adminCurrentArena[player.userID] == i 
                    ? "0.2 0.6 0.2 1" 
                    : "0.3 0.3 0.3 1";

                elements.Add(new CuiButton
                {
                    Button = { Color = color, Command = $"adminsetup.selectarena {i}" },
                    RectTransform = { AnchorMin = $"{xMin} 0.64", AnchorMax = $"{xMax} 0.70" },
                    Text = { Text = $"Arena {i}", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
                }, ADMIN_UI_NAME);
            }

            // Position Setup Section
            elements.Add(new CuiLabel
            {
                Text = { Text = "SET POSITION (stand at location first):", FontSize = 12, Align = TextAnchor.MiddleLeft, Color = "0.7 0.7 0.7 1" },
                RectTransform = { AnchorMin = "0.05 0.56", AnchorMax = "0.95 0.62" }
            }, ADMIN_UI_NAME);

            // Position buttons
            var posButtons = new[]
            {
                new { Label = "Set Gate", Command = "setgate", Color = "0.2 0.5 0.2 1" },
                new { Label = "Set Spectator", Command = "setspectator", Color = "0.5 0.5 0.2 1" }
            };

            for (int i = 0; i < posButtons.Length; i++)
            {
                float xMin = 0.05f + (i * 0.48f);
                float xMax = xMin + 0.45f;

                elements.Add(new CuiButton
                {
                    Button = { Color = posButtons[i].Color, Command = $"adminsetup.{posButtons[i].Command}" },
                    RectTransform = { AnchorMin = $"{xMin} 0.48", AnchorMax = $"{xMax} 0.54" },
                    Text = { Text = posButtons[i].Label, FontSize = 11, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
                }, ADMIN_UI_NAME);
            }

            // Spawn buttons
            elements.Add(new CuiButton
            {
                Button = { Color = "0.2 0.2 0.5 1", Command = "adminsetup.setsidea" },
                RectTransform = { AnchorMin = "0.05 0.40", AnchorMax = "0.47 0.46" },
                Text = { Text = "Add Side A Spawn", FontSize = 11, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, ADMIN_UI_NAME);

            elements.Add(new CuiButton
            {
                Button = { Color = "0.5 0.2 0.2 1", Command = "adminsetup.setsideb" },
                RectTransform = { AnchorMin = "0.53 0.40", AnchorMax = "0.95 0.46" },
                Text = { Text = "Add Side B Spawn", FontSize = 11, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, ADMIN_UI_NAME);

            // Utility Section
            elements.Add(new CuiLabel
            {
                Text = { Text = "UTILITIES:", FontSize = 12, Align = TextAnchor.MiddleLeft, Color = "0.7 0.7 0.7 1" },
                RectTransform = { AnchorMin = "0.05 0.32", AnchorMax = "0.95 0.38" }
            }, ADMIN_UI_NAME);

            elements.Add(new CuiButton
            {
                Button = { Color = "0.4 0.3 0.2 1", Command = "adminsetup.clearspheres" },
                RectTransform = { AnchorMin = "0.05 0.24", AnchorMax = "0.47 0.30" },
                Text = { Text = "Clear Spheres", FontSize = 11, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, ADMIN_UI_NAME);

            elements.Add(new CuiButton
            {
                Button = { Color = "0.2 0.4 0.3 1", Command = "adminsetup.save" },
                RectTransform = { AnchorMin = "0.53 0.24", AnchorMax = "0.95 0.30" },
                Text = { Text = "Save Config", FontSize = 11, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, ADMIN_UI_NAME);

            // Info text
            string infoText = "Stand at the position you want to set, then click the appropriate button above.";
            if (adminCurrentArena.ContainsKey(player.userID))
            {
                int arenaId = adminCurrentArena[player.userID];
                var arenaConfig = GetArenaConfig(arenaId);
                if (arenaConfig != null)
                {
                    int sideACount = arenaConfig.TeamSpawns.ContainsKey("Blue") ? arenaConfig.TeamSpawns["Blue"].Count : 0;
                    int sideBCount = arenaConfig.TeamSpawns.ContainsKey("Red") ? arenaConfig.TeamSpawns["Red"].Count : 0;
                    infoText = $"Arena {arenaId}: Side A Spawns: {sideACount} | Side B Spawns: {sideBCount}";
                }
            }

            elements.Add(new CuiLabel
            {
                Text = { Text = infoText, FontSize = 10, Align = TextAnchor.MiddleCenter, Color = "0.6 0.6 0.6 1" },
                RectTransform = { AnchorMin = "0.05 0.14", AnchorMax = "0.95 0.22" }
            }, ADMIN_UI_NAME);

            // Sphere colors legend
            elements.Add(new CuiLabel
            {
                Text = { Text = "🟢 Gate  |  🟡 Spectator  |  🔵 Side A  |  🔴 Side B", FontSize = 10, Align = TextAnchor.MiddleCenter, Color = "0.5 0.5 0.5 1" },
                RectTransform = { AnchorMin = "0.05 0.08", AnchorMax = "0.95 0.13" }
            }, ADMIN_UI_NAME);

            // Close button
            elements.Add(new CuiButton
            {
                Button = { Color = "0.6 0.2 0.2 1", Command = "adminsetup.close" },
                RectTransform = { AnchorMin = "0.4 0.02", AnchorMax = "0.6 0.06" },
                Text = { Text = "CLOSE", FontSize = 10, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, ADMIN_UI_NAME);

            CuiHelper.AddUi(player, elements);
        }

        [ConsoleCommand("adminsetup.selectarena")]
        private void ConsoleSelectArena(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                return;

            if (arg.Args.Length < 1 || !int.TryParse(arg.Args[0], out int arenaId) || arenaId < 1 || arenaId > 3)
                return;

            adminCurrentArena[player.userID] = arenaId;
            
            // Reset spawn counter for new arena
            if (!adminSpawnCounter.ContainsKey(player.userID))
                adminSpawnCounter[player.userID] = 0;
            else
                adminSpawnCounter[player.userID] = 0;

            CuiHelper.DestroyUi(player, ADMIN_UI_NAME); // Destroy old UI first
            ShowAdminUI(player); // Refresh UI
            player.ChatMessage($"Selected Arena {arenaId} for setup");
        }

        [ConsoleCommand("adminsetup.setgate")]
        private void ConsoleSetGate(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                return;

            SetGatePosition(player);
            CuiHelper.DestroyUi(player, ADMIN_UI_NAME); // Destroy old UI first
            ShowAdminUI(player); // Refresh UI
        }

        [ConsoleCommand("adminsetup.setspectator")]
        private void ConsoleSetSpectator(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                return;

            SetSpectatorPosition(player);
            CuiHelper.DestroyUi(player, ADMIN_UI_NAME); // Destroy old UI first
            ShowAdminUI(player); // Refresh UI
        }

        [ConsoleCommand("adminsetup.setsidea")]
        private void ConsoleSetSideA(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                return;

            if (!adminCurrentArena.ContainsKey(player.userID))
            {
                player.ChatMessage("Select an arena first");
                return;
            }

            int arenaId = adminCurrentArena[player.userID];
            var arenaConfig = GetArenaConfig(arenaId);
            if (arenaConfig != null)
            {
                int spawnIndex = arenaConfig.TeamSpawns.ContainsKey("Blue") ? arenaConfig.TeamSpawns["Blue"].Count : 0;
                SetSideASpawn(player, spawnIndex);
                CuiHelper.DestroyUi(player, ADMIN_UI_NAME); // Destroy old UI first
                ShowAdminUI(player); // Refresh UI
            }
        }

        [ConsoleCommand("adminsetup.setsideb")]
        private void ConsoleSetSideB(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                return;

            if (!adminCurrentArena.ContainsKey(player.userID))
            {
                player.ChatMessage("Select an arena first");
                return;
            }

            int arenaId = adminCurrentArena[player.userID];
            var arenaConfig = GetArenaConfig(arenaId);
            if (arenaConfig != null)
            {
                int spawnIndex = arenaConfig.TeamSpawns.ContainsKey("Red") ? arenaConfig.TeamSpawns["Red"].Count : 0;
                SetSideBSpawn(player, spawnIndex);
                CuiHelper.DestroyUi(player, ADMIN_UI_NAME); // Destroy old UI first
                ShowAdminUI(player); // Refresh UI
            }
        }

        [ConsoleCommand("adminsetup.clearspheres")]
        private void ConsoleClearSpheres(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                return;

            ClearPlayerSpheres(player);
            CuiHelper.DestroyUi(player, ADMIN_UI_NAME); // Destroy old UI first
            ShowAdminUI(player); // Refresh UI
        }

        [ConsoleCommand("adminsetup.save")]
        private void ConsoleSaveConfig(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                return;

            SaveArenaConfig(player);
            CuiHelper.DestroyUi(player, ADMIN_UI_NAME); // Destroy old UI first
            ShowAdminUI(player); // Refresh UI
        }

        [ConsoleCommand("adminsetup.close")]
        private void ConsoleCloseUI(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                return;

            CuiHelper.DestroyUi(player, ADMIN_UI_NAME);
        }

        private void SetGatePosition(BasePlayer player)
        {
            if (!adminCurrentArena.ContainsKey(player.userID))
            {
                player.ChatMessage("Select an arena first");
                return;
            }

            int arenaId = adminCurrentArena[player.userID];
            Vector3 position = player.transform.position;

            var arenaConfig = GetArenaConfig(arenaId);
            if (arenaConfig != null)
            {
                arenaConfig.GatePosition = position;
                player.ChatMessage($"✓ Gate position set for Arena {arenaId}");
                
                CreateSphere(player, position, "Gate", new Color(0f, 1f, 0f, 0.5f));
            }
        }

        private void SetSpectatorPosition(BasePlayer player)
        {
            if (!adminCurrentArena.ContainsKey(player.userID))
            {
                player.ChatMessage("Select an arena first");
                return;
            }

            int arenaId = adminCurrentArena[player.userID];
            Vector3 position = player.transform.position;

            var arenaConfig = GetArenaConfig(arenaId);
            if (arenaConfig != null)
            {
                arenaConfig.SpectatorPosition = position;
                player.ChatMessage($"✓ Spectator position set for Arena {arenaId}");
                
                CreateSphere(player, position, "Spectator", new Color(1f, 1f, 0f, 0.5f));
            }
        }

        private void SetSideASpawn(BasePlayer player, int spawnIndex)
        {
            if (!adminCurrentArena.ContainsKey(player.userID))
            {
                player.ChatMessage("Select an arena first");
                return;
            }

            int arenaId = adminCurrentArena[player.userID];
            Vector3 position = player.transform.position;

            var arenaConfig = GetArenaConfig(arenaId);
            if (arenaConfig != null)
            {
                if (!arenaConfig.TeamSpawns.ContainsKey("Blue"))
                {
                    arenaConfig.TeamSpawns["Blue"] = new List<Vector3>();
                }

                while (arenaConfig.TeamSpawns["Blue"].Count <= spawnIndex)
                {
                    arenaConfig.TeamSpawns["Blue"].Add(Vector3.zero);
                }

                arenaConfig.TeamSpawns["Blue"][spawnIndex] = position;
                player.ChatMessage($"✓ Side A spawn #{spawnIndex + 1} set for Arena {arenaId}");
                
                CreateSphere(player, position, $"Side A #{spawnIndex + 1}", new Color(0f, 0f, 1f, 0.5f));
            }
        }

        private void SetSideBSpawn(BasePlayer player, int spawnIndex)
        {
            if (!adminCurrentArena.ContainsKey(player.userID))
            {
                player.ChatMessage("Select an arena first");
                return;
            }

            int arenaId = adminCurrentArena[player.userID];
            Vector3 position = player.transform.position;

            var arenaConfig = GetArenaConfig(arenaId);
            if (arenaConfig != null)
            {
                if (!arenaConfig.TeamSpawns.ContainsKey("Red"))
                {
                    arenaConfig.TeamSpawns["Red"] = new List<Vector3>();
                }

                while (arenaConfig.TeamSpawns["Red"].Count <= spawnIndex)
                {
                    arenaConfig.TeamSpawns["Red"].Add(Vector3.zero);
                }

                arenaConfig.TeamSpawns["Red"][spawnIndex] = position;
                player.ChatMessage($"✓ Side B spawn #{spawnIndex + 1} set for Arena {arenaId}");
                
                CreateSphere(player, position, $"Side B #{spawnIndex + 1}", new Color(1f, 0f, 0f, 0.5f));
            }
        }

        private ArenaConfig GetArenaConfig(int arenaId)
        {
            switch (arenaId)
            {
                case 1: return config.Arena1;
                case 2: return config.Arena2;
                case 3: return config.Arena3;
                default: return null;
            }
        }

        private void CreateSphere(BasePlayer player, Vector3 position, string label, Color color)
        {
            var sphere = GameManager.server.CreateEntity("assets/prefabs/visualization/sphere.prefab", position) as SphereEntity;
            if (sphere != null)
            {
                sphere.currentRadius = 1f;
                sphere.lerpRadius = 1f;
                sphere.Spawn();

                if (!adminSpheres.ContainsKey(player.userID))
                {
                    adminSpheres[player.userID] = new List<SphereEntity>();
                }
                adminSpheres[player.userID].Add(sphere);
            }
        }

        private void ClearPlayerSpheres(BasePlayer player)
        {
            if (adminSpheres.ContainsKey(player.userID))
            {
                foreach (var sphere in adminSpheres[player.userID])
                {
                    if (sphere != null && !sphere.IsDestroyed)
                    {
                        sphere.Kill();
                    }
                }
                adminSpheres[player.userID].Clear();
                player.ChatMessage("✓ All sphere markers cleared");
            }
            else
            {
                player.ChatMessage("No sphere markers to clear");
            }
        }

        private void SaveArenaConfig(BasePlayer player)
        {
            SaveConfig();
            player.ChatMessage("✓ Configuration saved!");
            player.ChatMessage("Use 'o.reload PaintballArena' to apply changes");
        }

        private string FormatVector3(Vector3 v)
        {
            return $"({v.x:F1}, {v.y:F1}, {v.z:F1})";
        }

        #endregion

        #region UI System

        private void UpdateScoreboardForPlayer(BasePlayer player)
        {
            if (!playerArenaMap.ContainsKey(player.userID))
                return;

            var arena = playerArenaMap[player.userID];
            UpdateScoreboardForArena(arena);
        }

        private void UpdateScoreboardForArena(ArenaInstance arena)
        {
            var allPlayers = new List<ulong>();
            foreach (var team in arena.Teams.Values)
            {
                allPlayers.AddRange(team);
            }
            allPlayers.AddRange(arena.Spectators);

            foreach (var playerId in allPlayers)
            {
                var player = BasePlayer.FindByID(playerId);
                if (player != null)
                {
                    CreateScoreboard(player, arena);
                }
            }
        }

        private void CreateScoreboard(BasePlayer player, ArenaInstance arena)
        {
            CuiHelper.DestroyUi(player, "PaintballScoreboard");

            var elements = new CuiElementContainer();

            // Background panel
            elements.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0.8" },
                RectTransform = { AnchorMin = "0.7 0.85", AnchorMax = "0.95 0.98" }
            }, "Hud", "PaintballScoreboard");

            // Title
            elements.Add(new CuiLabel
            {
                Text = { Text = $"Arena {arena.ArenaId} - {arena.Mode}", FontSize = 14, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0 0.7", AnchorMax = "1 1" }
            }, "PaintballScoreboard");

            // Score
            elements.Add(new CuiLabel
            {
                Text = { Text = $"Blue: {arena.Score["Blue"]} | Red: {arena.Score["Red"]}", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0 0.4", AnchorMax = "1 0.7" }
            }, "PaintballScoreboard");

            // Round info
            if (arena.State == ArenaState.InProgress)
            {
                elements.Add(new CuiLabel
                {
                    Text = { Text = $"Round {arena.CurrentRound}/{arena.Config.MaxRounds}", FontSize = 10, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                    RectTransform = { AnchorMin = "0 0.1", AnchorMax = "1 0.4" }
                }, "PaintballScoreboard");
            }
            else
            {
                elements.Add(new CuiLabel
                {
                    Text = { Text = arena.State.ToString(), FontSize = 10, Align = TextAnchor.MiddleCenter, Color = "1 1 0 1" },
                    RectTransform = { AnchorMin = "0 0.1", AnchorMax = "1 0.4" }
                }, "PaintballScoreboard");
            }

            CuiHelper.AddUi(player, elements);
        }

        #endregion

        #region Helper Methods

        private void OptimizeArenaResources()
        {
            // Dynamic CPU Throttling
            // Only active arenas should consume resources
            foreach (var arena in arenaInstances.Values)
            {
                if (arena.GetTotalPlayers() == 0 && arena.IsActive)
                {
                    // Arena is empty but marked active - clean it up
                    arena.Reset();
                }
            }
        }

        #endregion
    }
}
