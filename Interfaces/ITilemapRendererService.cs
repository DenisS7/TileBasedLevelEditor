using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Services.Rendering.Data;

namespace TileBasedLevelEditor.Interfaces
{
    public interface ITilemapRendererService
    {
        List<TilemapCell> GetRenderedTilemapCells(Tilemap tilemap);
    }
}
