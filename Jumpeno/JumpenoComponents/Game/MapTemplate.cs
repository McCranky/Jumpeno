using System;

namespace Jumpeno.JumpenoComponents.Game {
    /**
     * Reprezentuje informácie o mape, ktoré su zapisovane alebo čitane z binarneho suboru, alebo použite pri inicializacii mapy.
     */
    [Serializable]
    public class MapTemplate {
        public string Name { get; set; }
        public string BackgroundColor { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool[] Tiles { get; set; } // znázorňuje, či sa na danom políčku nachádza platforma alebo nie
    }
}
