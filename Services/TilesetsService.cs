using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
