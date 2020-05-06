using Jumpeno.JumpenoComponents.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace Jumpeno.JumpenoComponents.Game {
    public enum GameMode {
        PLAYER, GUIDED
    }

    public enum GameState {
        LOBBY, COUNTDOWN, SHRINKING, GAMEOVER, SCOREBOARD, DELETED
    }

    public class GameTickEventArgs : EventArgs {
        public int FpsTickNum { set; get; }
    }

    /**
     * Obsahuje všetky časti hry. Mapu, hračov a zabezpečuje chod hry.
     */
    public class Game : JumpenoComponent, IDisposable {
        public MapTemplateCollection MapTemplates { get; protected set; }
        public GameState GameState { get; set; }
        public GameMode GameMode { get; set; }
        public Map Map { get; private set; }
        public int PlayersLimit { get; set; }
        public string Code { get; set; }
        public List<Player> PlayersInGame { get; set; }
        public List<Player> PlayersInLobby { get; set; }
        public Player Creator { get; set; }
        public Player Winner { get; set; }
        public int PlayersAllive { get; set; }
        public int FPSElapsed { get; set; }
        public int FramesToShrink { get; set; }
        public int FramesToStart { get; set; } // meni sa v zavyslosťi od počtu hračov
        public int FramesToScoreboard { get; set; }
        public int FramesToLobby { get; set; }
        public bool StoppedStartTimer { get; set; }
        public bool CountdownTimerRunning { get; set; }
        public bool ShrinkingAllowed { get; set; } = true;
        public bool StartTimerRunning { get; set; }
        public bool GameoverTimerRunning { get; set; }
        public bool ScoreboardTimerRunning { get; set; }
        public bool DeleteTimerRunning { get; set; }
        public const int _FPS = 60;
        public const double _MillisecondInSecond = 1000.0;
        private int currentFPS = 1;
        private int deleteFrames;
        public const int _MillisecondsDelay = 1000 / _FPS;
        private System.Timers.Timer timer;
        public override string CssStyle(bool smallScreen) => $@"
            top: 0px ;
            left: 0px;
            width: 100%;
            height: 100%; 
            ";
        public event EventHandler<GameTickEventArgs> OnTickReached;
        private void OnTick(GameTickEventArgs e) {
            var handler = OnTickReached;
            handler?.Invoke(this, e);
        }
        public Game(string code, string name, GameMode mode, int playersCount, MapTemplateCollection templates) {
            Name = name;
            MapTemplates = templates;
            PlayersLimit = playersCount;
            Code = code;
            GameMode = mode;
            PlayersInGame = new List<Player>(PlayersLimit);
            PlayersInLobby = new List<Player>(PlayersLimit);
            GameState = GameState.LOBBY;
            RestartTimers();
            StartGameEngine();
        }

        public bool AddPlayer(Player player) {
            if (GameMode == GameMode.GUIDED) {
                if (GameState == GameState.COUNTDOWN) {
                    if (PlayersInGame.Count < PlayersLimit) { // pripoji hrača do bežiacej hry
                        PlayersInGame.Add(player);
                        PlayersInLobby.Add(player);
                        player.InGame = true;
                        Map.SpawnPlayer(player);
                        ++PlayersAllive;
                        return true;
                    }
                    return false;
                } else { // Lobby
                    if (PlayersInLobby.Count < PlayersLimit) {
                        PlayersInLobby.Add(player);
                        return true;
                    }
                    return false;
                }
            } else { // Player Mode - hraju iba hrači ktory boli pri štarte, ostatny čakaju na skončenie hry
                if (PlayersInLobby.Count < PlayersLimit) {
                    PlayersInLobby.Add(player);
                    if (PlayersInLobby.Count == 1) {
                        Creator = player;
                    } else if (GameState == GameState.LOBBY){
                        FramesToStart /= 2;
                    }
                    return true;
                }
                return false;
            }
        }

        public void RemovePlayer(Player player) {
            if (player.Spectator) {
                return;
            }
            if (GameState != GameState.LOBBY) {
                lock(PlayersInGame) PlayersInGame.Remove(player);
                --PlayersAllive;
            }
            PlayersInLobby.Remove(player);
            if (PlayersInLobby.Count >= 1) {
                Creator = PlayersInLobby[rnd.Next(0, PlayersInLobby.Count - 1)];
                if (StartTimerRunning) {
                    FramesToStart *= 2;
                }
            }
            player.InGame = false;
        }

        public void RespawnPlayer(Player player) {
            player.X = rnd.Next(0, (int)Map.X - (int)player.Body.Size.X);
            player.Y = rnd.Next(0, (int)Map.Y - (int)player.Body.Size.Y);
            player.Visible = true;
            player.Alive = true;
            ++PlayersAllive;
        }

        public void Start() {
            //RestartTimers();

            PlayersInGame = new List<Player>(PlayersInLobby);
            foreach (var pl in PlayersInGame) {
                pl.InGame = true;
            }
            if (GameMode == GameMode.GUIDED) {
                PlayersInGame.Remove(Creator);
            }
            PlayersAllive = PlayersInGame.Count;
            Map = new Map(this, MapTemplates.GetRandomMap()); 
            Map.SpawnPlayers();
            StartTimerRunning = false;
            GameState = GameState.COUNTDOWN; // musi byt posledné, inak by sa mohlo pristupovať ku null objektom
        }

        public void StartGameEngine() {
            timer = new System.Timers.Timer(_MillisecondInSecond / _FPS);
            timer.Elapsed += async (sender, e) => await TickAsync(sender, e);
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        public void StopGameEngine() {
            timer.Elapsed -= async (sender, e) => await TickAsync(sender, e);
            timer.Enabled = false;
            OnTick(new GameTickEventArgs { FpsTickNum = currentFPS });
        }

        private void RestartTimers() {
            //[MINUTES] * [SECOND] * FRAMES
            FramesToStart = 2 * 60 * _FPS;
            FramesToLobby = 10 * _FPS;
            FramesToScoreboard = 5 * _FPS;
            FramesToShrink = 2 * 60 * _FPS;

            StoppedStartTimer = false;
            CountdownTimerRunning = true;
            ShrinkingAllowed = true;
            GameoverTimerRunning = true;
            ScoreboardTimerRunning = true;
    }

        /**
         * Metóda, ktorú volá "timer" každých (_MillisecondInSecond / _FPS) milisekund.
         * Zabezpečuje aktualizaciu hernej logiky.
         */
        private async Task TickAsync(Object source, ElapsedEventArgs e) {
            OnTick(new GameTickEventArgs { FpsTickNum = currentFPS });
            if (GameState == GameState.COUNTDOWN) {
                await Map.Update(currentFPS);
                if (PlayersAllive <= 1) {
                    GameState = GameState.GAMEOVER;
                    return;
                }
                if (FramesToShrink <= 0) {
                    GameState = GameState.SHRINKING;
                } else {
                    if (CountdownTimerRunning) {
                        --FramesToShrink;
                    }
                }
            } else if (GameState == GameState.SHRINKING) {
                await Map.Update(currentFPS);
                if (PlayersAllive == 1) {
                    GameState = GameState.GAMEOVER;
                    return;
                }
                if (Map.X > 0) {
                    if (ShrinkingAllowed) {
                        Map.Shrink();
                    }
                } else {
                    GameState = GameState.GAMEOVER;
                }
            } else if (GameState == GameState.LOBBY) {
                if (GameMode == GameMode.GUIDED) {
                    // samospustenie
                    if (StartTimerRunning) {
                        --FramesToStart;
                        if (FramesToStart <= 0) {
                            StartTimerRunning = false;
                            Start();
                        }
                    }
                } else {
                    if (PlayersInLobby.Count > 1) {
                        if (!StartTimerRunning && !StoppedStartTimer) {
                            StartTimerRunning = true;
                            if (PlayersInLobby.Count > 2) {
                                for (int i = 0; i < PlayersInLobby.Count - 2; i++) {
                                    FramesToStart /= 2;
                                }
                            }
                        } else {
                            // samospustenie
                            if (!StoppedStartTimer) {
                                --FramesToStart;
                            }
                            if (FramesToStart <= 0) {
                                StartTimerRunning = false;
                                Start();
                            }
                        }

                    } else if (PlayersInLobby.Count == 1) {
                        if (StartTimerRunning) {
                            StartTimerRunning = false;
                        }
                        if (DeleteTimerRunning) {
                            DeleteTimerRunning = false;
                        }
                    } else {
                        if (!DeleteTimerRunning) {
                            DeleteTimerRunning = true;
                            deleteFrames = 10 * _FPS;
                        } else {
                            --deleteFrames;
                            if (deleteFrames <= 0) {
                                Map = null;
                                Dispose();
                            }
                        }
                    }
                }
            } else if (GameState == GameState.GAMEOVER) {
                await Map.Update(currentFPS);
                if (GameoverTimerRunning) {
                    --FramesToScoreboard;
                }
                if (FramesToScoreboard <= 0) {
                    foreach (var player in PlayersInGame) {
                        if (player.Alive) {
                            Winner = player;
                            break;
                        }
                    }
                    GameState = GameState.SCOREBOARD;
                }
            } else if (GameState == GameState.SCOREBOARD) {
                if (ScoreboardTimerRunning) {
                    --FramesToLobby;
                }
                if (FramesToLobby <= 0) {
                    foreach (var pl in PlayersInGame) {
                        pl.InGame = false;
                        pl.Freeze();
                    }
                    Creator.InGame = false;
                    PlayersInGame.Clear();
                    RestartTimers();
                    GameState = GameState.LOBBY;
                }
            }
            currentFPS = (currentFPS % 60) + 1;
            ++FPSElapsed;
        }

        public Player GetPlayer(string playerKey) {
            if (GameState == GameState.LOBBY) {
                foreach (var player in PlayersInLobby) {
                    if (player.GetKey == playerKey) {
                        return player;
                    }
                }
            } else {
                foreach (var player in PlayersInGame) {
                    if (player.GetKey == playerKey) {
                        return player;
                    }
                }
            }
            
            return null;
        }

        public void Dispose() {
            timer.Elapsed -= async (sender, e) => await TickAsync(sender, e);
            timer.Enabled = false;
            GameState = GameState.DELETED;
            OnTick(new GameTickEventArgs { FpsTickNum = -1 });
        }
    }
}
