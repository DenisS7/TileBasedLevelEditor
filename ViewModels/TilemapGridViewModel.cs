using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedLevelEditor.Interfaces;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.ViewModels.Rendering;

namespace TileBasedLevelEditor.ViewModels
{
    public class TilemapGridViewModel : TileGridViewModel
    {
        private List<TilemapCellViewModel> _cells;
        public override IReadOnlyList<GridCellViewModel> Cells => _cells;

        ITilemapRendererService _tilemapRendererService;
        public TilemapGridViewModel(Vec2<int> tileSize, Vec2<int> nrTiles, Vec2<int> tileMargin, ITilemapRendererService tilemapRendererService, Action<Vec2<int>?>? OnHover = null, Action<Vec2<int>?>? OnSelect = null, bool shouldBeCentered = false, bool gridLinesVisibility = false, bool canHighlightSelectedTile = true) 
            : base(tileSize, nrTiles, tileMargin, OnHover, OnSelect, shouldBeCentered, gridLinesVisibility, canHighlightSelectedTile)
        {
            _tilemapRendererService = tilemapRendererService;

            int arraySize = nrTiles.X * nrTiles.Y;
            _cells = new List<TilemapCellViewModel>();
            for (int i = 0; i < arraySize; i++)
                _cells.Add(new TilemapCellViewModel(new Vec2<int>(i % nrTiles.Y, i / nrTiles.X)));
        }

        public void RemoveLayer(LayerViewModel layer)
        {
            _tilemapRendererService.RemoveLayer(_cells, layer);
        }

        public void ApplyTile(int tilemapArrayTileIndex, LayerViewModel layer)
        {
            _tilemapRendererService.ApplyTile(_cells[tilemapArrayTileIndex], tilemapArrayTileIndex, layer);
        }

        public void SetNewTilemap(TilemapEditorViewModel tilemapVM)
        {
            _cells = _tilemapRendererService.GetRenderedTilemapCells(tilemapVM);
            OnPropertyChanged(nameof(Cells));
        }
    }
}
