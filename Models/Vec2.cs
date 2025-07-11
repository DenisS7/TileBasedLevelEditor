using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TileBasedLevelEditor.Models
{
    public class Vec2<T> where T : INumber<T>
    {
        public T X { get; set; }
        public T Y { get; set; }

        [JsonConstructor]
        public Vec2(T x, T y)
        {
            X = x;
            Y = y;
        }

        public Vec2(T a)
        {
            X = Y = a;
        }

        public static implicit operator Vec2<T>(T scalar)
            => new Vec2<T>(scalar, scalar);

        public static Vec2<T> operator +(Vec2<T> a, Vec2<T> b)
        {
            return new Vec2<T>(a.X + b.X, a.Y + b.Y);
        }

        public static Vec2<T> operator -(Vec2<T> a, Vec2<T> b)
        {
            return new Vec2<T>(a.X - b.X, a.Y - b.Y);
        }

        public static Vec2<T> operator /(Vec2<T> a, Vec2<T> b)
        {
            return new Vec2<T>(a.X / b.X, a.Y / b.Y);
        }

        public static Vec2<T> operator *(Vec2<T> a, Vec2<T> b)
        {
            return new Vec2<T>(a.X * b.X, a.Y * b.Y);
        }

        public static bool operator <(Vec2<T> a, Vec2<T> b)
        {
            return a.X < b.X || a.Y < b.Y;
        }

        public static bool operator >(Vec2<T> a, Vec2<T> b)
        {
            return b < a;
        }

        public static bool operator <=(Vec2<T> a, Vec2<T> b)
        {
            return a.X <= b.X || a.Y <= b.Y;
        }

        public static bool operator >=(Vec2<T> a, Vec2<T> b)
        {
            return b <= a;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Vec2<T> other)
                return false;

            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}
