using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.ViewModels;
using TileBasedLevelEditor.ViewModels.Rendering;

namespace TileBasedLevelEditor.Interfaces
{
    public interface ITilemapRendererService
    {
        public List<TilemapCellViewModel> GetRenderedTilemapCells(TilemapEditorViewModel tilemap);

        public void ApplyTile(TilemapCellViewModel cell, int tileIndex, LayerViewModel layer);
        public void EraseTile(TilemapCellViewModel cell, LayerViewModel layer);
        public void RemoveLayer(List<TilemapCellViewModel> cells, LayerViewModel layer);
    }
}
