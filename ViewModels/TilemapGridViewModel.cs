using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TileBasedLevelEditor.Interfaces;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Services;
using TileBasedLevelEditor.ViewModels.Rendering;

namespace TileBasedLevelEditor.ViewModels
{
	public class TilemapGridViewModel : TileGridViewModel
	{
		private ITilemapRendererService _tilemapRendererService;
		private ITilemapParent _parent;
		
		public ObservableCollection<LayerViewModel> Layers => _parent.Layers;
		private List<TilemapCellViewModel> _cells;
		public override IReadOnlyList<GridCellViewModel> Cells => _cells;

		public event Action<int, int>? OnCellChanged;

		public TilemapGridViewModel(ITilemapParent parent, ITilemapRendererService tilemapRendererService, Action<Vec2<int>?>? OnHover = null, Action<Vec2<int>?>? OnSelect = null, bool shouldBeCentered = false, bool gridLinesVisibility = false, bool canHighlightSelectedTile = true) 
			: base(parent.CurrentTilemap.TileSize, parent.CurrentTilemap.TilemapSize, parent.CurrentTilemap.TileMargin, OnHover, OnSelect, shouldBeCentered, gridLinesVisibility, canHighlightSelectedTile)
		{
			_tilemapRendererService = tilemapRendererService;
			_parent = parent;

			int arraySize = NrTiles.X * NrTiles.Y;
			_cells = new List<TilemapCellViewModel>();
			for (int i = 0; i < arraySize; i++)
				_cells.Add(new TilemapCellViewModel(new Vec2<int>(i % NrTiles.X, i / NrTiles.X)));
		}

		public void RemoveLayer(LayerViewModel layer)
		{
			_tilemapRendererService.RemoveLayer(_cells, layer);
		}

		public void SwapLayers(LayerViewModel layer1, LayerViewModel layer2)
		{
			_tilemapRendererService.SwapLayers(_cells, layer1, layer2);
		}

		public void ApplyTile(int tilemapArrayTileIndex, LayerViewModel layer)
		{
			_tilemapRendererService.ApplyTile(_cells[tilemapArrayTileIndex], tilemapArrayTileIndex, layer);
			OnCellChanged?.Invoke(tilemapArrayTileIndex, layer.VisibilityIndex);
		}

		public void SetNewTilemap(TilemapEditorViewModel tilemapVM)
		{
			_cells = _tilemapRendererService.GetRenderedTilemapCells(tilemapVM);
			OnPropertyChanged(nameof(Cells));
		}

		protected override void OnTileHovered(Vec2<int>? initTile)
		{
			//TO DO: find a better way to clear tiles
			for (int i = 0; i < Layers.Last().Tiles.Length; i++)
			{
				if (Layers.Last().Tiles[i].TilesetID == Guid.Empty)
					continue;

				Layers.Last().Tiles[i] = new TileData();
				ApplyTile(i, Layers.Last());
			}

			if (initTile == null)
				return;

			if (TileSelectedService.SelectedTiles == null || TileSelectedService.SelectedTiles.Count == 0)
				return;

			if (TileSelectedService.ReferenceTile == null)
				return;
			
			foreach (TileSelectedService.TilemapPreviewTile previewTile in TileSelectedService.SelectedTiles)
			{
				Vec2<int> tilemapTileIndex = initTile + previewTile.TileData.TilesetIndex - TileSelectedService.ReferenceTile;
				if (tilemapTileIndex < 0 || tilemapTileIndex >= NrTiles)
					continue;

				int tilemapArrayTileIndex = tilemapTileIndex.X + tilemapTileIndex.Y * NrTiles.X;
				Layers.Last().Tiles[tilemapArrayTileIndex] = previewTile.TileData;
				ApplyTile(tilemapArrayTileIndex, Layers.Last());
			}
		}
	}
}
