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

        public int TileHeight { get; }

        public int TileWidth { get; }

        public int ImageHeight { get; }

        public int ImageWidth { get; }

        public byte[] ImageData { get; }


        public Tileset(string name, int tileHeight, int tileWidth, string imageSource)
        {
            Name = name;
            TileHeight = tileHeight;
            TileWidth = tileWidth;
            ImageData = File.ReadAllBytes(imageSource);

            if (ImageData.Length == 0)
                ImageHeight = ImageWidth = 0;
            else
            {
                using var ms = new MemoryStream(ImageData);
                var decoder = BitmapDecoder.Create(
                    ms,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad
                );
                var frame = decoder.Frames[0];
                ImageHeight = frame.PixelHeight;
                ImageWidth = frame.PixelWidth;
            }
        }
    }
}
