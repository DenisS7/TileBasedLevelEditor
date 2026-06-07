using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Services.Rendering.Data;

namespace TileBasedLevelEditor.Services.Rendering.Data
{
    public class TilemapCell : GridCell
    {
        public List<TilemapCellTile> Tiles { get; set; } = new List<TilemapCellTile>();

        public TilemapCell(Vec2<int> index) 
            : base(index)
        { }
    }
}
