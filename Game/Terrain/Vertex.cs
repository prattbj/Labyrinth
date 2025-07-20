using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MIConvexHull;
using System.Numerics;
namespace Labyrinth.Game.Terrain
{
    

    public class Vertex : IVertex
    {
        public double[] Position { get; }

        public Vertex(double x, double y)
        {
            Position = [x, y];
        }
        public static implicit operator Vector2(Vertex v) => new((float)v.X, (float)v.Y);
        public static implicit operator Vertex(Vector2 v) => new(v.X, v.Y);
        public static Vertex operator +(Vertex l, Vertex r) => new(r.X + l.X, r.Y + l.Y);
        public static Vertex operator *(Vertex l, float r) => new(l.X * r, l.Y * r);
        public static Vertex Zero => new(0, 0);
        public double X => Position[0];
        public double Y => Position[1];
    }
}