using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Serialization;

namespace TileBasedLevelEditor.Services
{
    public class TilesetsService : ITilesetsService
    {
        private Dictionary<Guid, Tileset> _tilesets;
        public Dictionary<Guid, Tileset> Tilesets => _tilesets;

        public TilesetsService() 
        {
            _tilesets = Serializer.DeserializeTilesets();
        }

        public void Reload()
        {
            _tilesets = Serializer.DeserializeTilesets();
        }

        public void AddTileset(Tileset tileset)
        {
            _tilesets.Add(tileset.ID, tileset);
        }

        public CroppedBitmap? GetTileImageAt(Guid tilesetID, Vec2<int> tilesetIndex)
        {
            Tileset? tileset;
            if (!Tilesets.TryGetValue(tilesetID, out tileset))
                return null;

            int tilesetArrayIndex = tilesetIndex.X + tilesetIndex.Y * tileset.NrTiles.X;
            return tileset.TileImages[tilesetArrayIndex];
        }

        public CroppedBitmap? GetTileImageAt(Guid tilesetID, int tilesetIndex)
        {
            Tileset? tileset;
            if (!Tilesets.TryGetValue(tilesetID, out tileset))
                return null;

            return tileset.TileImages[tilesetIndex];
        }
    }
}
