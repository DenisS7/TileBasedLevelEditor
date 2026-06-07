using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TileBasedLevelEditor.Interfaces;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.ViewModels;
using TileBasedLevelEditor.ViewModels.Rendering;

namespace TileBasedLevelEditor.Services
{
    public class TilemapRendererService : ITilemapRendererService
    {
        private readonly ITilesetsService _tilesetsService;
        public TilemapRendererService(ITilesetsService tilesetsService)
        {
            _tilesetsService = tilesetsService;
        }

        public List<TilemapCellViewModel> GetRenderedTilemapCells(TilemapEditorViewModel tilemapVM)
        {
            List<TilemapCellViewModel> result = new List<TilemapCellViewModel>();

            for (int i = 0; i < tilemapVM.TilemapSize.X * tilemapVM.TilemapSize.Y; i++)
            {
                TilemapCellViewModel cell = new TilemapCellViewModel(new Vec2<int>(i % tilemapVM.TilemapSize.Y, i / tilemapVM.TilemapSize.X));
                for (int j = 0; j < tilemapVM.Layers.Count; j++)
                {
                    TileData tile = tilemapVM.Layers[j].Tiles[i];
                    if (tile.TilesetID == Guid.Empty)
                        continue;

                    cell.Tiles.Add(new TilemapCellTileViewModel(tile, _tilesetsService.GetTileImageAt(tile.TilesetID, tile.TilesetIndex), tilemapVM.Layers[j]));
                }
                result.Add(cell);
            }

            return result;
        }

        public void ApplyTile(TilemapCellViewModel cell, int tileIndex, LayerViewModel layer)
        {
            TileData tile = layer.Tiles[tileIndex];
            CroppedBitmap? tileImage = _tilesetsService.GetTileImageAt(tile.TilesetID, tile.TilesetIndex);

            for (int i = 0; i < cell.Tiles.Count; i++)
            {
                if (cell.Tiles[i].LayerVM.VisibilityIndex == layer.VisibilityIndex)
                {
                    cell.Tiles[i].Tile = tile;
                    cell.Tiles[i].Image = tileImage;
                    return;
                }
                else if (cell.Tiles[i].LayerVM.VisibilityIndex < layer.VisibilityIndex)
                {
                    cell.Tiles.Insert(i, new TilemapCellTileViewModel(tile, tileImage, layer));
                    return;
                }
            }

            cell.Tiles.Add(new TilemapCellTileViewModel(tile, tileImage, layer));
        }

        public void EraseTile(TilemapCellViewModel cell, LayerViewModel layer)
        {
            for (int i = 0; i < cell.Tiles.Count; i++)
            {
                if (cell.Tiles[i].LayerVM.VisibilityIndex == layer.VisibilityIndex)
                {
                    cell.Tiles.RemoveAt(i);
                    break;
                }
            }
        }

        public void RemoveLayer(List<TilemapCellViewModel> cells, LayerViewModel layer)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (layer.Tiles[i].TilesetID == Guid.Empty)
                    continue;

                EraseTile(cells[i], layer);
            }
        }
    }
}
