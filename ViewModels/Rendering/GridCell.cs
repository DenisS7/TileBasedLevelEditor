using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.ViewModel;

namespace TileBasedLevelEditor.ViewModels.Rendering
{
    public class GridCellViewModel : ViewModelBase
    {
        private Vec2<int> _index;
        public Vec2<int> Index
        {
            get => _index;
            set
            {
                _index = value;
                OnPropertyChanged(nameof(Index));
            }
        }

        public GridCellViewModel(Vec2<int> index)
        {
            _index = index;
        }
    }
}
