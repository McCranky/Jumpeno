using Jumpeno.JumpenoComponents.Game;

namespace Jumpeno.JumpenoComponents.Utilities {
    /**
     * Trieda obsahuje informácie na základe ktorých môže byť vytvorená hra
     */
    public class InitialisingInfo {
        public string ErrorMessage { get; set; } = "";
        public string GameCode { get; set; }
        public string GameName { get; set; }
        public int PlayersLimit { get; set; } = 2;
        public GameMode GameMode { get; set; } = GameMode.PLAYER;

        public static string GameModeToString(GameMode mode) {
            return mode switch {
                GameMode.PLAYER => "Player",
                GameMode.GUIDED => "Guided",
                _ => "",
            };
        }
    }
}
