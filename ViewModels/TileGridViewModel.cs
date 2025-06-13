using NotesApp.Commands;
using NotesApp.ViewModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TileBasedLevelEditor.CustomArgs;
using TileBasedLevelEditor.Misc;
using TileBasedLevelEditor.Models;

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

        public Vec2<int> GridSize => TileSize * NrTiles;

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

        public record LineInfo(double X1, double Y1, double X2, double Y2, bool IsEdge);

        public ObservableCollection<LineInfo> GridLines { get; private set; } = [];

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

        private bool _canHighlightSelectedTile;

        public bool CanHighlightSelectedTile
        {
            get => _canHighlightSelectedTile;
            set
            {
                _canHighlightSelectedTile = value;
                OnPropertyChanged(nameof(CanHighlightSelectedTile));
            }
        }

        public bool IsTileSelected => SelectionAreas.Count > 0;

        private Vec2<int>? _initialSelectedTile = new Vec2<int>(-1);

        public Vec2<int>? InitialSelectedTile
        {
            get => _initialSelectedTile;
            set
            {
                _initialSelectedTile = value;
                OnPropertyChanged(nameof(InitialSelectedTile));
                OnPropertyChanged(nameof(IsTileSelected));            
            }
        }

        public List<SelectionArea> SelectionAreas { get; private set; } = [];
        public Geometry SelectionGeometry
        {
            get
            {
                Geometry? combined = null;
                Vec2<int> TileSizeMargin = TileSize + TileMargin;

                foreach (SelectionArea area in SelectionAreas)
                {
                    RectangleGeometry rectGeom = new RectangleGeometry(area.RectArea);
                    if(combined == null)
                        combined = rectGeom;
                    else
                    {
                        combined = Geometry.Combine(
                            combined,
                            rectGeom,
                            GeometryCombineMode.Union,
                            null);
                    }
                }

                return combined ?? Geometry.Empty;
            }
        }

        public ICommand HoverTileCommand { get; }
        public ICommand SelectTileCommand { get; }

        public TileGridViewModel(Vec2<int> tileSize, Vec2<int> nrTiles, Vec2<int> tileMargin, List<CroppedBitmap?>? tileImages = null, Action<Vec2<int>>? OnHover = null, Action<Vec2<int>>? OnSelect = null, bool gridLinesVisibility = false, bool canHighlightSelectedTile = true)
        {
            _tileSize = tileSize;
            _nrTiles = nrTiles;
            _tileMargin = tileMargin;
            _gridLinesVisibility = gridLinesVisibility;
            _canHighlightSelectedTile = canHighlightSelectedTile;

            HoverTileCommand = new RelayCommand(p => 
            {
                if (p == null || p is not Vec2<int> Index)
                {
                    HoveredTileIndex = null;
                    return;
                }

                HoveredTileIndex = Index;
                OnHover?.Invoke(HoveredTileIndex);
            });

            SelectTileCommand = new RelayCommand(p =>
            {
                if (p is not TileSelectionArgs args)
                    return;

                if (args.CurrentDragStage == DragStage.Start)
                {
                    if (!args.Add)
                        SelectionAreas.Clear();

                    InitialSelectedTile = args.Index;
                    if (args.Index != null)
                        SelectionAreas.Add(GetSelectionArea(args.Index, args.Index));
                }
                else if (args.Index != null && InitialSelectedTile != null)
                {                    
                    Debug.Assert(SelectionAreas.Count > 0);
                    
                    SelectionArea LastArea = SelectionAreas.Last();
                    LastArea = GetSelectionArea(InitialSelectedTile, args.Index);

                    if (!args.Add)
                    { 
                        SelectionAreas.Clear(); 
                        SelectionAreas.Add(LastArea);
                    }
                    else if (SelectionAreas.Count > 0)
                    {
                        SelectionAreas[SelectionAreas.Count - 1] = LastArea;
                    }
                }
                OnPropertyChanged(nameof(SelectionGeometry));
            });

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

            for (int i = 0; i <= NrTiles.X; i++)
            {
                double x = i * TileSize.X;
                if (i == 0)
                    x += 1.0;

                bool isEdge = i == 0 || i == NrTiles.X;
                GridLines.Add(new LineInfo(x, 0, x, GridSize.Y, isEdge));
            }

            for (int i = 0; i <= NrTiles.Y; i++)
            {
                double y = i * TileSize.Y;
                if (i == 0)
                    y += 1.0;

                bool isEdge = i == 0 || i == NrTiles.Y;
                GridLines.Add(new LineInfo(0, y, GridSize.X, y, isEdge));
            }
        }

        private SelectionArea GetSelectionArea(Vec2<int> PointA, Vec2<int> PointB)
        {
            Vec2<int> StartTile = new Vec2<int>(-1);
            Vec2<int> EndTile = new Vec2<int>(-1);

            if(PointA.X > PointB.X)
            {
                StartTile.X = PointB.X;
                EndTile.X = PointA.X;
            }
            else
            {
                StartTile.X = PointA.X;
                EndTile.X = PointB.X;
            }

            if(PointA.Y > PointB.Y)
            {
                StartTile.Y = PointB.Y;
                EndTile.Y = PointA.Y;
            }
            else
            {
                StartTile.Y = PointA.Y;
                EndTile.Y = PointB.Y;
            }

            return new SelectionArea(StartTile, EndTile, TileSize, TileMargin);
        }
    }
}
