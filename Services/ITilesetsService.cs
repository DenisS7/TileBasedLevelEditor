using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedLevelEditor.Models;

namespace TileBasedLevelEditor.Services
{
    public interface ITilesetsService
    {
        IReadOnlyList<Tileset> Tilesets { get; }
        void Reload();
        void AddTileset(Tileset tileset);
    }
}
