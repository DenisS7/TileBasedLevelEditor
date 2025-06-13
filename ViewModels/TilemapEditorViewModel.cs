using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NotesApp.Commands;
using System.Windows.Input;
using System.Windows.Shapes;
using NotesApp.ViewModel;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Services;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace TileBasedLevelEditor.ViewModels
{
    class TilemapEditorViewModel : ViewModelBase
    {
        private Tilemap _currentTilemap;

        public Tilemap CurrentTilemap
        {
            get => _currentTilemap;
            set
            {
                _currentTilemap = value;
                OnPropertyChanged(nameof(CurrentTilemap));
            }
        }

        private Vec2<int> TilemapSize => CurrentTilemap.TilemapSize;
        public Vec2<int> TileSize => CurrentTilemap.TileSize;

        public TileGridViewModel TileGridVM { get; }

        public TilemapEditorViewModel()
        {
            _currentTilemap = new Tilemap("TestTilemap", new Vec2<int>(32, 32), new Vec2<int>(10, 15));
            TileGridVM = new TileGridViewModel(TileSize, TilemapSize, new Vec2<int>(0, 0), null, null, OnTileSelected, true, false);
        }

        public TilemapEditorViewModel(Tilemap currentTilemap)
        {
            _currentTilemap = currentTilemap;
            TileGridVM = new TileGridViewModel(TileSize, TilemapSize, new Vec2<int>(0, 0), null, null, OnTileSelected, true, false);
        }
        private void OnTileSelected(Vec2<int>? vec)
        {
            if (TileSelectedService.SelectedTiles == null || TileSelectedService.SelectedTiles.Count == 0 || vec == null)
                return;

            TileData referenceTile = TileSelectedService.SelectedTiles[0].Item1;

            foreach (Tuple<TileData, CroppedBitmap?> tileData in TileSelectedService.SelectedTiles)
            {
                Vec2<int> tilemapTileIndex = vec + tileData.Item1.TilesetIndex - referenceTile.TilesetIndex;
                if (tilemapTileIndex < 0 || tilemapTileIndex >= TilemapSize)
                    continue;

                CurrentTilemap.SetTile(tilemapTileIndex, tileData.Item1.TilesetIndex, tileData.Item1.TilesetName);
                TileGridVM.TileImages[tilemapTileIndex.X + tilemapTileIndex.Y * TilemapSize.X] = tileData.Item2;
            }
        }
    }
}
