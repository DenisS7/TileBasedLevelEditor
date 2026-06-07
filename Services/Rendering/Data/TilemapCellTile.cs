using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TileBasedLevelEditor.Models;

namespace TileBasedLevelEditor.Services.Rendering.Data
{
    public class TilemapCellTile
    {
        public TileData TileData { get; set; }
        public CroppedBitmap? Image { get; set; }
        public Layer Layer { get; set; }

        public TilemapCellTile(TileData tileData,  CroppedBitmap? image, Layer layer)
        {
            TileData = tileData;
            Image = image;
            Layer = layer;
        }
    }
}
