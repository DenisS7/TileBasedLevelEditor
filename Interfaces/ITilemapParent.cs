using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.ViewModels;

namespace TileBasedLevelEditor.Interfaces
{
    public interface ITilemapParent
    {
        public Tilemap CurrentTilemap { get; }
        public ObservableCollection<LayerViewModel> Layers { get; }
        public void OnLayerDeleted(LayerViewModel layer);
        public void OnLayersSwapped(int layer1, int layer2);
    }
}
