using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TileBasedLevelEditor.Models
{
    class Tilemap
    {
        public string Name { get; }

        public Vec2<int> TileSize { get; }

        public Vec2<int> TilemapSize { get; }

        public Dictionary<Vec2<int>, TileData> Tiles { get; }

        public Tilemap(string name, Vec2<int> tileSize, Vec2<int> tilemapSize)
        {
            Name = name;
            TileSize = tileSize;
            TilemapSize = tilemapSize;
            Tiles = new Dictionary<Vec2<int>, TileData>(tilemapSize.X * tilemapSize.Y);
        }

        public void SetTile(Vec2<int> tilemapIndex, Vec2<int> tilesetIndex, string tilesetName)
        {
            int tileIndex = tilemapIndex.Y * TilemapSize.X + tilemapIndex.X;
            if (0 > tileIndex || tileIndex >= Tiles.Count)
                return;

            Tiles[tilemapIndex].TilesetIndex = tilesetIndex;
            Tiles[tilemapIndex].TilesetName = tilesetName;
        }
    }
}
