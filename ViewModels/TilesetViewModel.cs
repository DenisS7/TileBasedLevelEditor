using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using NotesApp.Commands;
using NotesApp.ViewModel;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Services;
using TileBasedLevelEditor.Views;

namespace TileBasedLevelEditor.ViewModels
{
    class TilesetViewModel : ViewModelBase
    {
        private Tileset? _currentTileset;

        public Tileset? CurrentTileset
        {
            get => _currentTileset;
            private set
            {
                if (_currentTileset == value) return;
                _currentTileset = value;
                OnPropertyChanged(nameof(CurrentTileset));

                if (_currentTileset != null)
                {
                    GetTilesetImage();
                    OnPropertyChanged(nameof(ImageSize));
                    OnPropertyChanged(nameof(TileSize));
                    OnPropertyChanged(nameof(NrTiles));
                }
                else
                {
                    TilesetImage = null;
                    OnPropertyChanged(nameof(ImageSize));
                    OnPropertyChanged(nameof(TileSize));
                    OnPropertyChanged(nameof(NrTiles));
                }
            }
        }

        public Vec2<int> TileSize => CurrentTileset?.TileSize ?? new Vec2<int>(0);

        public Vec2<int> ImageSize => CurrentTileset?.ImageSize ?? new Vec2<int>(0);

        public Vec2<int> NrTiles => CurrentTileset?.NrTiles ?? new Vec2<int>(0);

        private ImageSource? _tilesetImage;

        public ImageSource? TilesetImage
        {
            get => _tilesetImage;
            private set
            {
                if (_tilesetImage == value) return;
                _tilesetImage = value;
                OnPropertyChanged(nameof(TilesetImage));
            }
        }

        public ICommand LoadTilesetCommand { get; }

        public TileGridViewModel TileGridVM { get; }

        public TilesetViewModel()
        {
            _currentTileset = null;
            LoadTilesetCommand = new RelayCommand(OnLoadTileset);
            TileGridVM = new TileGridViewModel(TileSize, NrTiles, new Vec2<int>(2, 2), null, OnTileSelected);
        }

        public TilesetViewModel(Tileset currentTileset)
        {
            _currentTileset = currentTileset;
            GetTilesetImage();
            TileGridVM = new TileGridViewModel(TileSize, NrTiles, new Vec2<int>(2, 2));
            LoadTilesetCommand = new RelayCommand(OnLoadTileset);
        }

        private void OnLoadTileset(object? parameter)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select a Tileset Image",
                Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|All Images|*.png;*.jpg;*.jpeg"
            };

            bool? result = dlg.ShowDialog();
            if (result != true) 
                return;

            string path = dlg.FileName;
            if (!File.Exists(path)) 
                return;

            string name = System.IO.Path.GetFileNameWithoutExtension(path);

            //hardcoded size for now
            Vec2<int> tileSize = new Vec2<int>(32, 32);

            try
            {
                CurrentTileset = new Tileset(name, tileSize, path);
                TileGridVM.TileSize = tileSize;
                TileGridVM.NrTiles = NrTiles;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading tileset:\n{ex.Message}",
                    "Load Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void GetTilesetImage()
        {
            if (CurrentTileset?.ImageData == null)
            {
                TilesetImage = null;
                return;
            }

            TileGridVM.TileImages.Clear();
            try
            {
                var bmp = new BitmapImage();
                using (var ms = new MemoryStream(CurrentTileset.ImageData))
                {
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    bmp.EndInit();
                    bmp.Freeze();
                }
                TilesetImage = bmp;

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
                        TileGridVM.TileImages.Add(tileBmp);
                    }
                }
            }
            catch
            {
                TilesetImage = null;
            }
        }

        private void OnTileSelected(Vec2<int>? vec)
        {
            if (CurrentTileset == null || TileGridVM.InitialSelectedTile == null)
                return;

            if (TileGridVM.InitialSelectedTile < 0 || TileGridVM.InitialSelectedTile >= NrTiles)
                return;

            HashSet<TileData> SelectedTiles = [];
            foreach (SelectionArea selectionArea in TileGridVM.SelectionAreas)
            {
                for (int i = selectionArea.StartTile.X; i <= selectionArea.EndTile.X; i++)
                {
                    for (int j = selectionArea.StartTile.Y; j <= selectionArea.EndTile.Y; j++)
                    {
                        SelectedTiles.Add(new TileData(new Vec2<int>(i, j), CurrentTileset.Name));
                    }
                }
            }

            List<Tuple<TileData, CroppedBitmap?>>? SelectedTilesFull = [];
            foreach (TileData tileData in SelectedTiles)
            {
                CroppedBitmap? tileImage = TileGridVM.TileImages[tileData.TilesetIndex.X + tileData.TilesetIndex.Y * NrTiles.X];
                SelectedTilesFull.Add(new Tuple<TileData, CroppedBitmap?>(tileData, tileImage));
            }

            TileSelectedService.SelectedTiles = SelectedTilesFull;
        }
    }
}
