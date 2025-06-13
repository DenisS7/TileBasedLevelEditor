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
        private static List<Tuple<TileData, CroppedBitmap?>>? _selectedTiles;
        public static List<Tuple<TileData, CroppedBitmap?>>? SelectedTiles
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
