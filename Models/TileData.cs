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
        public Guid TilesetID { get; set; }

        public TileData()
        {
            TilesetIndex = new Vec2<int>(-1);
            TilesetID = Guid.Empty;
        }

        public TileData(Vec2<int> tilesetIndex, Guid tilesetID)
        {
            TilesetIndex = tilesetIndex;
            TilesetID = tilesetID;
        }
    }

}
