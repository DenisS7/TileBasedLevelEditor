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
        public ObservableCollection<TileSelectedService.TilemapPreviewTile> PreviewTiles { get; }
        private List<TilemapCellViewModel> _cells;
        public override IReadOnlyList<GridCellViewModel> Cells => _cells;

        public TilemapGridViewModel(ITilemapParent parent, ITilemapRendererService tilemapRendererService, Action<Vec2<int>?>? OnHover = null, Action<Vec2<int>?>? OnSelect = null, bool shouldBeCentered = false, bool gridLinesVisibility = false, bool canHighlightSelectedTile = true) 
            : base(parent.CurrentTilemap.TileSize, parent.CurrentTilemap.TilemapSize, parent.CurrentTilemap.TileMargin, OnHover, OnSelect, shouldBeCentered, gridLinesVisibility, canHighlightSelectedTile)
        {
            PreviewTiles = new ObservableCollection<TileSelectedService.TilemapPreviewTile>();
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

        public void ApplyTile(int tilemapArrayTileIndex, LayerViewModel layer)
        {
            _tilemapRendererService.ApplyTile(_cells[tilemapArrayTileIndex], tilemapArrayTileIndex, layer);
        }

        public void SetNewTilemap(TilemapEditorViewModel tilemapVM)
        {
            _cells = _tilemapRendererService.GetRenderedTilemapCells(tilemapVM);
            OnPropertyChanged(nameof(Cells));
        }

        public void ShowPreview(List<TileSelectedService.TilemapPreviewTile> previewTiles)
        {
            PreviewTiles.Clear();
        }

        protected override void OnTileHovered(Vec2<int>? initTile)
        {
            PreviewTiles.Clear();
            if (initTile == null)
                return;

            if (TileSelectedService.SelectedTiles == null || TileSelectedService.SelectedTiles.Count == 0)
                return;

            TileData referenceTile = TileSelectedService.SelectedTiles[0].TileData;
            
            foreach (TileSelectedService.TilemapPreviewTile previewTile in TileSelectedService.SelectedTiles)
            {
                Vec2<int> tilemapTileIndex = initTile + previewTile.TileData.TilesetIndex - referenceTile.TilesetIndex;
                if (tilemapTileIndex < 0 || tilemapTileIndex >= NrTiles)
                    continue;
                TileSelectedService.TilemapPreviewTile tilemapPreviewTile = previewTile with { Index = tilemapTileIndex };
                PreviewTiles.Add(tilemapPreviewTile);
                //HoveredOverTiles.Add(new Tuple<Vec2<int>, CroppedBitmap?>(tilemapTileIndex, TileGridVM.Cells[tilemapTileIndex.X + tilemapTileIndex.Y * TilemapSize.X]));
                //TileGridVM.Cells[tilemapTileIndex.X + tilemapTileIndex.Y * TilemapSize.X] = tileData.Item2;
            }
        }
    }
}
