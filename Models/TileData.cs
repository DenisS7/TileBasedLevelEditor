using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TileBasedLevelEditor.Models
{
    public struct TileData
    {
        public Vec2<int> TilesetIndex { get; set; }
        //public CroppedBitmap TileImage { get; set; }
        public string TilesetName { get; set; }

        public TileData()
        {
            TilesetIndex = new Vec2<int>(-1);
            TilesetName = "";
        }

        public TileData(Vec2<int> tilesetIndex, string tilesetName = "")
        {
            TilesetIndex = tilesetIndex;
            TilesetName = tilesetName;
        }
    }

}
