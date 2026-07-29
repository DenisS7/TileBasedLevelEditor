using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.ViewModels;
using TileBasedLevelEditor.ViewModels.Rendering;

namespace TileBasedLevelEditor.CustomUI
{
    public class GridCanvas : FrameworkElement
    {
        //private readonly VisualCollection _layers;
        private readonly VisualCollection _children;
        private readonly List<ContainerVisual> _layers;
        private DrawingVisual[] _cells = Array.Empty<DrawingVisual>();
        private TileGridViewModel? _source = null;
        private Vec2<int> _gridSize => _source?.NrTiles ?? new Vec2<int>(0);
        private Vec2<int> _tileSize => _source?.TileSize ?? 32;
        private Rect _localRect;

        protected override int VisualChildrenCount => _children.Count;

        public GridCanvas()
        {
            _children = new VisualCollection(this);
            _layers = new List<ContainerVisual>();

            DataContextChanged += OnDataContextChanged;
        }
        protected override Visual GetVisualChild(int index)
        {
            return _children[index];
        }
        protected override Size MeasureOverride(Size size)
        {
            return new Size(_tileSize.X * _gridSize.X, _tileSize.Y * _gridSize.Y);
        }

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is TileGridViewModel vm)
            {
                BuildCanvas(vm);
            }    
        }

        public void AttachToTiles(GridCellViewModel cell, object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems == null)
                return;

            foreach (var newItem in args.NewItems)
            {
                if (newItem is TilemapCellTileViewModel tile)
                {
                    tile.PropertyChanged += (s, e) => RedrawCell(cell.Index, tile.LayerVM.VisibilityIndex);
                }
            }
        }

        public void AttachToCell(GridCellViewModel cell)
        {
            if (cell is TilemapCellViewModel tilemapCell)
            {
                tilemapCell.Tiles.CollectionChanged += (s, e) => AttachToTiles(cell, s, e);
                for (int i = 0; i < tilemapCell.Tiles.Count; i++)
                {
                    tilemapCell.Tiles[i].PropertyChanged += (s, e) => RedrawCell(cell.Index, tilemapCell.Tiles[i].LayerVM.VisibilityIndex);
                }
            }
            else if (cell is TilesetCellViewModel tilesetCell)
            {
                //TO DO implement tileset changing
            }
            else
            {
                throw new NotImplementedException("Not implemented for cell type");
            }
        }

        private ContainerVisual BuildLayerContainer(LayerViewModel layer)
        {
            ContainerVisual layerContainer = new ContainerVisual { Opacity = layer.Visible ? layer.Opacity : 0.0 };
            layer.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName is nameof(layer.Visible) || e.PropertyName is nameof(layer.Opacity))
                    layerContainer.Opacity = layer.Visible ? layer.Opacity : 0.0;
            };
            return layerContainer;
        }

        public void BuildCanvas(TileGridViewModel source)
        {
            _source = source;
            _localRect = new Rect(0, 0, _tileSize.X, _tileSize.Y);

            _children.Clear();

            if (source is TilemapGridViewModel tilemap)
            {
                foreach (LayerViewModel layer in tilemap.Layers)
                {
                    ContainerVisual layerContainer = BuildLayerContainer(layer);

                    for (int i = 0; i < _gridSize.Y; i++)
                    {
                        for (int j = 0; j < _gridSize.X; j++)
                        {
                            int cellArrayIndex = i * _gridSize.X + j;
                            TilemapCellViewModel cell = (TilemapCellViewModel)source.Cells[cellArrayIndex];
                            
                            DrawingVisual dv = new DrawingVisual { Offset = new Vector(j * (_tileSize.X + source.TileMargin.X), i * (_tileSize.Y + source.TileMargin.Y)) };
                            RenderOptions.SetBitmapScalingMode(dv, BitmapScalingMode.NearestNeighbor);

                            RecordTile(dv, source.Cells[cellArrayIndex], layer.VisibilityIndex);
                            AttachToCell(source.Cells[cellArrayIndex]);
                            layerContainer.Children.Add(dv);

                        }
                    }

                    _layers.Add(layerContainer);
                    _children.Add(layerContainer);
                }
            }
            else if (source is TilesetGridViewModel tileset)
            {
                _cells = new DrawingVisual[_gridSize.X * _gridSize.Y];
                for (int i = 0; i < _gridSize.Y; i++)
                {
                    for (int j = 0; j < _gridSize.X; j++)
                    {
                        DrawingVisual dv = new DrawingVisual { Offset = new Vector(j * (_tileSize.X + source.TileMargin.X), i * (_tileSize.Y + source.TileMargin.Y)) };
                        RenderOptions.SetBitmapScalingMode(dv, BitmapScalingMode.NearestNeighbor);

                        int cellArrayIndex = i * _gridSize.X + j;
                        RecordTile(dv, source.Cells[cellArrayIndex]);
                        AttachToCell(source.Cells[cellArrayIndex]);
                        _cells[cellArrayIndex] = dv;
                        _children.Add(dv);
                    }
                }
            }    
            else
            {
                throw new NotImplementedException("Grid canvas source type not implemented!");
            }

            InvalidateMeasure();
        }

        public void RedrawCell(Vec2<int> cellIndex, int visibilityIndex = -1)
        {
            if (_source == null)
                return;

            int cellArrayIndex = cellIndex.Y * _gridSize.X + cellIndex.X;
            RecordTile((DrawingVisual)_layers[visibilityIndex].Children[cellArrayIndex], _source.Cells[cellArrayIndex], visibilityIndex);
        }

        private void RecordTile(DrawingVisual dv, GridCellViewModel cell, int visibilityIndex = -1)
        {
            using var dc = dv.RenderOpen();

            if (cell is TilemapCellViewModel tilemapCell)
            {
                IEnumerable<TilemapCellTileViewModel> tiles = tilemapCell.Tiles.Where(tile => tile.LayerVM.VisibilityIndex == visibilityIndex);
                if (tiles.Count() == 0)
                    return;

                dc.DrawImage(tiles.ElementAt(0).Image, _localRect);
            }
            else if (cell is TilesetCellViewModel tilesetCell)
            {
                dc.DrawImage(tilesetCell.Image, _localRect);
            }
            else
            {
                throw new NotImplementedException("Not implemented for cell type");
            }
        }
    }
}
