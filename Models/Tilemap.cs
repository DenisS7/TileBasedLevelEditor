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
        public string Name { get; set; }

        public Vec2<int> TileSize { get; set; }

        public Vec2<int> TilemapSize { get; set; }

        public TileData[] Tiles { get; set; }

        public HashSet<Guid> TilesetsUsed { get; }

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
            TilesetsUsed = new HashSet<Guid>();
        }

        public int GetTilemapArrayIndex(Vec2<int> tilemapIndex)
        {
            return tilemapIndex.Y * TilemapSize.X + tilemapIndex.X;
        }

        public void SetTile(Vec2<int> tilemapIndex, Vec2<int> tilesetIndex, Guid tilesetID)
        {
            int tilemapArrayIndex = GetTilemapArrayIndex(tilemapIndex);
            if (0 > tilemapArrayIndex || tilemapArrayIndex >= Tiles.Length)
                return;

            Tiles[tilemapArrayIndex].TilesetIndex = tilesetIndex;
            Tiles[tilemapArrayIndex].TilesetID = tilesetID;

            TilesetsUsed.Add(tilesetID);
        }
    }
}
