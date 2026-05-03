using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedLevelEditor.Models;

namespace TileBasedLevelEditor.Interfaces
{
    public interface ITilemapLayersParent
    {
        public Tilemap CurrentTilemap { get; }

        public void OnLayerDeleted(Layer layer);
        public void OnLayerVisibilityChange(Layer layer);
    }
}
