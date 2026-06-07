using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.ViewModel;
using TileBasedLevelEditor.ViewModels;

namespace TileBasedLevelEditor.ViewModels.Rendering
{
    public class TilemapCellTileViewModel : ViewModelBase
    {
        private TileData _tile;
        public TileData Tile
        {
            get => _tile;
            set 
            {
                _tile = value;
                OnPropertyChanged(nameof(Tile));
            }
        }

        private CroppedBitmap? _image;
        public CroppedBitmap? Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }

        private LayerViewModel _layerVM;
        public LayerViewModel LayerVM
        {
            get => _layerVM;
            set
            {
                _layerVM = value;
                OnPropertyChanged(nameof(LayerVM));
            }
        }

        public TilemapCellTileViewModel(TileData tile, CroppedBitmap? image, LayerViewModel layerVM)
        {
            _tile = Tile;
            _image = image;
            _layerVM = layerVM;
        }
    }
}
