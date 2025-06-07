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

        public ObservableCollection<Line> GridLines { get; } = [];

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
                    HoveredTileLocation = new Vec2<double>(_hoveredTileIndex.X * TileSize.X, _hoveredTileIndex.Y * TileSize.Y);
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

        public ICommand LoadTilesetCommand { get; }
        public ICommand HoverTileCommand { get; } 

        public TilesetViewModel()
        {
            _currentTileset = null;
            LoadTilesetCommand = new RelayCommand(OnLoadTileset);
            HoverTileCommand = new RelayCommand(p => HoveredTileIndex = p as Vec2<int>);
            
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

            string name = System.IO.Path.GetFileNameWithoutExtension(path);

            //hardcoded size for now
            Vec2<int> tileSize = new Vec2<int>(32, 32);

            try
            {
                CurrentTileset = new Tileset(name, tileSize, path);
                GenerateGridLines();
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

        private void GenerateGridLines()
        {
            GridLines.Clear();

            if (CurrentTileset == null)
                return;

            int columns = CurrentTileset.ImageSize.X / CurrentTileset.TileSize.X;
            int rows = CurrentTileset.ImageSize.Y / CurrentTileset.TileSize.Y;

            for (int i = 0; i <= columns; i++)
            {
                double x = i * CurrentTileset.TileSize.X;
                if (i == 0)
                    x += 1.0;
                Line line = new Line()
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = CurrentTileset.ImageSize.Y,
                    Stroke = Brushes.White,
                    StrokeThickness = 1
                };

                GridLines.Add(line);
            }

            for (int i = 0; i <= rows; i++)
            {
                double y = i * CurrentTileset.TileSize.Y;
                if (i == 0)
                    y += 1.0;
                Line line = new Line()
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = CurrentTileset.ImageSize.X,
                    Y2 = y,
                    Stroke = Brushes.White,
                    StrokeThickness = 1
                };

                GridLines.Add(line);
            }
        }
    }
}
