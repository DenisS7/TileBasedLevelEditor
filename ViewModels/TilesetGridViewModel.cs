using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.ViewModels.Rendering;

namespace TileBasedLevelEditor.ViewModels
{
    public class TilesetGridViewModel : TileGridViewModel
    {
        private List<TilesetCellViewModel> _cells;
        public override IReadOnlyList<GridCellViewModel> Cells => _cells;

        public TilesetGridViewModel(Vec2<int> tileSize, Vec2<int> nrTiles, Vec2<int> tileMargin, List<CroppedBitmap?>? tilesetImages, Action<Vec2<int>?>? OnHover = null, Action<Vec2<int>?>? OnSelect = null, bool shouldBeCentered = false, bool gridLinesVisibility = false, bool canHighlightSelectedTile = true)
            : base(tileSize, nrTiles, tileMargin, OnHover, OnSelect, shouldBeCentered, gridLinesVisibility, canHighlightSelectedTile)
        {
            int arraySize = nrTiles.X * nrTiles.Y;
            _cells = new List<TilesetCellViewModel>();
            if (tilesetImages != null)
            {
                Debug.Assert(tilesetImages.Count() == arraySize);
                for (int i = 0; i < arraySize; i++)
                {
                    _cells.Add(new TilesetCellViewModel(new Vec2<int>(i % nrTiles.Y, i / nrTiles.X), tilesetImages[i]));
                }
            }
            else
            {
                for (int i = 0; i < arraySize; i++)
                {
                    _cells.Add(new TilesetCellViewModel(new Vec2<int>(i % nrTiles.Y, i / nrTiles.X), null));
                }
            }
        }

        public void SetNewTileImages(List<CroppedBitmap?> tileImages)
        {
            if (_cells.Count > tileImages.Count)
                _cells.RemoveRange(tileImages.Count, _cells.Count - tileImages.Count);

            for (int i = 0; i < tileImages.Count(); i++)
            {
                if (i >= _cells.Count)
                    _cells.Add(new TilesetCellViewModel(new Vec2<int>(i % NrTiles.Y, i / NrTiles.X), tileImages[i]));
                else
                    _cells[i].Image = tileImages[i];
            }
        }

        public void ClearCells()
        {
            _cells.Clear();
            OnPropertyChanged(nameof(Cells));
        }

        protected override void OnTileHovered(Vec2<int>? initTile)
        {
        }
    }
}