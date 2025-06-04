using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TileBasedLevelEditor.Models
{
    public class Tileset
    {
        public string Name { get; }

        public Vec2<int> TileSize { get; }

        public Vec2<int> ImageSize { get; }

        public byte[] ImageData { get; private set; }


        public Tileset(string name, int tileHeight, int tileWidth, string path)
        {
            Name = name;
            TileSize = new Vec2<int>(tileHeight, tileWidth);
            ImageSize = new Vec2<int>(0);
            ImageData = [];
            GetImageData(path);
        }

        public Tileset(string name, Vec2<int> tileSize, string path)
        {
            Name = name;
            TileSize = tileSize;
            ImageSize = new Vec2<int>(0);
            ImageData = [];
            GetImageData(path);
        }

        private void GetImageData(string path)
        {
            ImageData = File.ReadAllBytes(path);

            if (ImageData.Length == 0)
                return;

            using var ms = new MemoryStream(ImageData);
            var decoder = BitmapDecoder.Create(
                ms,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad
            );
            var frame = decoder.Frames[0];
            ImageSize.X = frame.PixelWidth;
            ImageSize.Y = frame.PixelHeight;
        }
    }
}
