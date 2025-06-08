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
            TileGridVM = new TileGridViewModel(TileSize, TilemapSize, new Vec2<int>(0, 0), null, true, false);
        }

        public TilemapEditorViewModel(Tilemap currentTilemap)
        {
            _currentTilemap = currentTilemap;
            TileGridVM = new TileGridViewModel(TileSize, TilemapSize, new Vec2<int>(0, 0), null, true, false);
        }
    }
}
