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

        public Rect RectArea
        {
            get
            {
                Vec2<int> leftTopMargin = new Vec2<int>(StartTile.X > 0 ? 1 : 0, StartTile.Y > 0 ? 1 : 0);
                return new Rect( x: StartTile.X * (TileSize.X + TileMargin.X) - TileMargin.X * leftTopMargin.X,
                          y: StartTile.Y * (TileSize.Y + TileMargin.Y) - TileMargin.X * leftTopMargin.Y,
                          width: (EndTile.X - StartTile.X + 1) * (TileSize.X + TileMargin.X) + TileMargin.X * leftTopMargin.X,
                          height: (EndTile.Y - StartTile.Y + 1) * (TileSize.Y + TileMargin.Y) + TileMargin.Y * leftTopMargin.Y);
            }
          }
    }
}
