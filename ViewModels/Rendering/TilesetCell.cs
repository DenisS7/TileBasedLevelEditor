using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TileBasedLevelEditor.Models;

namespace TileBasedLevelEditor.ViewModels.Rendering
{
    public class TilesetCellViewModel : GridCellViewModel
    {
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

        public TilesetCellViewModel(Vec2<int> index, CroppedBitmap? image)
            : base(index)
        {
            Image = image;
        }
    }
}
