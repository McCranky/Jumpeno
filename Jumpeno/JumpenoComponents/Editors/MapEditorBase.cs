using Jumpeno.JumpenoComponents.Game;
using Jumpeno.JumpenoComponents.Utilities;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.IO;

namespace Jumpeno.JumpenoComponents.Editors {
    /**
     * Slúži ako základna trieda pre komponent MapEditor.razor.
     * Umožňuje vytvárať, editovať a mazať mapy.
     */
    public class MapEditorBase : ComponentBase {
        public Dictionary<string, MapTemplate> Maps { get; set; }
        public const int _TileSize = Map._TileSize;
        public bool ShowMessage { get; set; }
        protected string message = "";
        public string MapPath { get; set; } = "";
        protected MapTemplate MapTemplate = null;
        public int EditorMode { get; set; }
        public MapEditorBase() { 
            Maps = new Dictionary<string, MapTemplate>();
            LoadMaps("./wwwroot/MapTemplates");
        }

        private void LoadMaps(string directoryPath) {
            if (Directory.Exists(directoryPath)) {
                string[] paths = Directory.GetFiles(directoryPath);
                MapTemplate map;
                foreach (var path in paths) {
                    if (!Maps.ContainsKey(path)) {
                        map = ReadFromFile(path);
                        Maps.Add(path, map);
                    }
                }
            }
        }

        protected void EditorChange() {
            if (EditorMode == 2) {
                CreateMapTemplate();
            } else if (EditorMode == 1) {
                LoadMaps("./wwwroot/MapTemplates");
            }
        }

        protected void RemoveMap() {
            if (!String.IsNullOrEmpty(MapPath)) {
                if (Maps.ContainsKey(MapPath)) {
                    Maps.Remove(MapPath);
                    if (File.Exists(MapPath)) {
                        File.Delete(MapPath);
                        MapPath = null;
                    }
                }
            } else {
                CreateMapTemplate();
            }
        }

        protected void DismissMessage() {
            ShowMessage = false;
        }


        protected void OnMapSelect() {
            if (MapPath != null && MapPath != "") {
                MapTemplate = new MapTemplate();
                if (!Maps.TryGetValue(MapPath, out MapTemplate)) {
                    // to by sa nemalo stať
                }
            }
        }

        protected void ChangeTile(int x, int y) {
            int index = MapTemplate.Width * y + x;
            MapTemplate.Tiles[index] = !MapTemplate.Tiles[index];
            Console.WriteLine($@"[X:{x}|Y:{y}] - Index:{index} set to {MapTemplate.Tiles[index]}");
            StateHasChanged();
        }

        protected void CreateMapTemplate() {
            MapTemplate = new MapTemplate { Height = 9, Width = 16, Name = "test", Tiles = new bool[16*9], BackgroundColor = "#241e3b" };
            MapPath = "";
            StateHasChanged();
        }

        protected void AddMap() {
            MapPath = "./wwwroot/MapTemplates\\" + MapTemplate.Name;
            if (Maps.ContainsKey(MapPath)) {
                ShowMessage = true;
                message = "Map with this name already exists.";
                StateHasChanged();
            } else {
                SaveMap();
            }
        }

        protected void SaveMap() {
            if (!String.IsNullOrEmpty(MapPath) && MapPath != "./wwwroot/MapTemplates\\" + MapTemplate.Name) {
                if (File.Exists(MapPath)) {
                    File.Delete(MapPath);
                    if (Maps.ContainsKey(MapPath)) {
                        Maps.Remove(MapPath);
                    }
                    MapPath = null;
                }
            }
            MapPath = String.IsNullOrEmpty(MapPath) ? ("./wwwroot/MapTemplates\\" + MapTemplate.Name) : MapPath;
            IOModule.WriteToBinaryFile<MapTemplate>(MapPath, MapTemplate);
            if (!Maps.ContainsKey(MapPath)) {
                Maps.Add(MapPath, MapTemplate);
            }
            message = "Map successfully modiffied.";
            ShowMessage = true;
        }

        private MapTemplate ReadFromFile(string path) {
            return IOModule.ReadFromBinaryFile<MapTemplate>(path);
        }
    }
}
