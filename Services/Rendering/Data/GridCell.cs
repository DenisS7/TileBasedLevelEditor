using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedLevelEditor.Models;

namespace TileBasedLevelEditor.Services.Rendering.Data
{
    public class GridCell
    {
        public Vec2<int> Index;

        public GridCell(Vec2<int> index)
        {
            Index = index;
        }
    }
}
