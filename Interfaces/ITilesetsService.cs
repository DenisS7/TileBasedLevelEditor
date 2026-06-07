using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TileBasedLevelEditor.Models;

namespace TileBasedLevelEditor.Services
{
    public interface ITilesetsService
    {
        public Dictionary<Guid, Tileset> Tilesets { get; }
        public void Reload();
        public void AddTileset(Tileset tileset);
        public CroppedBitmap? GetTileImageAt(Guid tilesetID, Vec2<int> tilesetIndex);
        public CroppedBitmap? GetTileImageAt(Guid tilesetID, int tilesetIndex);
    }
}
