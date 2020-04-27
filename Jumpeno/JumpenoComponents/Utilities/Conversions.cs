namespace Jumpeno.JumpenoComponents.Utilities {
    public class Conversions {
        public static string FramesToTime(int frames) {
            int seconds = frames / Game.Game._FPS;
            return (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
        }

        public static string FramesToSec(int frames) {
            int seconds = frames / Game.Game._FPS;
            return seconds.ToString();
        }

        public static int Map2DToIndex(int x, int y, int width) {
            return width * y + x;
        }
    }
}
