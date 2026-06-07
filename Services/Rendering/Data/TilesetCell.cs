using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Services.Rendering.Data;

namespace TileBasedLevelEditor.Services.Rendering.Data
{
    public class TilesetCell : GridCell
    {
        public CroppedBitmap? Image { get; set; }

        public TilesetCell(Vec2<int> index, CroppedBitmap? image)
            : base(index)
        {
            Image = image;
        }
    }
}
