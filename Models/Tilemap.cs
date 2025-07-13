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

        public TileData[] Tiles { get; }

        public HashSet<string> TilesetsUsed { get; }

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
            int tileIndex = tilemapIndex.Y * TilemapSize.X + tilemapIndex.X;
            if (0 > tileIndex || tileIndex >= Tiles.Length)
                return;

            int tilemapArrayIndex = GetTilemapArrayIndex(tilesetIndex);

            Tiles[tilemapArrayIndex].TilesetIndex = tilesetIndex;
            Tiles[tilemapArrayIndex].TilesetName = tilesetName;

            TilesetsUsed.Add(tilesetName);
        }
    }
}
