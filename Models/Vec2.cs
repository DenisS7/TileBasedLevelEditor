using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedLevelEditor.Models
{
    public class Vec2<T>
    {
        public T X { get; set; }
        public T Y { get; set; }

        public Vec2(T x, T y)
        {
            X = x;
            Y = y;
        }

        public Vec2(T a)
        {
            X = Y = a;
        }
    }
}
