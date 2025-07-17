using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Shapes;

namespace TileBasedLevelEditor.Models
{
    public class Tileset
    {
        public string Name { get; private set;}

        public string FilePath { get; private set;}

        public Vec2<int> TileSize { get; private set;}

        public Vec2<int> NrTiles { get; private set;}

        public Vec2<int> ImageSize { get; private set;}

        [JsonIgnore]
        public List<CroppedBitmap?> TileImages { get; private set; }

        [JsonConstructor]
        public Tileset(string name, string filePath,  Vec2<int> tileSize, Vec2<int> nrTiles, Vec2<int> imageSize)
        {
            Name = name;
            TileSize = tileSize;
            NrTiles = nrTiles;
            ImageSize = imageSize;
            FilePath = filePath;
            TileImages = GetTilesetTiles(filePath);
        }

        public Tileset(string name, int tileHeight, int tileWidth, string filePath)
        {
            Name = name;
            TileSize = new Vec2<int>(tileHeight, tileWidth);
            ImageSize = new Vec2<int>(0);
            FilePath = filePath;
            GetImageData(filePath);
            NrTiles = ImageSize / TileSize;
            TileImages = GetTilesetTiles(filePath);
        }

        public Tileset(string name, Vec2<int> tileSize, string filePath)
        {
            Name = name;
            TileSize = tileSize;
            ImageSize = new Vec2<int>(0);
            FilePath = filePath;
            GetImageData(filePath);
            NrTiles = ImageSize / TileSize;
            TileImages = GetTilesetTiles(filePath);
        }

        private List<CroppedBitmap?> GetTilesetTiles(string filePath)
        {
            List<CroppedBitmap?> TileImages = [];

            byte[] ImageData = File.ReadAllBytes(filePath);
            if (ImageData.Length == 0)
            {
                return TileImages;
            }

            try
            {
                var bmp = new BitmapImage();
                using (var ms = new MemoryStream(ImageData))
                {
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    bmp.EndInit();
                    bmp.Freeze();
                }
                ImageSource TilesetImage = bmp;

                for (int y = 0; y < NrTiles.Y; y++)
                {
                    for (int x = 0; x < NrTiles.X; x++)
                    {
                        var rect = new Int32Rect(
                            x * TileSize.X,
                            y * TileSize.Y,
                            TileSize.X,
                            TileSize.Y
                        );
                        var tileBmp = new CroppedBitmap(bmp, rect);
                        TileImages.Add(tileBmp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading tileset image:\n{ex.Message}",
                    "Load Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            return TileImages;
        }

        private void GetImageData(string filePath)
        {
            byte[] ImageData = File.ReadAllBytes(filePath);

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
