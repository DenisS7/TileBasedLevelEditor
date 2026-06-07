using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.ViewModels;

namespace TileBasedLevelEditor.Interfaces
{
    public interface ITilemapLayersParent
    {
        public Tilemap CurrentTilemap { get; }

        public void OnLayerDeleted(LayerViewModel layer);
        public void OnLayerVisibilityChange(LayerViewModel layer);
    }
}
