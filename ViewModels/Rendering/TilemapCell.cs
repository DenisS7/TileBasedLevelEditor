using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedLevelEditor.Models;

namespace TileBasedLevelEditor.ViewModels.Rendering
{
    public class TilemapCellViewModel : GridCellViewModel
    {
        public ObservableCollection<TilemapCellTileViewModel> Tiles { get; set; }

        public TilemapCellViewModel(Vec2<int> index) 
            : base(index)
        {
            Tiles = new ObservableCollection<TilemapCellTileViewModel>();
        }
    }
}
