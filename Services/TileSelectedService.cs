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
        private static TileData? _selectedTile;
        public static TileData? SelectedTile
        {
            get => _selectedTile;
            set
            {
                if (_selectedTile != value)
                {
                    _selectedTile = value;
                    SelectedTileChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        private static CroppedBitmap? _selectedTileImage;
        public static CroppedBitmap? SelectedTileImage
        {
            get => _selectedTileImage;
            set
            {
                if (_selectedTileImage != value)
                {
                    _selectedTileImage = value;
                }
            }
        }

        public static EventHandler? SelectedTileChanged; 
    }
}
