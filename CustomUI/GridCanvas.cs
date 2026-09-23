using Newtonsoft.Json.Bson;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
		private readonly ContainerVisual _layersParent = new ContainerVisual();
		private DrawingVisual[] _cells = Array.Empty<DrawingVisual>();
		private TileGridViewModel? _source = null;
		private Vec2<int> _gridNrTiles => _source?.NrTiles ?? 0;
		private Vec2<int> _tileSize => _source?.TileSize ?? 32;
		private Vec2<int> _gridSize => _source?.GridSize ?? 0;
		private Rect _localRect;

		private DrawingVisual gridLineVisual = new DrawingVisual();
		private readonly SolidColorBrush _gridLineBrush = (SolidColorBrush)Application.Current.Resources["ForegroundBrush"];
		private Pen _innerGridLineStroke;
		private readonly double _innerGridLineThickness = 0.3;
		private readonly double _edgeGridLineThickness = 1.0;
		private readonly DoubleCollection _innerGridLineDashes = new DoubleCollection() { 10.0, 10.0 };

		protected override int VisualChildrenCount => _children.Count;

		public GridCanvas()
		{
			_children = new VisualCollection(this);
			_innerGridLineStroke = new Pen(_gridLineBrush, _innerGridLineThickness);
			_innerGridLineStroke.DashStyle = new DashStyle(_innerGridLineDashes, 0);

			DataContextChanged += OnDataContextChanged;
		}
		protected override Visual GetVisualChild(int index)
		{
			return _children[index];
		}
		protected override Size MeasureOverride(Size size)
		{
			return new Size(_gridSize.X, _gridSize.Y);
		}

		protected override void OnRender(DrawingContext dc)
		{
			dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

			if (_source != null && _source.GridLinesVisibility)
			{
				Pen edgeGridLineStroke = new Pen(_gridLineBrush, _edgeGridLineThickness);
				edgeGridLineStroke.Thickness = _edgeGridLineThickness / _source.ScrollViewerZoom;

				edgeGridLineStroke.Freeze();

				double halfEdgeGridLineThickness = edgeGridLineStroke.Thickness / 2.0;

				dc.DrawLine(edgeGridLineStroke, new Point(-edgeGridLineStroke.Thickness, -halfEdgeGridLineThickness), new Point(_gridSize.X + edgeGridLineStroke.Thickness, -halfEdgeGridLineThickness));
				dc.DrawLine(edgeGridLineStroke, new Point(-halfEdgeGridLineThickness, -edgeGridLineStroke.Thickness), new Point(-halfEdgeGridLineThickness, _gridSize.Y + edgeGridLineStroke.Thickness));
				dc.DrawLine(edgeGridLineStroke, new Point(-edgeGridLineStroke.Thickness, _gridSize.Y + halfEdgeGridLineThickness), new Point(_gridSize.X + edgeGridLineStroke.Thickness, _gridSize.Y + halfEdgeGridLineThickness));
				dc.DrawLine(edgeGridLineStroke, new Point(_gridSize.X + halfEdgeGridLineThickness, -edgeGridLineStroke.Thickness), new Point(_gridSize.X + halfEdgeGridLineThickness, _gridSize.Y + edgeGridLineStroke.Thickness));
			} 
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue is TileGridViewModel vm)
			{
				BuildCanvas(vm);
			}    
		}

		private void OnGridCellsChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (_source == null)
				return;

			if (e.PropertyName == nameof(_source.Cells))
			{
				BuildCanvas(_source);
			}
		}

		private void OnGridLinesVisibilityChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (sender is TileGridViewModel vm && e.PropertyName is nameof(_source.GridLinesVisibility))
			{
				CreateGridLines();
			}
		}

		private void OnZoomChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (sender is TileGridViewModel vm && e.PropertyName is nameof(_source.ScrollViewerZoom))
			{
				_innerGridLineStroke.Thickness = _innerGridLineThickness / vm.ScrollViewerZoom;
				InvalidateVisual();
			}
		}

		private void OnLayerCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
				{
					foreach (LayerViewModel layer in e.NewItems!)
					{
						AddNewLayer(layer);
					}
					break;
				}
				case NotifyCollectionChangedAction.Remove:
				{
					foreach (LayerViewModel layer in e.OldItems!)
					{
						RemoveLayer(layer);
					}
					break;
				}
				case NotifyCollectionChangedAction.Move:
				{
					//TO DO: Add rearranging layers logic here or on layer rendering changed
					break;
				}
			}
		}

		private void OnLayerRenderingChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (sender is not LayerViewModel layer)
				return;

			if (e.PropertyName is nameof(layer.Visible) || e.PropertyName is nameof(layer.Opacity))
				Unsafe.As<ContainerVisual>(_layersParent.Children[layer.VisibilityIndex]).Opacity = layer.Visible ? layer.Opacity : 0.0;
			//else if (e.PropertyName is nameof(layer.VisibilityIndex))
			//{
			//
			//}
		}

		private void CreateGridLines()
		{
			if (_source == null)
				return;

			_source.PropertyChanged -= OnZoomChanged;
			using (gridLineVisual.RenderOpen()) { };
			InvalidateVisual();

			if (!_source.GridLinesVisibility)
				return;

			_innerGridLineStroke.Thickness = _innerGridLineThickness / _source.ScrollViewerZoom;
			_innerGridLineStroke.DashStyle = new DashStyle(_innerGridLineDashes, 0);

			GeometryGroup horizontalTileGridLine = new GeometryGroup();
			horizontalTileGridLine.Children.Add(new LineGeometry(new Point(0, 0), new Point(_gridSize.X, 0)));
			
			GeometryGroup verticalTileGridLine = new GeometryGroup();
			verticalTileGridLine.Children.Add(new LineGeometry(new Point(0, 0), new Point(0, _gridSize.Y)));

			GeometryDrawing horizontalGeometryDrawing = new GeometryDrawing();
			horizontalGeometryDrawing.Geometry = horizontalTileGridLine;
			horizontalGeometryDrawing.Pen = _innerGridLineStroke;
			
			GeometryDrawing verticalGeometryDrawing = new GeometryDrawing();
			verticalGeometryDrawing.Geometry = verticalTileGridLine;
			verticalGeometryDrawing.Pen = _innerGridLineStroke;

			DrawingBrush horizotalBrush = new DrawingBrush(horizontalGeometryDrawing);
			horizotalBrush.TileMode = TileMode.Tile;
			horizotalBrush.Viewport = new Rect(0, 0, _gridSize.X, _tileSize.Y);
			horizotalBrush.Viewbox = new Rect(new Point(0, 0), new Point(_gridSize.X, _tileSize.Y));
			horizotalBrush.ViewportUnits = BrushMappingMode.Absolute;
			horizotalBrush.ViewboxUnits = BrushMappingMode.Absolute;

			DrawingBrush verticalBrush = new DrawingBrush(verticalGeometryDrawing);
			verticalBrush.TileMode = TileMode.Tile;
			verticalBrush.Viewport = new Rect(0, 0, _tileSize.X, _gridSize.Y);
			verticalBrush.Viewbox = new Rect(new Point(0, 0), new Point(_tileSize.X, _gridSize.Y));
			verticalBrush.ViewportUnits = BrushMappingMode.Absolute;
			verticalBrush.ViewboxUnits = BrushMappingMode.Absolute;

			RenderOptions.SetBitmapScalingMode(gridLineVisual, BitmapScalingMode.Linear);
			RenderOptions.SetEdgeMode(gridLineVisual, EdgeMode.Unspecified);

			using DrawingContext dc = gridLineVisual.RenderOpen();
			dc.DrawRectangle(horizotalBrush, null, new Rect(new Point(0, _tileSize.Y), new Size(_gridSize.X, _gridSize.Y - _tileSize.Y - 1)));
			dc.DrawRectangle(verticalBrush, null, new Rect(new Point(_tileSize.X, 0), new Size(_gridSize.X - _tileSize.X - 1, _gridSize.Y)));

			Pen edgeGridLineStroke = new Pen(_gridLineBrush, _edgeGridLineThickness);
			edgeGridLineStroke.Thickness = _edgeGridLineThickness / _source?.ScrollViewerZoom ?? 1.0;

			double halfEdgeGridLineThickness = _edgeGridLineThickness / 2.0;

			_source.PropertyChanged += OnZoomChanged;
			
			_children.Add(gridLineVisual);
		}

		private void ClearCanvas()
		{
			if (_source != null)
			{
				_source.PropertyChanged -= OnZoomChanged;
				_source.PropertyChanged -= OnGridLinesVisibilityChanged;
				_source.PropertyChanged -= OnGridCellsChanged;

				if (_source is TilemapGridViewModel tilemap)
				{
					for (int i = 0; i < tilemap.Layers.Count; i++)
					{
						tilemap.Layers[i].PropertyChanged -= OnLayerRenderingChanged;
					}

					tilemap.OnCellChanged -= RedrawCell;
				}
				_source = null;
			}

			_children.Clear();
			_layersParent.Children.Clear();

			InvalidateVisual();
		}

		private ContainerVisual BuildLayerContainer(LayerViewModel layer)
		{
			ContainerVisual layerContainer = new ContainerVisual { Opacity = layer.Visible ? layer.Opacity : 0.0 };
			layer.PropertyChanged += OnLayerRenderingChanged;
			return layerContainer;
		}

		private void RemoveLayer(LayerViewModel layer)
		{
			if (_source == null)
				return;

			layer.PropertyChanged -= OnLayerRenderingChanged;
			_layersParent.Children.RemoveAt(layer.VisibilityIndex);
		}

		private void AddNewLayer(LayerViewModel layer)
		{
			if (_source == null)
				return;

			ContainerVisual layerContainer = BuildLayerContainer(layer);

			for (int i = 0; i < _gridNrTiles.Y; i++)
			{
				for (int j = 0; j < _gridNrTiles.X; j++)
				{
					int cellArrayIndex = i * _gridNrTiles.X + j;
					TilemapCellViewModel cell = (TilemapCellViewModel)_source.Cells[cellArrayIndex];

					DrawingVisual dv = new DrawingVisual { Offset = new Vector(j * (_tileSize.X + _source.TileMargin.X), i * (_tileSize.Y + _source.TileMargin.Y)) };
					RenderOptions.SetBitmapScalingMode(dv, BitmapScalingMode.NearestNeighbor);

					RecordTile(dv, _source.Cells[cellArrayIndex], layer.VisibilityIndex);
					layerContainer.Children.Add(dv);
				}
			}
			_layersParent.Children.Add(layerContainer);
		}

		private void BuildCanvas(TileGridViewModel source)
		{
			ClearCanvas();

			_source = source;
			_localRect = new Rect(0, 0, _tileSize.X, _tileSize.Y);

			if (source is TilemapGridViewModel tilemap)
			{
				foreach (LayerViewModel layer in tilemap.Layers)
				{
					AddNewLayer(layer);   
				}

				tilemap.OnCellChanged += RedrawCell;
				tilemap.Layers.CollectionChanged += OnLayerCollectionChanged;
				_children.Add(_layersParent);
			}
			else if (source is TilesetGridViewModel tileset)
			{
				_cells = new DrawingVisual[_gridNrTiles.X * _gridNrTiles.Y];
				for (int i = 0; i < _gridNrTiles.Y; i++)
				{
					for (int j = 0; j < _gridNrTiles.X; j++)
					{
						DrawingVisual dv = new DrawingVisual { Offset = new Vector(j * (_tileSize.X + source.TileMargin.X), i * (_tileSize.Y + source.TileMargin.Y)) };
						RenderOptions.SetBitmapScalingMode(dv, BitmapScalingMode.NearestNeighbor);

						int cellArrayIndex = i * _gridNrTiles.X + j;
						RecordTile(dv, source.Cells[cellArrayIndex]);
						_cells[cellArrayIndex] = dv;
						_children.Add(dv);
					}
				}
			}    
			else
			{
				throw new NotImplementedException("Grid canvas source type not implemented!");
			}

			_source.PropertyChanged += OnGridCellsChanged;
			_source.PropertyChanged += OnGridLinesVisibilityChanged;

			CreateGridLines();
			InvalidateMeasure();
		}

		private DrawingVisual GetCellFromLayer(int cellArrayIndex, int layerIndex)
		{
			return Unsafe.As<DrawingVisual>(Unsafe.As<ContainerVisual>(_layersParent.Children[layerIndex]).Children[cellArrayIndex]);
		}

		private void RedrawCell(int cellArrayIndex, int visibilityIndex)
		{
			if (_source == null)
				return;

			RecordTile(GetCellFromLayer(cellArrayIndex, visibilityIndex), _source.Cells[cellArrayIndex], visibilityIndex);
		}

		private void RecordTile(DrawingVisual dv, GridCellViewModel cell, int visibilityIndex = -1)
		{
			using DrawingContext dc = dv.RenderOpen();

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
