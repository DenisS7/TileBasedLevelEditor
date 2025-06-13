using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TileBasedLevelEditor.Models
{
    public class SelectionArea
    {
        public Vec2<int> StartTile;
        public Vec2<int> EndTile;
        public Vec2<int> TileSize;
        public Vec2<int> TileMargin;

        public SelectionArea(Vec2<int> startTile, Vec2<int> endTile, Vec2<int> tileSize, Vec2<int> tileMargin)
        {
            StartTile = startTile;
            EndTile = endTile;
            TileSize = tileSize;
            TileMargin = tileMargin;
        }

        public Rect RectArea => new Rect(
          x: StartTile.X * (TileSize.X + TileMargin.X),
          y: StartTile.Y * (TileSize.Y + TileMargin.Y),
          width: (EndTile.X - StartTile.X + 1) * (TileSize.X + TileMargin.X) + TileMargin.X,
          height: (EndTile.Y - StartTile.Y + 1) * (TileSize.Y + TileMargin.Y) + TileMargin.Y
        );
    }
}
