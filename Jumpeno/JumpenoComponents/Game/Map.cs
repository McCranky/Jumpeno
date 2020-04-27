using Jumpeno.JumpenoComponents.Editors;
using Jumpeno.JumpenoComponents.Entities;
using Jumpeno.JumpenoComponents.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Jumpeno.JumpenoComponents.Game {
    /**
     * Reprezentuje hracie pole, sa vyskytujú platformi a hráči.
     */
    public class Map : JumpenoComponent {
        public const int _TileSize = 64;
        public List<Platform> Platforms{ get; set; }
        private string backgroundColor;

        public override string CssStyle(bool smallScreen) => smallScreen ? $@"
            width: {((int)Math.Round(X/2, 0)).ToString()}px;
            height: {((int)Math.Round(Y/2, 0)).ToString()}px; 
            background-color: {backgroundColor};
            " : $@"
            width: {((int)Math.Round(X, 0)).ToString()}px;
            height: {((int)Math.Round(Y, 0)).ToString()}px; 
            background-color: {backgroundColor};
            ";

        public Map(Game game, MapTemplate template) {
            Game = game;
            Platforms = new List<Platform>();
            // pokiaľ z nejakého dôvodu nemáme pripravené žiadne mapy, tak sa aplikuje prednastavená varianta, aby bolo na čom hrať
            if (template != null) {
                Body.Position = new Vector2(
                    template.Width * _TileSize, // X - sirka
                    template.Height * _TileSize  // Y - vyska
                );
                backgroundColor = template.BackgroundColor;
                Name = template.Name;
                GeneratePlatforms(template.Tiles);
            } else {
                Body.Position = new Vector2(
                    16 * _TileSize, // X - sirka
                    9 * _TileSize  // Y - vyska
                );
                backgroundColor = "rgb(36, 30, 59)";
                Name = "Default";
                GeneratePlatforms(null);
            }
        }

        private void GeneratePlatforms(bool[] template) {
            if (template != null) {
                int width =  (int)Body.Position.X / _TileSize;
                int height =  (int)Body.Position.Y / _TileSize;
                for (int i = 0; i < width; i++) {
                    for (int j = 0; j < height; j++) {
                        if (template[Conversions.Map2DToIndex(i, j, width)]) {
                            Platforms.Add(new Platform("tile.png", new Vector2(i * _TileSize, j * _TileSize)));
                        }
                    }
                }
            } else { // ak nemáme template, tak vygenerujeme aspoň pár platforiem aby boli na mape nejake prekažky
                Platforms.Add(new Platform("tile.png", new Vector2(0 * _TileSize, 8 * _TileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(1 * _TileSize, 8 * _TileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(6 * _TileSize, 6 * _TileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(5 * _TileSize, 6 * _TileSize)));
            }
            
        }

        public void SpawnPlayers() {
            foreach (var player in Game.PlayersInGame) {
                player.Game = Game;
                player.Alive = true;
                player.Visible = true;
                player.SetBody();
                PositionPlayer(player);
            }
        }

        /**
         * Používa sa pri hernom režime Guided, kedy sa môže hráč napojiť počas odpočtu.
         */
        public void SpawnPlayer(Player player) {
            player.Game = Game;
            player.Alive = true;
            player.Visible = true;
            player.SetBody();
            PositionPlayer(player);
        }

        /**
         * Zmenšovanie mapy
         */
        public void Shrink() {
            float move = 64 * (1f/Game._FPS);
            X -= move;
            foreach (var platform in Platforms) {
                platform.X -= move / 2f;
            }
            foreach (var player in Game.PlayersInGame) {
                player.X -= move / 2f;
            }
        }

        /**
         * Metóda aktualizuje všetkých hráčov a skontroluje kolízie.
         */
        public override async Task Update(int fpsTickNum) {
            //DEBUG updates
            //System.Console.WriteLine("_FPS:" + fpsTickNum + "  Updated " + Count++ + " times.");
            foreach (var p in Game.PlayersInGame) {
                await p.Update(fpsTickNum);
            }

            Vector2 colissionDirection = new Vector2(0, 0);
            // player and walls collision
            foreach (var player in Game.PlayersInGame) {
                if (!player.Alive) {
                    continue;
                }
                // top border
                if (player.Y < 0) {
                    player.Y = 0;
                    player.OnCollision(new Vector2(0, -1));
                }
                // bottom border
                if (player.Y > Y - player.Body.Size.Y) {
                    player.Y = Y - player.Body.Size.Y;
                    player.OnCollision(new Vector2(0, 1));
                }
                // left border
                if (player.X <= 0) {
                    player.X = 0;
                    player.LeftColission = true;
                }
                //right border
                if (player.X >= X - player.Body.Size.X) {
                    player.X = X - player.Body.Size.X;
                    player.RightColission = true;
                }
            }

            // player and platform collision
            foreach (var player in Game.PlayersInGame) {
                if (!player.Alive) {
                    continue;
                }
                foreach (var platform in Platforms) {
                    if (!platform.Visible) {
                        continue;
                    }
                    if (player.GetCollider().CheckCollision(platform.GetCollider(), ref colissionDirection, 0)) {
                        player.OnCollision(colissionDirection);
                    }
                }
            }

            // players collision
            foreach (var pl1 in Game.PlayersInGame) {
                if (!pl1.Alive) {
                    continue;
                }
                foreach (var pl2 in Game.PlayersInGame) {
                    if (!pl2.Alive || pl1 == pl2) {
                        continue;
                    }
                    if (pl1.GetCollider().CheckCollision(pl2.GetCollider(), ref colissionDirection, 0)) {
                        if (colissionDirection.Y > 55 && colissionDirection.Y < 70 && pl1.Falling) { // skocil mu na hlavu
                            pl1.Kills += 1;
                            pl1.OnCollision(colissionDirection);
                            pl2.Die();
                            --Game.PlayersAllive;
                            if (Game.PlayersAllive == 1) {
                                return;
                            }
                        }
                    }
                }

            }

            // check for player crush while map shrinking
            foreach (var player in Game.PlayersInGame) {
                if (player.LeftColission && player.RightColission) {
                    if (Game.PlayersAllive == 1) {
                        //Game.Winner = player;
                        //EndGame = true;
                        return;
                    }

                    player.Die();
                    --Game.PlayersAllive;
                }

                player.LeftColission = false;
                player.RightColission = false;
            }


            //await Task.Run( () => base.Update(fpsTickNum)); // Iba ak by som potreboval animovať pozadie
        }

        /**
         * Nastaví hráčovi takú pozíciu, aby nekolidoval so žiadnou platformou a stenou
         */
        private void PositionPlayer(Player player) {
            Vector2 colissionDirection = new Vector2(0, 0);
            bool hit;
            do {
                hit = false;
                player.X = rnd.Next(0, (int)X - (int)player.Body.Size.X);
                player.Y = rnd.Next(0, (int)Y - (int)player.Body.Size.Y);
                
                foreach (var platform in Platforms) {
                    if (player.GetCollider().CheckCollision(platform.GetCollider(), ref colissionDirection, 0, false)) {
                        hit = true;
                        break;
                    }
                }
            } while (hit);
        }
    }
}
