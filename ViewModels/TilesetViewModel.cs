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

        //Right & Bottom should always be 0
        public Thickness TileMargin { get; } = new Thickness(2, 2, 0, 0);

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

        public ObservableCollection<CroppedBitmap> TileImages { get; private set; } = [];

        private bool _isTileHovered = true;

        public bool IsTileHovered
        {
            get => _isTileHovered;
            set
            {
                _isTileHovered = value;
                OnPropertyChanged(nameof(IsTileHovered));
            }
        }

        private Vec2<int>? _hoveredTileIndex = new Vec2<int>(-1);

        public Vec2<int>? HoveredTileIndex
        {
            get => _hoveredTileIndex;
            set
            {
                _hoveredTileIndex = value;
                OnPropertyChanged(nameof(HoveredTileIndex));
                if (_hoveredTileIndex != null && _hoveredTileIndex.X >= 0 && _hoveredTileIndex.Y >= 0)
                {
                    IsTileHovered = true;
                    HoveredTileLocation = new Vec2<double>(_hoveredTileIndex.X * (TileSize.X + TileMargin.Left) + TileMargin.Left, _hoveredTileIndex.Y * (TileSize.Y + TileMargin.Top) + TileMargin.Top);
                }
                else
                {
                    IsTileHovered = false;
                    HoveredTileLocation = new Vec2<double>(0.0, 0.0);
                }
                OnPropertyChanged(nameof(IsTileHovered));
                OnPropertyChanged(nameof(HoveredTileLocation));
            }
        }

        private Vec2<double> _hoveredTileLocation = new Vec2<double>(0.0);

        public Vec2<double> HoveredTileLocation
        {
            get => _hoveredTileLocation;
            set
            {
                _hoveredTileLocation = value;
                OnPropertyChanged(nameof(HoveredTileLocation));
            }
        }

        private bool _isTileSelected = false;

        public bool IsTileSelected
        {
            get => _isTileSelected;
            set
            {
                _isTileSelected = value;
                OnPropertyChanged(nameof(IsTileSelected));
            }
        }

        private Vec2<int>? _selectedTileIndex = new Vec2<int>(-1);

        public Vec2<int>? SelectedTileIndex
        {
            get => _selectedTileIndex;
            set
            {
                _selectedTileIndex = value;
                OnPropertyChanged(nameof(SelectedTileIndex));
                if (_selectedTileIndex != null && _selectedTileIndex.X >= 0 && _selectedTileIndex.Y >= 0)
                {
                    IsTileSelected = true;
                    SelectedTileLocation = HoveredTileLocation;
                }
                else
                {
                    IsTileSelected = false;
                    SelectedTileLocation = new Vec2<double>(0.0, 0.0);
                }
                OnPropertyChanged(nameof(IsTileSelected));
                OnPropertyChanged(nameof(SelectedTileLocation));
            }
        }

        private Vec2<double> _selectedTileLocation = new Vec2<double>(0.0);

        public Vec2<double> SelectedTileLocation
        {
            get => _selectedTileLocation;
            set
            {
                _selectedTileLocation = value;
                OnPropertyChanged(nameof(SelectedTileLocation));
            }
        }

        public ICommand LoadTilesetCommand { get; }
        public ICommand HoverTileCommand { get; } 
        public ICommand SelectTileCommand { get; } 

        public TilesetViewModel()
        {
            _currentTileset = null;
            LoadTilesetCommand = new RelayCommand(OnLoadTileset);
            HoverTileCommand = new RelayCommand(p => HoveredTileIndex = p as Vec2<int>);
            SelectTileCommand = new RelayCommand(p => SelectedTileIndex = p as Vec2<int>);
            
        }
        public TilesetViewModel(Tileset currentTileset)
        {
            _currentTileset = currentTileset;
            LoadTilesetCommand = new RelayCommand(OnLoadTileset);
            HoverTileCommand = new RelayCommand(p => HoveredTileIndex = p as Vec2<int>);
            SelectTileCommand = new RelayCommand(p => SelectedTileIndex = p as Vec2<int>);
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

            TileImages.Clear();
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
                        TileImages.Add(tileBmp);
                    }
                }
            }
            catch
            {
                TilesetImage = null;
            }
        }
    }
}
