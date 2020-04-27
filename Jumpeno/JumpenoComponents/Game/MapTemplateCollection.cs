using System;
using System.Collections.Generic;
using System.IO;
using Jumpeno.JumpenoComponents.Utilities;

namespace Jumpeno.JumpenoComponents.Game {
    /**
     * Trieda slúži na udržiavanie máp počas behu servera. Mapy poskytuje pri vytváraní hri, kedy sa vyberie jedna náhodná.
     */
    public class MapTemplateCollection {
        private readonly Random rnd = new Random();
        private List<MapTemplate> Maps { get; set; }
        public const string _MapFolderPath = "./wwwroot/MapTemplates";
        public MapTemplateCollection() {
            ReloadMaps();
        }

        public void ReloadMaps() {
            Maps = new List<MapTemplate>();
            if (Directory.Exists(_MapFolderPath)) {
                string[] paths = Directory.GetFiles(_MapFolderPath);
                foreach (var path in paths) {
                    Maps.Add(IOModule.ReadFromBinaryFile<MapTemplate>(path));
                }
            }
        }

        public MapTemplate GetRandomMap() {
            if (Maps.Count > 0) {
                return Maps[rnd.Next(0, Maps.Count)];
            }
            return null;
        }
    }
}
