using Jumpeno.JumpenoComponents.Game;
using Jumpeno.JumpenoComponents.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Jumpeno.JumpenoComponents {
    /**
     * Vytvára, maže a udržiava všetky bežiace hry na servery.
     * Každých 30 sekún preverí neaktívne hry a tieto vymaže.
     */
    public class MatchController {
        public const int _CodeLength = 5;
        public const string _Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private System.Timers.Timer timer;
        private Random random = new Random();
        public const int _GamesLimit = 5;
        private int ticksToMapCollectionUpdate = 10;
        private MapTemplateCollection templateCollection;
        public Dictionary<string, Game.Game> Matches { get; set; }

        public MatchController() {
            Matches = new Dictionary<string, Game.Game>();
            timer = new System.Timers.Timer(30 * 1000);
            timer.Elapsed += (sender, e) => Tick(sender, e);
            timer.AutoReset = true;
            timer.Enabled = true;
            templateCollection = new MapTemplateCollection();
        }

        private void Tick(Object source, ElapsedEventArgs e) {
            Dictionary<string, Game.Game> temp = new Dictionary<string, Game.Game>();
            foreach (var match in Matches) {
                if (match.Value.GameState == GameState.DELETED) {
                    temp.Add(match.Key, match.Value);
                }
            }
            foreach (var toDel in temp) {
                Matches.Remove(toDel.Key);
            }
            temp.Clear();

            if (ticksToMapCollectionUpdate <= 0) {
                templateCollection.ReloadMaps();
            }
            --ticksToMapCollectionUpdate;
        }

        public void DeleteGame(string code) {
            Game.Game gm = null;
            if (Matches.TryGetValue(code, out gm)) {
                gm.StopGameEngine();
                Matches.Remove(code);
            }
            Matches.Remove(code);
        }

        public bool TryGetGame(string code, out Game.Game gm) {
            return Matches.TryGetValue(code, out gm);
        }

        public bool TryAddGame(InitialisingInfo initInfo, out Game.Game gm) {
            gm = null;
            if (Matches.Count < _GamesLimit) {
                initInfo.GameCode = GenerateCode();
                initInfo.GameName = String.IsNullOrEmpty(initInfo.GameName) ? "Unnamed" : initInfo.GameName; //("Game_" + Matches.Count)
                gm = new Game.Game(initInfo.GameCode, initInfo.GameName, initInfo.GameMode, initInfo.PlayersLimit, templateCollection);
                Matches.Add(gm.Code, gm);
                return true;
            }
            return false;
        }

        public string GenerateCode() {
            string code;
            do {
                code = new string(Enumerable.Repeat(_Chars, _CodeLength)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            } while (Matches.ContainsKey(code));
            return code;
        }
    }
}
