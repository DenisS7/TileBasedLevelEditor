using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TileBasedLevelEditor.Interfaces;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Services.Rendering.Data;

namespace TileBasedLevelEditor.Services
{
    public class TilemapRendererService : ITilemapRendererService
    {
        private readonly ITilesetsService _tilesetsService;
        public TilemapRendererService(ITilesetsService tilesetsService)
        {
            _tilesetsService = tilesetsService;
        }

        private List<Layer> GetTopLayersForTileAt(Tilemap tilemap, int tileIndex, bool onlyVisible, int n = 1)
        {
            List<Layer> topLayers = new List<Layer>();
            int currentLayerPosition = 0;
            
            foreach (Layer layer in tilemap.Layers)
            {
                if (onlyVisible && !layer.Visible)
                    continue;

                if (layer.Tiles[tileIndex].TilesetID != Guid.Empty)
                {
                    topLayers.Add(layer);
                    currentLayerPosition++;
                    if (currentLayerPosition >= n && n != 0)
                        return topLayers;
                }
            }

            return topLayers;
        }

        public List<TilemapCell> GetRenderedTilemapCells(Tilemap tilemap)
        {
            List<TilemapCell> result = new List<TilemapCell>();

            for (int i = 0; i < tilemap.TilemapSize.X * tilemap.TilemapSize.Y; i++)
            {
                TilemapCell cell = new TilemapCell(new Vec2<int>(i % tilemap.TilemapSize.Y, i / tilemap.TilemapSize.X));
                for (int j = tilemap.Layers.Count - 1; j >= 0; j--)
                {
                    TileData tile = tilemap.Layers[j].Tiles[i];
                    cell.Tiles.Add(new TilemapCellTile(tile, _tilesetsService.GetTileImageAt(tile.TilesetID, tile.TilesetIndex), tilemap.Layers[j]));
                }
                result.Add(cell);
            }

            return result;
        }
    }
}
