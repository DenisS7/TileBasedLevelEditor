using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using NotesApp.Commands;
using NotesApp.ViewModel;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Views;

namespace TileBasedLevelEditor.ViewModels
{
    class TilesetViewModel : ViewModelBase
    {
        private Tileset? _currentTileset;

        public Tileset CurrentTileset
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
                }
                else
                {
                    TilesetImage = null;
                    OnPropertyChanged(nameof(ImageSize));
                    OnPropertyChanged(nameof(TileSize));
                }
            }
        }

        public Vec2<int> TileSize => CurrentTileset?.TileSize ?? new Vec2<int>(0);

        public Vec2<int> ImageSize => CurrentTileset?.ImageSize ?? new Vec2<int>(0);

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

        public TilesetViewModel()
        {
            _currentTileset = null;
            LoadTilesetCommand = new RelayCommand(OnLoadTileset);
        }
        public TilesetViewModel(Tileset currentTileset)
        {
            _currentTileset = currentTileset;
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

            string name = Path.GetFileNameWithoutExtension(path);

            //hardcoded size for now
            Vec2<int> tileSize = new Vec2<int>(32, 32);

            try
            {
                CurrentTileset = new Tileset(name, tileSize, path);
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
            }
            catch
            {
                TilesetImage = null;
            }
        }
    }
}
