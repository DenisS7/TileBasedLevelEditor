using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TileBasedLevelEditor.Models
{
    class TileData
    {
        public Vec2<int> TilesetIndex { get; set; }
        public CroppedBitmap TileImage { get; set; }

        public TileData(Vec2<int> tilesetIndex, CroppedBitmap tileImage)
        {
            TilesetIndex = tilesetIndex;
            TileImage = tileImage;
        }
    }

}
