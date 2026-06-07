using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TileBasedLevelEditor.Models;

namespace TileBasedLevelEditor.Services
{
    public static class TileSelectedService
    {
        public record TilemapPreviewTile(Vec2<int> Index, TileData TileData, CroppedBitmap? Image);
        private static List<TilemapPreviewTile>? _selectedTiles;
        public static List<TilemapPreviewTile>? SelectedTiles
        {
            get => _selectedTiles;
            set
            {
                if (_selectedTiles != value)
                {
                    _selectedTiles = value;
                    SelectedTileChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public static EventHandler? SelectedTileChanged; 
    }
}
