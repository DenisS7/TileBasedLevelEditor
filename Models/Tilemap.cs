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

        public HashSet<Guid> TilesetsUsed { get; }

        public List<Layer> Layers { get; }

        public Tilemap(string name, Vec2<int> tileSize, Vec2<int> tilemapSize)
        {
            Name = name;
            TileSize = tileSize;
            TilemapSize = tilemapSize;
            TilesetsUsed = new HashSet<Guid>();
            int tilemapArraySize = tilemapSize.X * tilemapSize.Y;
            Layers = [  new Layer("Layer 1", tilemapArraySize, 0),
                        new Layer("Layer 2", tilemapArraySize, 1),
                        new Layer("Layer 3", tilemapArraySize, 2),
                        new Layer("Layer 4", tilemapArraySize, 3)];
        }

        public int GetTilemapArrayIndex(Vec2<int> tilemapIndex)
        {
            return tilemapIndex.Y * TilemapSize.X + tilemapIndex.X;
        }

        public void SetTile(Vec2<int> tilemapIndex, Vec2<int> tilesetIndex, Layer layer, Guid tilesetID)
        {
            int tilemapArrayIndex = GetTilemapArrayIndex(tilemapIndex);
            SetTile(tilemapArrayIndex, tilesetIndex, layer, tilesetID);
        }

        public void SetTile(int tilemapIndex, Vec2<int> tilesetIndex, Layer layer, Guid tilesetID)
        {
            if (!Layers.Contains(layer))
                return;

            if (0 > tilemapIndex || tilemapIndex >= layer.Tiles.Length)
                return;

            layer.Tiles[tilemapIndex].TilesetIndex = tilesetIndex;
            layer.Tiles[tilemapIndex].TilesetID = tilesetID;

            TilesetsUsed.Add(tilesetID);
        }
    }
}
