using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedLevelEditor.Models;

namespace TileBasedLevelEditor.CustomArgs
{
    public enum DragStage
    {
        Start = 0,
        Dragging = 1,
        End = 2
    }
    public class TileSelectionArgs
    {
        public Vec2<int>? Index { get; }
        public bool Add { get; }
        public DragStage CurrentDragStage { get; }

        public TileSelectionArgs(Vec2<int>? index, bool add, DragStage currentDragStage)
        {
            Index = index; 
            Add = add;
            CurrentDragStage = currentDragStage;
        }
    }
}
