using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TileBasedLevelEditor.Models
{
    public class Tilemap
    {
        public string Name { get; }

        public Vec2<int> TileSize { get; }

        public Vec2<int> TilemapSize { get; }

        public TileData[] Tiles { get; }

        public HashSet<string> TilesetsUsed { get; }

        public List<Layer> Layers { get; } = [
            new Layer("Layer 1"), 
            new Layer("Layer 2"), 
            new Layer("Layer 3"), 
            new Layer("Layer 4")];

        public Tilemap(string name, Vec2<int> tileSize, Vec2<int> tilemapSize)
        {
            Name = name;
            TileSize = tileSize;
            TilemapSize = tilemapSize;
            Tiles = new TileData[tilemapSize.X * tilemapSize.Y];
            TilesetsUsed = new HashSet<string>();
        }

        public int GetTilemapArrayIndex(Vec2<int> tilemapIndex)
        {
            return tilemapIndex.Y * TilemapSize.X + tilemapIndex.X;
        }

        public void SetTile(Vec2<int> tilemapIndex, Vec2<int> tilesetIndex, string tilesetName)
        {
            int tilemapArrayIndex = GetTilemapArrayIndex(tilesetIndex);
            if (0 > tilemapArrayIndex || tilemapArrayIndex >= Tiles.Length)
                return;


            Tiles[tilemapArrayIndex].TilesetIndex = tilesetIndex;
            Tiles[tilemapArrayIndex].TilesetName = tilesetName;

            TilesetsUsed.Add(tilesetName);
        }
    }
}
