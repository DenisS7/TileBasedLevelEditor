using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TileBasedLevelEditor.Models
{
    public class Tileset
    {
        public string Name { get; private set;}

        public string FilePath { get; private set;}

        public Vec2<int> TileSize { get; private set;}

        public Vec2<int> NrTiles { get; private set;}

        public Vec2<int> ImageSize { get; private set;}

        [JsonConstructor]
        public Tileset(string name, string filePath,  Vec2<int> tileSize, Vec2<int> nrTiles, Vec2<int> imageSize)
        {
            Name = name;
            TileSize = tileSize;
            NrTiles = nrTiles;
            ImageSize = imageSize;
            FilePath = filePath;
        }

        public Tileset(string name, int tileHeight, int tileWidth, string path)
        {
            Name = name;
            TileSize = new Vec2<int>(tileHeight, tileWidth);
            ImageSize = new Vec2<int>(0);
            FilePath = path; 
            GetImageData(path);
            NrTiles = ImageSize / TileSize;
        }

        public Tileset(string name, Vec2<int> tileSize, string path)
        {
            Name = name;
            TileSize = tileSize;
            ImageSize = new Vec2<int>(0);
            FilePath = path;
            GetImageData(path);
            NrTiles = ImageSize / TileSize;
        }

        private void GetImageData(string path)
        {
            byte[] ImageData = File.ReadAllBytes(path);

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
