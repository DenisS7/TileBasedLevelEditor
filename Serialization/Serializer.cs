using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using TileBasedLevelEditor.Models;

namespace TileBasedLevelEditor.Serialization
{
    public static class Serializer
    {
        static string TilesetsFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Save");
        static List<string> TilesetsPaths = [];
        public static List<Tileset> DeserializeTilesets()
        {
            List<Tileset> tilesets = [];
            string existingTilesetsPath = Path.Combine(TilesetsFilesPath, "Tilesets.json");
            if (!File.Exists(existingTilesetsPath))
                return tilesets;

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            string tilesetsPathJson = File.ReadAllText(existingTilesetsPath);
            List<string>? LoadedTilesetsPaths = JsonConvert.DeserializeObject<List<string>>(tilesetsPathJson, settings);

            if (LoadedTilesetsPaths == null)
                return tilesets;

            TilesetsPaths = LoadedTilesetsPaths;

            foreach ( string tilesetPath in TilesetsPaths )
            {
                if (!File.Exists(tilesetPath)) 
                    continue;

                string tilesetJson = File.ReadAllText(tilesetPath);
                Tileset? tileset = JsonConvert.DeserializeObject<Tileset>(tilesetJson, settings);

                if (tileset == null)
                    continue;

                tilesets.Add(tileset);
            }

            return tilesets;
        }

        public static bool SaveTileset(Tileset tileset)
        {
            var dlg = new SaveFileDialog
            {
                Title = "Save tileset",
                Filter = "Json files (*.json)|*.json",
                DefaultExt = ".json",
                FileName = tileset.Name,
                OverwritePrompt = true
            };

            bool? result = dlg.ShowDialog();
            if (result != true)
                return false;

            string path = dlg.FileName;
            TilesetsPaths.Add(path);

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            string tilesetJson = JsonConvert.SerializeObject(tileset, settings);
            File.WriteAllText(path, tilesetJson);

            string existingTilesetsPath = Path.Combine(TilesetsFilesPath, "Tilesets.json");
            if (!Path.Exists(TilesetsFilesPath))
            {
                Directory.CreateDirectory(TilesetsFilesPath);
            }

            string tilesetsPathJson = JsonConvert.SerializeObject(TilesetsPaths, settings);
            File.WriteAllText(existingTilesetsPath, tilesetsPathJson);
           
            return true;
        }
    }
}
