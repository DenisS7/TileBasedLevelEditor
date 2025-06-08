using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using NotesApp.ViewModel;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Views;
using NotesApp.Commands;

namespace TileBasedLevelEditor.ViewModels
{
    class TileGridViewModel : ViewModelBase
    {
        private Vec2<int> _tileSize;
        public Vec2<int> TileSize
        {
            get => _tileSize;
            set
            {
                _tileSize = value;
                OnPropertyChanged(nameof(TileSize));
            }
        }

        private Vec2<int> _nrTiles;

        public Vec2<int> NrTiles
        {
            get => _nrTiles;
            set
            {
                _nrTiles = value;
                OnPropertyChanged(nameof(NrTiles));
            }
        }

        //Right & Bottom should always be 0
        public Thickness ThicknessTileMargin => new Thickness(_tileMargin.X, _tileMargin.Y, 0, 0);

        private Vec2<int> _tileMargin;

        public Vec2<int> TileMargin
        {
            get => _tileMargin;
            set
            {
                _tileMargin = value;
                OnPropertyChanged(nameof(TileMargin));
                OnPropertyChanged(nameof(ThicknessTileMargin));
            }
        }

        public ObservableCollection<CroppedBitmap?> TileImages { get; private set; }

        private bool _gridLinesVisibility;

        public bool GridLinesVisibility
        {
            get => _gridLinesVisibility;
            set
            {
                _gridLinesVisibility = value;
                OnPropertyChanged(nameof(GridLinesVisibility));
            }
        }
        public ObservableCollection<Line> GridLines { get; private set; } = [];

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
                    HoveredTileLocation = new Vec2<double>(_hoveredTileIndex.X * (TileSize.X + TileMargin.X) + TileMargin.X, _hoveredTileIndex.Y * (TileSize.Y + TileMargin.Y) + TileMargin.Y);
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

        public ICommand HoverTileCommand { get; }
        public ICommand SelectTileCommand { get; }

        public TileGridViewModel(Vec2<int> tileSize, Vec2<int> nrTiles, Vec2<int> tileMargin, List<CroppedBitmap?>? tileImages = null, bool gridLinesVisibility = false)
        {
            _tileSize = tileSize;
            _nrTiles = nrTiles;
            _tileMargin = tileMargin;
            _gridLinesVisibility = gridLinesVisibility;
            HoverTileCommand = new RelayCommand(p => HoveredTileIndex = p as Vec2<int>);
            SelectTileCommand = new RelayCommand(p => SelectedTileIndex = p as Vec2<int>);
            if (tileImages != null)
                TileImages = new ObservableCollection<CroppedBitmap?>(tileImages); 
            else
            {
                TileImages = new ObservableCollection<CroppedBitmap?>();
                for (int i = 0; i < NrTiles.X; i++)
                {
                    for (int j = 0; j < NrTiles.Y; j++)
                    {
                        TileImages.Add(null);
                    }
                }
            }

            GenerateGridLines();
        }

        private void GenerateGridLines()
        {
            GridLines.Clear();

            Vec2<double> tileGridMax = new Vec2<double>(NrTiles.X * TileSize.X, NrTiles.Y * TileSize.Y);

            for (int i = 0; i <= NrTiles.X; i++)
            {
                double x = i * TileSize.X;
                if (i == 0)
                    x += 1.0;
                Line line = new Line()
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = tileGridMax.Y,
                    Stroke = Brushes.White,
                    StrokeThickness = 1
                };

                GridLines.Add(line);
            }

            for (int i = 0; i <= NrTiles.Y; i++)
            {
                double y = i * TileSize.Y;
                if (i == 0)
                    y += 1.0;
                Line line = new Line()
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = tileGridMax.X,
                    Y2 = y,
                    Stroke = Brushes.White,
                    StrokeThickness = 1
                };

                GridLines.Add(line);
            }
        }
    }
}
