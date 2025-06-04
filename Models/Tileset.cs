using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TileBasedLevelEditor.Models
{
    public class Tileset
    {
        public string Name { get; }

        public Vec2<int> TileSize { get; }

        public Vec2<int> ImageSize { get; }

        public byte[] ImageData { get; }


        public Tileset(string name, int tileHeight, int tileWidth, string imageSource)
        {
            Name = name;
            TileSize = new Vec2<int>(tileHeight, tileWidth);
            ImageSize = new Vec2<int>(0);
            ImageData = File.ReadAllBytes(imageSource);

            if (ImageData.Length == 0)
                return;

            using var ms = new MemoryStream(ImageData);
            var decoder = BitmapDecoder.Create(
                ms,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad
            );
            var frame = decoder.Frames[0];
            ImageSize.X = frame.PixelHeight;
            ImageSize.Y = frame.PixelWidth;
        }
    }
}
