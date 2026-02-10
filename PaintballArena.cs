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
        
        // Queue system - NEW
        private Dictionary<ulong, PlayerInfo> playerInfo = new Dictionary<ulong, PlayerInfo>();
        private Dictionary<int, ArenaQueue> arenaQueues = new Dictionary<int, ArenaQueue>();
        
        // Admin setup spheres
        private Dictionary<ulong, List<SphereEntity>> adminSpheres = new Dictionary<ulong, List<SphereEntity>>();
        private Dictionary<ulong, int> adminCurrentArena = new Dictionary<ulong, int>();
        
        private const string ADMIN_PERMISSION = "paintballarena.admin";
        private const float SPHERE_DETECTION_DISTANCE = 2f; // Distance to detect sphere collision
        
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
                            ["SideA"] = new List<Vector3> { new Vector3(150f, 0f, 150f), new Vector3(140f, 0f, 150f) },
                            ["SideB"] = new List<Vector3> { new Vector3(60f, 0f, 50f), new Vector3(50f, 0f, 50f) }
                        },
                        TeamSelectionSpheres = new Dictionary<string, Vector3>
                        {
                            ["Green"] = new Vector3(5f, 0f, 15f),
                            ["Blue"] = new Vector3(10f, 0f, 10f),
                            ["Orange"] = new Vector3(15f, 0f, 0f),
                            ["Yellow"] = new Vector3(10f, 0f, -10f),
                            ["Purple"] = new Vector3(5f, 0f, -15f)
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
                            ["SideA"] = new List<Vector3> { new Vector3(250f, 0f, 250f), new Vector3(240f, 0f, 250f) },
                            ["SideB"] = new List<Vector3> { new Vector3(160f, 0f, 150f), new Vector3(150f, 0f, 150f) }
                        },
                        TeamSelectionSpheres = new Dictionary<string, Vector3>
                        {
                            ["Green"] = new Vector3(15f, 0f, 15f),
                            ["Blue"] = new Vector3(20f, 0f, 10f),
                            ["Orange"] = new Vector3(25f, 0f, 0f),
                            ["Yellow"] = new Vector3(20f, 0f, -10f),
                            ["Purple"] = new Vector3(15f, 0f, -15f)
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
                            ["SideA"] = new List<Vector3> { new Vector3(350f, 0f, 350f), new Vector3(340f, 0f, 350f) },
                            ["SideB"] = new List<Vector3> { new Vector3(260f, 0f, 250f), new Vector3(250f, 0f, 250f) }
                        },
                        TeamSelectionSpheres = new Dictionary<string, Vector3>
                        {
                            ["Green"] = new Vector3(25f, 0f, 15f),
                            ["Blue"] = new Vector3(30f, 0f, 10f),
                            ["Orange"] = new Vector3(35f, 0f, 0f),
                            ["Yellow"] = new Vector3(30f, 0f, -10f),
                            ["Purple"] = new Vector3(25f, 0f, -15f)
                        }
                    },
                    Global = new GlobalSettings
                    {
                        LobbyCentral = new Vector3(0f, 0f, 0f),
                        TeamColorSpheres = new Dictionary<string, Vector3>
                        {
                            ["Green"] = new Vector3(-10f, 0f, 15f),
                            ["Blue"] = new Vector3(-10f, 0f, 5f),
                            ["Orange"] = new Vector3(-10f, 0f, -5f),
                            ["Yellow"] = new Vector3(-10f, 0f, -15f),
                            ["Purple"] = new Vector3(-10f, 0f, -25f)
                        },
                        ArenaGateSpheres = new List<Vector3>
                        {
                            new Vector3(10f, 0f, 10f),   // Arena 1 gate
                            new Vector3(10f, 0f, 0f),    // Arena 2 gate
                            new Vector3(10f, 0f, -10f)   // Arena 3 gate
                        },
                        ReturnToLobbySphere = new Vector3(20f, 0f, 0f),  // Sphere to return to lobby from arena
                        LeaveLobby = new Vector3(-20f, 0f, 0f),          // Position to exit the system
                        EnableVoiceIsolation = true,
                        VoiceIsolationDistance = 50f,
                        MaxActiveTeamsPerArena = 2  // Only 2 teams can battle at once
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

            [JsonProperty("Team Selection Spheres")]
            public Dictionary<string, Vector3> TeamSelectionSpheres { get; set; }
        }

        public class GlobalSettings
        {
            [JsonProperty("Central Lobby Position")]
            public Vector3 LobbyCentral { get; set; }
            
            [JsonProperty("Team Color Spheres")]
            public Dictionary<string, Vector3> TeamColorSpheres { get; set; }
            
            [JsonProperty("Arena Gate Spheres")]
            public List<Vector3> ArenaGateSpheres { get; set; }
            
            [JsonProperty("Return To Lobby Sphere")]
            public Vector3 ReturnToLobbySphere { get; set; }
            
            [JsonProperty("Leave Lobby Position")]
            public Vector3 LeaveLobby { get; set; }

            [JsonProperty("Enable Voice Isolation")]
            public bool EnableVoiceIsolation { get; set; }

            [JsonProperty("Voice Isolation Distance")]
            public float VoiceIsolationDistance { get; set; }
            
            [JsonProperty("Max Active Teams Per Arena")]
            public int MaxActiveTeamsPerArena { get; set; }
        }

        // Player state tracking for queue system
        public enum PlayerState
        {
            None,
            InLobby,
            TeamSelected,
            InQueue,
            InBattle,
            Spectating
        }

        public class PlayerInfo
        {
            public ulong PlayerId { get; set; }
            public PlayerState State { get; set; }
            public string SelectedTeam { get; set; }
            public int QueuedArena { get; set; }
            
            public PlayerInfo(ulong playerId)
            {
                PlayerId = playerId;
                State = PlayerState.None;
                SelectedTeam = null;
                QueuedArena = -1;
            }
        }

        public class ArenaQueue
        {
            public int ArenaId { get; set; }
            public List<string> WaitingTeams { get; set; }
            public List<string> ActiveTeams { get; set; }
            public int MaxActiveTeams { get; set; }
            
            public ArenaQueue(int arenaId, int maxActiveTeams = 2)
            {
                ArenaId = arenaId;
                WaitingTeams = new List<string>();
                ActiveTeams = new List<string>();
                MaxActiveTeams = maxActiveTeams;
            }
            
            public bool CanAddTeam(string team)
            {
                return !ActiveTeams.Contains(team) && !WaitingTeams.Contains(team);
            }
            
            public void AddTeam(string team)
            {
                if (ActiveTeams.Count < MaxActiveTeams)
                {
                    ActiveTeams.Add(team);
                }
                else
                {
                    if (!WaitingTeams.Contains(team))
                    {
                        WaitingTeams.Add(team);
                    }
                }
            }
            
            public void RemoveTeam(string team)
            {
                ActiveTeams.Remove(team);
                WaitingTeams.Remove(team);
            }
            
            public List<string> PullNextTeams()
            {
                var teamsToActivate = new List<string>();
                
                while (ActiveTeams.Count < MaxActiveTeams && WaitingTeams.Count > 0)
                {
                    var team = WaitingTeams[0];
                    WaitingTeams.RemoveAt(0);
                    ActiveTeams.Add(team);
                    teamsToActivate.Add(team);
                }
                
                return teamsToActivate;
            }
            
            public int GetQueuePosition(string team)
            {
                if (ActiveTeams.Contains(team))
                    return 0; // Currently playing
                    
                int index = WaitingTeams.IndexOf(team);
                return index >= 0 ? index + 1 : -1;
            }
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
                    ["Green"] = new List<ulong>(),
                    ["Blue"] = new List<ulong>(),
                    ["Orange"] = new List<ulong>(),
                    ["Yellow"] = new List<ulong>(),
                    ["Purple"] = new List<ulong>()
                };
                this.Score = new Dictionary<string, int>
                {
                    ["Green"] = 0,
                    ["Blue"] = 0,
                    ["Orange"] = 0,
                    ["Yellow"] = 0,
                    ["Purple"] = 0
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
                
                // Find winner (team with highest score)
                string winner = Score.OrderByDescending(x => x.Value).First().Key;
                BroadcastToArena($"Match ended! {winner} team wins!");
                
                PauseTimer();
                
                // Trigger queue rotation after delay
                plugin.timer.Once(5f, () =>
                {
                    plugin.OnArenaMatchEnd(ArenaId);
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
                        player.Teleport(plugin.config.Global.LobbyCentral);
                        plugin.playerArenaMap.Remove(playerId);
                        
                        // Reset player info
                        if (plugin.playerInfo.ContainsKey(playerId))
                        {
                            plugin.playerInfo[playerId].State = PlayerState.InLobby;
                            plugin.playerInfo[playerId].SelectedTeam = null;
                            plugin.playerInfo[playerId].QueuedArena = -1;
                        }
                    }
                }
            }

            public void Reset()
            {
                foreach (var team in Teams.Keys.ToList())
                {
                    Teams[team].Clear();
                    Score[team] = 0;
                }
                Spectators.Clear();
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
            
            // Initialize queue system
            arenaQueues[1] = new ArenaQueue(1, config.Global.MaxActiveTeamsPerArena);
            arenaQueues[2] = new ArenaQueue(2, config.Global.MaxActiveTeamsPerArena);
            arenaQueues[3] = new ArenaQueue(3, config.Global.MaxActiveTeamsPerArena);

            // Start resource optimization timer (runs every 30 seconds)
            timer.Repeat(30f, 0, OptimizeArenaResources);
            
            // Start sphere detection timer (runs every 0.5 seconds)
            timer.Repeat(0.5f, 0, CheckPlayerSphereProximity);

            Puts("PaintballArena plugin loaded - Queue-based system initialized");
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

        #region Sphere Detection & Queue System

        private void CheckPlayerSphereProximity()
        {
            // Safety checks - ensure config is loaded
            if (config == null || config.Global == null)
                return;
            
            if (config.Global.TeamColorSpheres == null || config.Global.ArenaGateSpheres == null)
                return;
            
            foreach (var player in BasePlayer.activePlayerList)
            {
                if (player == null || !player.IsConnected)
                    continue;

                var playerId = player.userID;
                
                // Skip if player is already in battle
                if (playerInfo.ContainsKey(playerId) && playerInfo[playerId].State == PlayerState.InBattle)
                    continue;

                // Check team color sphere proximity
                foreach (var teamColor in config.Global.TeamColorSpheres)
                {
                    if (Vector3.Distance(player.transform.position, teamColor.Value) < SPHERE_DETECTION_DISTANCE)
                    {
                        OnPlayerEnterTeamSphere(player, teamColor.Key);
                        return; // Only one action per tick
                    }
                }

                // Check arena gate sphere proximity
                for (int i = 0; i < config.Global.ArenaGateSpheres.Count; i++)
                {
                    if (Vector3.Distance(player.transform.position, config.Global.ArenaGateSpheres[i]) < SPHERE_DETECTION_DISTANCE)
                    {
                        OnPlayerEnterArenaGate(player, i + 1); // Arena ID is 1-based
                        return; // Only one action per tick
                    }
                }

                // Check return to lobby sphere proximity
                if (config.Global.ReturnToLobbySphere != Vector3.zero)
                {
                    if (Vector3.Distance(player.transform.position, config.Global.ReturnToLobbySphere) < SPHERE_DETECTION_DISTANCE)
                    {
                        OnPlayerReturnToLobby(player);
                        return; // Only one action per tick
                    }
                }

                // Check leave lobby sphere proximity
                if (config.Global.LeaveLobby != Vector3.zero)
                {
                    if (Vector3.Distance(player.transform.position, config.Global.LeaveLobby) < SPHERE_DETECTION_DISTANCE)
                    {
                        OnPlayerLeaveLobby(player);
                        return; // Only one action per tick
                    }
                }
            }
        }

        private void OnPlayerEnterTeamSphere(BasePlayer player, string team)
        {
            var playerId = player.userID;
            
            // Initialize player info if needed
            if (!playerInfo.ContainsKey(playerId))
            {
                playerInfo[playerId] = new PlayerInfo(playerId);
            }
            
            var info = playerInfo[playerId];
            
            // Skip if already selected this team
            if (info.SelectedTeam == team)
                return;
            
            // Assign team
            info.SelectedTeam = team;
            info.State = PlayerState.TeamSelected;
            
            SendReply(player, $"✓ Team Selected: <color={GetTeamColor(team)}>{team}</color>");
            SendReply(player, "Now walk into an Arena Gate to join a match!");
        }

        private void OnPlayerEnterArenaGate(BasePlayer player, int arenaId)
        {
            var playerId = player.userID;
            
            // Check if player has selected a team
            if (!playerInfo.ContainsKey(playerId) || string.IsNullOrEmpty(playerInfo[playerId].SelectedTeam))
            {
                SendReply(player, "❌ You must select a team color first!");
                return;
            }
            
            var info = playerInfo[playerId];
            var team = info.SelectedTeam;
            
            // Check if already in this queue
            if (info.QueuedArena == arenaId && info.State == PlayerState.InQueue)
            {
                SendReply(player, $"You're already queued for Arena {arenaId}");
                return;
            }
            
            // Get arena queue
            if (!arenaQueues.ContainsKey(arenaId))
            {
                SendReply(player, $"❌ Arena {arenaId} not found!");
                return;
            }
            
            var queue = arenaQueues[arenaId];
            var arena = arenaInstances[arenaId];
            
            // Check if team can be added
            if (!queue.CanAddTeam(team))
            {
                SendReply(player, $"❌ Team {team} is already in Arena {arenaId}");
                return;
            }
            
            // Add team to queue
            queue.AddTeam(team);
            info.QueuedArena = arenaId;
            info.State = PlayerState.InQueue;
            
            // Add player to arena's team
            if (!arena.Teams.ContainsKey(team))
            {
                arena.Teams[team] = new List<ulong>();
            }
            if (!arena.Teams[team].Contains(playerId))
            {
                arena.Teams[team].Add(playerId);
            }
            
            // Map player to arena
            playerArenaMap[playerId] = arena;
            
            int queuePos = queue.GetQueuePosition(team);
            if (queuePos == 0)
            {
                SendReply(player, $"✓ Joined Arena {arenaId} as <color={GetTeamColor(team)}>{team}</color> Team - ACTIVE");
                
                // Teleport to spectator to wait for match start
                player.Teleport(arena.Config.SpectatorPosition);
                
                // Check if we can start a match
                CheckAndStartArenaMatch(arenaId);
            }
            else
            {
                SendReply(player, $"✓ Queued for Arena {arenaId} as <color={GetTeamColor(team)}>{team}</color> Team - Position: {queuePos}");
                
                // Teleport to spectator area to wait
                player.Teleport(arena.Config.SpectatorPosition);
            }
        }

        private void OnPlayerReturnToLobby(BasePlayer player)
        {
            var playerId = player.userID;
            
            // Skip if player is already in lobby
            if (playerInfo.ContainsKey(playerId) && playerInfo[playerId].State == PlayerState.InLobby)
                return;
            
            // Teleport to lobby
            player.Teleport(config.Global.LobbyCentral);
            
            // Update player state
            if (!playerInfo.ContainsKey(playerId))
            {
                playerInfo[playerId] = new PlayerInfo(playerId);
            }
            
            var info = playerInfo[playerId];
            info.State = PlayerState.InLobby;
            info.SelectedTeam = null;
            info.QueuedArena = 0;
            
            // Remove from arena if applicable
            if (playerArenaMap.ContainsKey(playerId))
            {
                var arena = playerArenaMap[playerId];
                foreach (var team in arena.Teams)
                {
                    team.Value.Remove(playerId);
                }
                playerArenaMap.Remove(playerId);
            }
            
            SendReply(player, "✓ Returned to lobby");
        }

        private void OnPlayerLeaveLobby(BasePlayer player)
        {
            var playerId = player.userID;
            
            // Skip if already outside lobby system
            if (!playerInfo.ContainsKey(playerId) || playerInfo[playerId].State == PlayerState.None)
                return;
            
            // Teleport to leave position
            player.Teleport(config.Global.LeaveLobby);
            
            // Clear player state
            if (playerInfo.ContainsKey(playerId))
            {
                playerInfo.Remove(playerId);
            }
            
            // Remove from arena if applicable
            if (playerArenaMap.ContainsKey(playerId))
            {
                var arena = playerArenaMap[playerId];
                foreach (var team in arena.Teams)
                {
                    team.Value.Remove(playerId);
                }
                playerArenaMap.Remove(playerId);
            }
            
            SendReply(player, "✓ Left the Paintball Arena system");
        }

        private void CheckAndStartArenaMatch(int arenaId)
        {
            var queue = arenaQueues[arenaId];
            var arena = arenaInstances[arenaId];
            
            // Need exactly 2 teams active to start
            if (queue.ActiveTeams.Count != 2)
                return;
            
            // Check if arena is already in progress
            if (arena.State != ArenaState.WaitingForPlayers)
                return;
            
            arena.CheckAndStartMatch();
        }

        private void OnArenaMatchEnd(int arenaId)
        {
            var queue = arenaQueues[arenaId];
            var arena = arenaInstances[arenaId];
            
            // Move current active teams to spectator
            foreach (var team in queue.ActiveTeams.ToList())
            {
                var teamPlayers = arena.Teams.ContainsKey(team) ? arena.Teams[team] : new List<ulong>();
                foreach (var playerId in teamPlayers.ToList())
                {
                    var player = BasePlayer.FindByID(playerId);
                    if (player != null)
                    {
                        player.Teleport(arena.Config.SpectatorPosition);
                        
                        if (playerInfo.ContainsKey(playerId))
                        {
                            playerInfo[playerId].State = PlayerState.Spectating;
                        }
                    }
                }
            }
            
            // Clear active teams
            queue.ActiveTeams.Clear();
            
            // Pull next teams from queue
            var nextTeams = queue.PullNextTeams();
            
            if (nextTeams.Count >= 2)
            {
                arena.BroadcastToArena($"Next match: {nextTeams[0]} vs {nextTeams[1]}!");
                arena.State = ArenaState.WaitingForPlayers;
                
                // Update player states
                foreach (var team in nextTeams)
                {
                    var teamPlayers = arena.Teams.ContainsKey(team) ? arena.Teams[team] : new List<ulong>();
                    foreach (var playerId in teamPlayers)
                    {
                        if (playerInfo.ContainsKey(playerId))
                        {
                            playerInfo[playerId].State = PlayerState.InQueue;
                        }
                    }
                }
                
                CheckAndStartArenaMatch(arenaId);
            }
            else
            {
                arena.BroadcastToArena("Waiting for more teams to join...");
                arena.State = ArenaState.WaitingForPlayers;
            }
        }

        #endregion

        #region Commands

        [ChatCommand("arena")]
        private void ArenaCommand(BasePlayer player, string command, string[] args)
        {
            if (args.Length == 0)
            {
                player.ChatMessage("Available commands:");
                player.ChatMessage("/arena join <1-3> <Green/Blue/Orange/Yellow/Purple> - Join an arena and team");
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
                        player.ChatMessage("Usage: /arena join <1-3> <Green/Blue/Orange/Yellow/Purple>");
                        return;
                    }
                    
                    if (!int.TryParse(args[1], out int arenaId) || arenaId < 1 || arenaId > 3)
                    {
                        player.ChatMessage("Invalid arena ID. Choose 1, 2, or 3");
                        return;
                    }

                    string team = args[2];
                    string[] validTeams = { "Green", "Blue", "Orange", "Yellow", "Purple" };
                    if (!validTeams.Contains(team))
                    {
                        player.ChatMessage("Invalid team. Choose: Green, Blue, Orange, Yellow, or Purple");
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
            player.Teleport(config.Global.LobbyCentral);
            
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

            // Main panel background - MADE TALLER
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.1 0.1 0.1 0.95" },
                RectTransform = { AnchorMin = "0.3 0.1", AnchorMax = "0.7 0.9" },
                CursorEnabled = true
            }, "Overlay", ADMIN_UI_NAME);

            // Title
            elements.Add(new CuiLabel
            {
                Text = { Text = "PAINTBALL ARENA - ADMIN SETUP", FontSize = 20, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0 0.94", AnchorMax = "1 1" }
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
                RectTransform = { AnchorMin = "0 0.88", AnchorMax = "1 0.93" }
            }, ADMIN_UI_NAME);

            // Arena Selection Section
            elements.Add(new CuiLabel
            {
                Text = { Text = "SELECT ARENA:", FontSize = 12, Align = TextAnchor.MiddleLeft, Color = "0.7 0.7 0.7 1" },
                RectTransform = { AnchorMin = "0.05 0.82", AnchorMax = "0.95 0.86" }
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
                    RectTransform = { AnchorMin = $"{xMin} 0.76", AnchorMax = $"{xMax} 0.81" },
                    Text = { Text = $"Arena {i}", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
                }, ADMIN_UI_NAME);
            }

            // Position Setup Section
            elements.Add(new CuiLabel
            {
                Text = { Text = "GLOBAL POSITIONS:", FontSize = 12, Align = TextAnchor.MiddleLeft, Color = "0.7 0.7 0.7 1" },
                RectTransform = { AnchorMin = "0.05 0.70", AnchorMax = "0.95 0.74" }
            }, ADMIN_UI_NAME);

            // Global position buttons (lobby, return, leave)
            var globalButtons = new[]
            {
                new { Label = "Set Lobby", Command = "setlobby", Color = "0.3 0.3 0.5 1" },
                new { Label = "Return Sphere", Command = "setreturn", Color = "0.3 0.5 0.3 1" },
                new { Label = "Leave Lobby", Command = "setleave", Color = "0.5 0.3 0.3 1" }
            };

            for (int i = 0; i < globalButtons.Length; i++)
            {
                float xMin = 0.05f + (i * 0.305f);
                float xMax = xMin + 0.285f;

                elements.Add(new CuiButton
                {
                    Button = { Color = globalButtons[i].Color, Command = $"adminsetup.{globalButtons[i].Command}" },
                    RectTransform = { AnchorMin = $"{xMin} 0.64", AnchorMax = $"{xMax} 0.69" },
                    Text = { Text = globalButtons[i].Label, FontSize = 10, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
                }, ADMIN_UI_NAME);
            }

            // Per-Arena Positions
            elements.Add(new CuiLabel
            {
                Text = { Text = "PER-ARENA POSITIONS:", FontSize = 12, Align = TextAnchor.MiddleLeft, Color = "0.7 0.7 0.7 1" },
                RectTransform = { AnchorMin = "0.05 0.58", AnchorMax = "0.95 0.62" }
            }, ADMIN_UI_NAME);

            // Per-arena position buttons
            var perArenaButtons = new[]
            {
                new { Label = "Set Gate", Command = "setgate", Color = "0.2 0.5 0.2 1" },
                new { Label = "Set Spectator", Command = "setspectator", Color = "0.5 0.5 0.2 1" }
            };

            for (int i = 0; i < perArenaButtons.Length; i++)
            {
                float xMin = 0.05f + (i * 0.455f);
                float xMax = xMin + 0.435f;

                elements.Add(new CuiButton
                {
                    Button = { Color = perArenaButtons[i].Color, Command = $"adminsetup.{perArenaButtons[i].Command}" },
                    RectTransform = { AnchorMin = $"{xMin} 0.52", AnchorMax = $"{xMax} 0.57" },
                    Text = { Text = perArenaButtons[i].Label, FontSize = 10, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
                }, ADMIN_UI_NAME);
            }

            // Spawn buttons section
            elements.Add(new CuiLabel
            {
                Text = { Text = "BATTLE SPAWNS:", FontSize = 12, Align = TextAnchor.MiddleLeft, Color = "0.7 0.7 0.7 1" },
                RectTransform = { AnchorMin = "0.05 0.46", AnchorMax = "0.95 0.50" }
            }, ADMIN_UI_NAME);

            // Battle spawn buttons (requires arena selection) - SIDE A vs SIDE B
            elements.Add(new CuiButton
            {
                Button = { Color = "0.2 0.4 0.6 1", Command = "adminsetup.setsidea" },
                RectTransform = { AnchorMin = "0.05 0.40", AnchorMax = "0.48 0.45" },
                Text = { Text = "Add Side A Spawn", FontSize = 11, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, ADMIN_UI_NAME);

            elements.Add(new CuiButton
            {
                Button = { Color = "0.6 0.2 0.2 1", Command = "adminsetup.setsideb" },
                RectTransform = { AnchorMin = "0.52 0.40", AnchorMax = "0.95 0.45" },
                Text = { Text = "Add Side B Spawn", FontSize = 11, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, ADMIN_UI_NAME);

            // Team selection sphere buttons (lobby) - for selecting which team to join
            elements.Add(new CuiLabel
            {
                Text = { Text = "TEAM LOBBY SPHERES:", FontSize = 10, Align = TextAnchor.MiddleLeft, Color = "0.7 0.7 0.7 1" },
                RectTransform = { AnchorMin = "0.05 0.34", AnchorMax = "0.95 0.38" }
            }, ADMIN_UI_NAME);

            elements.Add(new CuiButton
            {
                Button = { Color = "0.1 0.3 0.1 1", Command = "adminsetup.setlobbygreen" },
                RectTransform = { AnchorMin = "0.05 0.29", AnchorMax = "0.23 0.33" },
                Text = { Text = "Green Sphere", FontSize = 9, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, ADMIN_UI_NAME);

            elements.Add(new CuiButton
            {
                Button = { Color = "0.1 0.1 0.4 1", Command = "adminsetup.setlobbyblue" },
                RectTransform = { AnchorMin = "0.25 0.29", AnchorMax = "0.43 0.33" },
                Text = { Text = "Blue Sphere", FontSize = 9, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, ADMIN_UI_NAME);

            elements.Add(new CuiButton
            {
                Button = { Color = "0.4 0.2 0.05 1", Command = "adminsetup.setlobbyorange" },
                RectTransform = { AnchorMin = "0.45 0.29", AnchorMax = "0.63 0.33" },
                Text = { Text = "Orange Sphere", FontSize = 9, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, ADMIN_UI_NAME);

            elements.Add(new CuiButton
            {
                Button = { Color = "0.4 0.4 0.05 1", Command = "adminsetup.setlobbyyellow" },
                RectTransform = { AnchorMin = "0.65 0.29", AnchorMax = "0.83 0.33" },
                Text = { Text = "Yellow Sphere", FontSize = 9, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, ADMIN_UI_NAME);

            elements.Add(new CuiButton
            {
                Button = { Color = "0.2 0.05 0.3 1", Command = "adminsetup.setlobbypurple" },
                RectTransform = { AnchorMin = "0.85 0.29", AnchorMax = "0.95 0.33" },
                Text = { Text = "Purple", FontSize = 8, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, ADMIN_UI_NAME);

            // Arena Gate Sphere Buttons
            elements.Add(new CuiLabel
            {
                Text = { Text = "ARENA GATE SPHERES (Lobby):", FontSize = 10, Align = TextAnchor.MiddleLeft, Color = "0.7 0.7 0.7 1" },
                RectTransform = { AnchorMin = "0.05 0.23", AnchorMax = "0.95 0.27" }
            }, ADMIN_UI_NAME);

            elements.Add(new CuiButton
            {
                Button = { Color = "0.3 0.3 0.3 1", Command = "adminsetup.setarenagate 1" },
                RectTransform = { AnchorMin = "0.05 0.18", AnchorMax = "0.35 0.22" },
                Text = { Text = "Arena 1 Gate", FontSize = 9, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, ADMIN_UI_NAME);

            elements.Add(new CuiButton
            {
                Button = { Color = "0.3 0.3 0.3 1", Command = "adminsetup.setarenagate 2" },
                RectTransform = { AnchorMin = "0.37 0.18", AnchorMax = "0.63 0.22" },
                Text = { Text = "Arena 2 Gate", FontSize = 9, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, ADMIN_UI_NAME);

            elements.Add(new CuiButton
            {
                Button = { Color = "0.3 0.3 0.3 1", Command = "adminsetup.setarenagate 3" },
                RectTransform = { AnchorMin = "0.65 0.18", AnchorMax = "0.95 0.22" },
                Text = { Text = "Arena 3 Gate", FontSize = 9, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, ADMIN_UI_NAME);

            // Utility Section
            elements.Add(new CuiLabel
            {
                Text = { Text = "UTILITIES:", FontSize = 12, Align = TextAnchor.MiddleLeft, Color = "0.7 0.7 0.7 1" },
                RectTransform = { AnchorMin = "0.05 0.11", AnchorMax = "0.95 0.15" }
            }, ADMIN_UI_NAME);

            elements.Add(new CuiButton
            {
                Button = { Color = "0.4 0.3 0.2 1", Command = "adminsetup.clearspheres" },
                RectTransform = { AnchorMin = "0.05 0.06", AnchorMax = "0.47 0.10" },
                Text = { Text = "Clear Spheres", FontSize = 11, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, ADMIN_UI_NAME);

            elements.Add(new CuiButton
            {
                Button = { Color = "0.2 0.4 0.3 1", Command = "adminsetup.save" },
                RectTransform = { AnchorMin = "0.53 0.06", AnchorMax = "0.95 0.10" },
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
                    int sideACount = arenaConfig.TeamSpawns.ContainsKey("SideA") ? arenaConfig.TeamSpawns["SideA"].Count : 0;
                    int sideBCount = arenaConfig.TeamSpawns.ContainsKey("SideB") ? arenaConfig.TeamSpawns["SideB"].Count : 0;
                    infoText = $"Arena {arenaId} | Side A: {sideACount} spawns | Side B: {sideBCount} spawns";
                }
            }

            elements.Add(new CuiLabel
            {
                Text = { Text = infoText, FontSize = 9, Align = TextAnchor.MiddleCenter, Color = "0.6 0.6 0.6 1" },
                RectTransform = { AnchorMin = "0.05 0.11", AnchorMax = "0.95 0.16" }
            }, ADMIN_UI_NAME);

            // Sphere colors legend
            elements.Add(new CuiLabel
            {
                Text = { Text = "Lobby🟣 | Gate🟢 | Spec🟡 | Spawns: 🔵SideA 🔴SideB | Teams in Lobby: 🟢Green 🔵Blue 🟠Orange 🟡Yellow 🟣Purple", FontSize = 8, Align = TextAnchor.MiddleCenter, Color = "0.5 0.5 0.5 1" },
                RectTransform = { AnchorMin = "0.05 0.06", AnchorMax = "0.95 0.10" }
            }, ADMIN_UI_NAME);

            // Close button
            elements.Add(new CuiButton
            {
                Button = { Color = "0.6 0.2 0.2 1", Command = "adminsetup.close" },
                RectTransform = { AnchorMin = "0.4 0.01", AnchorMax = "0.6 0.05" },
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

        [ConsoleCommand("adminsetup.setlobby")]
        private void ConsoleSetLobby(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                return;

            SetLobbyPosition(player);
            CuiHelper.DestroyUi(player, ADMIN_UI_NAME); // Destroy old UI first
            ShowAdminUI(player); // Refresh UI
        }

        [ConsoleCommand("adminsetup.setreturn")]
        private void ConsoleSetReturn(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                return;

            SetReturnToLobbySphere(player);
            CuiHelper.DestroyUi(player, ADMIN_UI_NAME);
            ShowAdminUI(player);
        }

        [ConsoleCommand("adminsetup.setleave")]
        private void ConsoleSetLeave(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                return;

            SetLeaveLobbyPosition(player);
            CuiHelper.DestroyUi(player, ADMIN_UI_NAME);
            ShowAdminUI(player);
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

        // Team selection sphere setters (for lobby)
        [ConsoleCommand("adminsetup.setlobbygreen")]
        private void ConsoleSetLobbyGreen(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                return;

            SetTeamSelectionSphere(player, "Green");
            CuiHelper.DestroyUi(player, ADMIN_UI_NAME);
            ShowAdminUI(player);
        }

        [ConsoleCommand("adminsetup.setlobbyblue")]
        private void ConsoleSetLobbyBlue(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                return;

            SetTeamSelectionSphere(player, "Blue");
            CuiHelper.DestroyUi(player, ADMIN_UI_NAME);
            ShowAdminUI(player);
        }

        [ConsoleCommand("adminsetup.setlobbyorange")]
        private void ConsoleSetLobbyOrange(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                return;

            SetTeamSelectionSphere(player, "Orange");
            CuiHelper.DestroyUi(player, ADMIN_UI_NAME);
            ShowAdminUI(player);
        }

        [ConsoleCommand("adminsetup.setlobbyyellow")]
        private void ConsoleSetLobbyYellow(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                return;

            SetTeamSelectionSphere(player, "Yellow");
            CuiHelper.DestroyUi(player, ADMIN_UI_NAME);
            ShowAdminUI(player);
        }

        [ConsoleCommand("adminsetup.setlobbypurple")]
        private void ConsoleSetLobbyPurple(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                return;

            SetTeamSelectionSphere(player, "Purple");
            CuiHelper.DestroyUi(player, ADMIN_UI_NAME);
            ShowAdminUI(player);
        }

        // Arena Gate Sphere setter
        [ConsoleCommand("adminsetup.setarenagate")]
        private void ConsoleSetArenaGate(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
                return;

            if (arg.Args.Length < 1 || !int.TryParse(arg.Args[0], out int arenaId) || arenaId < 1 || arenaId > 3)
            {
                player.ChatMessage("Usage: adminsetup.setarenagate <1-3>");
                return;
            }

            Vector3 position = player.transform.position;
            
            // Ensure list is initialized
            if (config.Global.ArenaGateSpheres == null)
            {
                config.Global.ArenaGateSpheres = new List<Vector3> { Vector3.zero, Vector3.zero, Vector3.zero };
            }
            
            // Ensure list has enough elements
            while (config.Global.ArenaGateSpheres.Count < 3)
            {
                config.Global.ArenaGateSpheres.Add(Vector3.zero);
            }
            
            config.Global.ArenaGateSpheres[arenaId - 1] = position;
            
            // Create sphere marker (dark grey for arena gates)
            CreateSphere(player, position, $"Arena {arenaId} Gate", new Color(0.3f, 0.3f, 0.3f, 0.5f));
            
            player.ChatMessage($"✓ Arena {arenaId} Gate sphere set at your position");
            
            CuiHelper.DestroyUi(player, ADMIN_UI_NAME);
            ShowAdminUI(player);
        }

        // Team spawn setters (for battle)
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
                int spawnIndex = arenaConfig.TeamSpawns.ContainsKey("SideA") ? arenaConfig.TeamSpawns["SideA"].Count : 0;
                SetTeamSpawn(player, "SideA", spawnIndex);
                CuiHelper.DestroyUi(player, ADMIN_UI_NAME);
                ShowAdminUI(player);
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
                int spawnIndex = arenaConfig.TeamSpawns.ContainsKey("SideB") ? arenaConfig.TeamSpawns["SideB"].Count : 0;
                SetTeamSpawn(player, "SideB", spawnIndex);
                CuiHelper.DestroyUi(player, ADMIN_UI_NAME);
                ShowAdminUI(player);
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

        private void SetLobbyPosition(BasePlayer player)
        {
            Vector3 position = player.transform.position;
            config.Global.LobbyCentral = position;
            player.ChatMessage($"✓ Central Lobby position set at {FormatVector3(position)}");
            
            CreateSphere(player, position, "Lobby", new Color(0.5f, 0f, 0.5f, 0.5f)); // Purple
        }

        private void SetReturnToLobbySphere(BasePlayer player)
        {
            Vector3 position = player.transform.position;
            config.Global.ReturnToLobbySphere = position;
            player.ChatMessage($"✓ Return to Lobby Sphere set at {FormatVector3(position)}");
            
            CreateSphere(player, position, "Return", new Color(0.3f, 0.5f, 0.3f, 0.5f)); // Green
        }

        private void SetLeaveLobbyPosition(BasePlayer player)
        {
            Vector3 position = player.transform.position;
            config.Global.LeaveLobby = position;
            player.ChatMessage($"✓ Leave Lobby position set at {FormatVector3(position)}");
            
            CreateSphere(player, position, "Leave", new Color(0.5f, 0.3f, 0.3f, 0.5f)); // Red
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

        private void SetTeamSelectionSphere(BasePlayer player, string team)
        {
            Vector3 position = player.transform.position;

            // Store in global config (single lobby for all arenas)
            if (config.Global.TeamColorSpheres == null)
            {
                config.Global.TeamColorSpheres = new Dictionary<string, Vector3>();
            }

            config.Global.TeamColorSpheres[team] = position;
            player.ChatMessage($"✓ {team} Team selection sphere set in central lobby");
            
            Color sphereColor = GetTeamColor(team);
            CreateSphere(player, position, $"{team} Team Sphere", sphereColor);
        }

        private void SetTeamSpawn(BasePlayer player, string team, int spawnIndex)
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
                if (!arenaConfig.TeamSpawns.ContainsKey(team))
                {
                    arenaConfig.TeamSpawns[team] = new List<Vector3>();
                }

                while (arenaConfig.TeamSpawns[team].Count <= spawnIndex)
                {
                    arenaConfig.TeamSpawns[team].Add(Vector3.zero);
                }

                arenaConfig.TeamSpawns[team][spawnIndex] = position;
                player.ChatMessage($"✓ {team} spawn #{spawnIndex + 1} set for Arena {arenaId}");
                
                Color sphereColor = GetTeamColor(team);
                CreateSphere(player, position, $"{team} #{spawnIndex + 1}", sphereColor);
            }
        }

        private Color GetTeamColor(string team)
        {
            switch (team)
            {
                case "SideA": return new Color(0.2f, 0.4f, 0.6f, 0.5f);   // Blue for Side A
                case "SideB": return new Color(0.6f, 0.2f, 0.2f, 0.5f);   // Red for Side B
                case "Green": return new Color(0f, 0.8f, 0f, 0.5f);    // Green (team selection only)
                case "Blue": return new Color(0f, 0.3f, 1f, 0.5f);     // Blue (team selection only)
                case "Orange": return new Color(1f, 0.5f, 0f, 0.5f);   // Orange (team selection only)
                case "Yellow": return new Color(1f, 1f, 0f, 0.5f);     // Yellow (team selection only)
                case "Purple": return new Color(0.6f, 0f, 0.8f, 0.5f); // Purple (team selection only)
                default: return new Color(0.5f, 0.5f, 0.5f, 0.5f);     // Gray
            }
        }

        // Keep old methods for backwards compatibility but redirect to new unified method
        private void SetSideASpawn(BasePlayer player, int spawnIndex)
        {
            SetTeamSpawn(player, "Blue", spawnIndex);
        }

        private void SetSideBSpawn(BasePlayer player, int spawnIndex)
        {
            SetTeamSpawn(player, "Red", spawnIndex);
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
