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
        private readonly VisualCollection _children;
        private DrawingVisual[] _cells = Array.Empty<DrawingVisual>();
        private TileGridViewModel? _source = null;
        private Vec2<int> _gridSize => _source?.NrTiles ?? new Vec2<int>(0);
        private Vec2<int> _tileSize => _source?.TileSize ?? 32;
        private Rect _localRect;

        protected override int VisualChildrenCount => _children.Count;

        public GridCanvas()
        {
            _children = new VisualCollection(this);

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
                    tile.PropertyChanged += (s, e) => RedrawCell(cell.Index);
                }
            }
        }

        public void AttachToCell(GridCellViewModel cell)
        {
            if (cell is TilemapCellViewModel tilemapCell)
            {
                tilemapCell.Tiles.CollectionChanged += (s, e) => AttachToTiles(cell, s, e);
                foreach (TilemapCellTileViewModel tile in tilemapCell.Tiles)
                {
                    tile.PropertyChanged += (s, e) => RedrawCell(cell.Index);
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

        public void BuildCanvas(TileGridViewModel source)
        {
            _source = source;
            _localRect = new Rect(0, 0, _tileSize.X, _tileSize.Y);

            _children.Clear();
            _cells = new DrawingVisual[_gridSize.X * _gridSize.Y];

            for (int i = 0; i < _gridSize.Y; i++)
            {
                for (int j = 0; j < _gridSize.X; j++)
                {
                    DrawingVisual dv = new DrawingVisual { Offset = new Vector(j * (_tileSize.X + source.TileMargin.X) , i  * (_tileSize.Y + source.TileMargin.Y)) };
                    RenderOptions.SetBitmapScalingMode(dv, BitmapScalingMode.NearestNeighbor);
                    
                    int cellArrayIndex = i * _gridSize.X + j;
                    RecordCell(dv, source.Cells[cellArrayIndex]);
                    AttachToCell(source.Cells[cellArrayIndex]);
                    _cells[cellArrayIndex] = dv;
                    _children.Add(dv);
                }
            }

            InvalidateMeasure();
        }

        public void RedrawCell(Vec2<int> cellIndex)
        {
            if (_source == null)
                return;

            int cellArrayIndex = cellIndex.Y * _gridSize.X + cellIndex.X;
            RecordCell(_cells[cellArrayIndex], _source.Cells[cellArrayIndex]);
        }

        private void RecordCell(DrawingVisual dv, GridCellViewModel cell)
        {
            using var dc = dv.RenderOpen();

            if (cell is TilemapCellViewModel tilemapCell)
            {
                foreach (TilemapCellTileViewModel tilemapCellTile in tilemapCell.Tiles)
                {
                    if (!tilemapCellTile.LayerVM.Visible)
                        continue;

                    bool applyOpacity = tilemapCellTile.LayerVM.Opacity < 1.0;
                    if (applyOpacity)
                        dc.PushOpacity(tilemapCellTile.LayerVM.Opacity);
                    dc.DrawImage(tilemapCellTile.Image, _localRect);
                    if (applyOpacity)
                        dc.Pop();
                }
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
