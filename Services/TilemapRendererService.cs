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
                TilemapCellViewModel cell = new TilemapCellViewModel(new Vec2<int>(i % tilemapVM.TilemapSize.X, i / tilemapVM.TilemapSize.X));
                for (int j = 0; j < tilemapVM.Layers.Count - 1; j++)
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

        public void SwapLayers(List<TilemapCellViewModel> cells, LayerViewModel layer1, LayerViewModel layer2)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                TilemapCellTileViewModel? tile1 = null;
                TilemapCellTileViewModel? tile2 = null;
                for (int j = 0; j < cells[i].Tiles.Count; j++)
                {
                    if (cells[i].Tiles[j].LayerVM == layer1)
                    {
                        tile1 = cells[i].Tiles[j];
                        if (tile2 != null)
                            break;
                    }
                    else if (cells[i].Tiles[j].LayerVM == layer2)
                    {
                        tile2 = cells[i].Tiles[j];
                        if (tile1 != null)
                            break;
                    }
                }

                if (tile1 != null && tile2 != null)
                {
                    (tile1, tile2) = (tile2, tile1);
                    continue;
                }

                if (tile1 == null)
                {
                    if (tile2 == null)
                        continue;

                    layer1.Tiles[i] = tile2.Tile;
                    ApplyTile(cells[i], i, layer1);
                    continue;
                }

                layer2.Tiles[i] = tile1.Tile;
                ApplyTile(cells[i], i, layer2);
            }
        }
    }
}
