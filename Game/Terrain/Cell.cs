using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;
using Raylib_cs;
using System.Net.Quic;
using System.Reflection;
using System.Diagnostics;
namespace Labyrinth.Game.Terrain
{
    public class Cell
    {
        // public List<Cell> Neighbors = new();
        // public List<Cell> Links = new();

        public readonly static Color[] sectionColors = [Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Yellow, Color.Beige];
        private Dictionary<int, Cell> cells;
        private Vector2[] Polygon;
        private List<(Vector2, Vector2)> lines = [];
        private List<Point[]> trianglesToDraw = [];
        private List<(Vector2, Vector2)> collisionLines = [];
        private List<(Vector2, Vector2, int, int)> linkedLines = [];
        private List<int> neighbors = [];
        private List<int> links = [];
        private bool seen = false;
        private readonly int section = -1;
        private readonly int index;
        private readonly Vector2 center;
        static Shader shader = Raylib.LoadShader(null, "./Assets/Shaders/WallFragment.glsl");
        static Texture2D wallTexture = Raylib.LoadTexture("./Assets/Textures/CaveWall.png");

        //static RenderTexture2D 
        static readonly Vector2[] uvs =
        [
            new Vector2(0, 0),
            new Vector2(2, 0),
            new Vector2(2, 2),
            new Vector2(0, 2)
        ];

        public Cell(Generation.CellInitializer cellInitializer, Dictionary<int, Cell> cells)
        {
            Raylib.SetTextureWrap(wallTexture, TextureWrap.Repeat);
            Polygon = [.. cellInitializer.Polygon];
            section = cellInitializer.SectionIndex;
            this.cells = cells;
            index = cellInitializer.Index;
            center = Polygon.Aggregate(Vector2.Zero, (sum, v) => sum + v) / Polygon.Length;
            foreach (var c in cellInitializer.Neighbors)
            {
                neighbors.Add(c.Index);
            }
            foreach (var c in cellInitializer.Links)
            {
                links.Add(c.Index);
            }






        }
        struct Point(float X, float Y, float U, float V)
        {

            public float X = X;
            public float Y = Y;
            public float U = U;
            public float V = V;
        }


        public void CreateEdges()
        {
            float scale = 0.8f;
            float textureRepeatFactor = 0.05f;


            foreach (var (a, b) in lines)
            {
                Vector2 scaledA = ScalePoint(a, scale);
                Vector2 scaledB = ScalePoint(b, scale);

                // Compute edge direction and length
                float edgeLength = Vector2.Distance(a, b);
                float repeatU = edgeLength * textureRepeatFactor;

                Point[] vertices =
                [
                    new(a.X, a.Y, 0, 0),
                    new(scaledB.X, scaledB.Y, repeatU, 0),
                    new(b.X, b.Y, repeatU, 1),


                    new(a.X, a.Y, 0, 0),
                    new(scaledA.X, scaledA.Y, repeatU, 1),
                    new(scaledB.X, scaledB.Y, 0, 1),
                ];
                foreach (var link in linkedLines)
                {
                    if (a == link.Item1)
                    {
                        collisionLines.Add((a, scaledA));
                    }
                    if (a == link.Item2)
                    {
                        collisionLines.Add((a, scaledA));
                    }
                    if (b == link.Item2)
                    {
                        collisionLines.Add((b, scaledB));
                    }
                    if (b == link.Item1)
                    {
                        collisionLines.Add((b, scaledB));
                    }
                }

                collisionLines.Add((scaledA, scaledB));
                trianglesToDraw.Add(vertices);
                foreach (int neighborIndex in links)
                {
                    if (!cells.TryGetValue(neighborIndex, out var neighbor))
                        continue;

                    // Skip if the neighbor hasn't generated geometry yet
                    if (neighbor.trianglesToDraw.Count == 0)
                        continue;

                    foreach (var (nA, nB) in neighbor.lines)
                    {
                        if (a == nA || a == nB)
                        {
                            Vector2 shared = a;
                            Vector2 s = neighbor.ScalePoint(shared, scale);
                            Point[] newPoint = [
                                new(shared.X, shared.Y, 0, 0),
                                new(s.X, s.Y, 0, 1),
                                new(scaledA.X, scaledA.Y, 0, 0),

                            ];
                            collisionLines.Add((s, scaledA));
                            trianglesToDraw.Add(newPoint);
                            break;
                        }
                        else if (b == nA || b == nB)
                        {
                            Vector2 shared = b;
                            Vector2 s = neighbor.ScalePoint(shared, scale);
                            Point[] newPoint = [
                                new(shared.X, shared.Y, 0, 0),
                                new(scaledB.X, scaledB.Y, 0, 1),
                                new(s.X, s.Y, 0, 0),
                            ];
                            collisionLines.Add((s, scaledB));
                            trianglesToDraw.Add(newPoint);
                            break;
                        }
                    }
                }
            }

        }

        private Vector2 ScalePoint(Vector2 point, float scale)
        {
            Vector2 direction = point - center;
            return center + direction * scale;
        }

        public Vector2 GetCenter()
        {
            return center;
        }

        // public List<(Vector2, Vector2)> GetCollisionLines()
        // {
        //     return lines;
        // }

        public List<(Vector2, Vector2)> GetCollisionLines()
        {
            List<(Vector2, Vector2)> all = [];
            all.AddRange(collisionLines);
            foreach (var link in links)
            {
                all.AddRange(cells[link].collisionLines);
            }
            return all;
        }
        public int GetIndex()
        {
            return index;
        }
        public void InitializeLines()
        {
            int count = Polygon.Length;
            for (int i = 0; i < count; i++)
            {
                Vector2 a = Polygon[i];
                Vector2 b = Polygon[(i + 1) % count];

                bool shared = false;

                foreach (int l in links)
                {
                    Cell link = cells[l];
                    var poly = link.Polygon;
                    int linkCount = poly.Length;

                    for (int j = 0; j < linkCount; j++)
                    {
                        Vector2 la = poly[j];
                        Vector2 lb = poly[(j + 1) % linkCount];

                        if ((a == la && b == lb) || (a == lb && b == la))
                        {
                            shared = true;
                            linkedLines.Add((la, lb, index, l));
                            break;
                        }
                    }

                    if (shared) break;
                }

                if (!shared)
                {
                    lines.Add((a, b));
                }
            }
        }
        public List<(Vector2, Vector2)> GetLines()
        {
            return lines;
        }
        public List<(Vector2, Vector2, int, int)> GetLinkedLines()
        {
            return linkedLines;
        }
        public Vector2[] GetPolygon()
        {
            return Polygon;
        }

        public int GetSection()
        {
            return section;
        }

        public bool IsSeen()
        {
            return seen;
        }
        public void Draw()
        {


            Rlgl.Begin(DrawMode.Triangles);
            Rlgl.SetTexture(wallTexture.Id);
            DrawEdges();
            Rlgl.End();

            Rlgl.SetTexture(0);
        }
        public void DrawEdges(HashSet<int>? drawnCells = null)
        {
            drawnCells ??= [index];
            //if (drawnCells.Contains(index)) return;
            //drawnCells.Add(index);

            seen = true;
            Vector2 currentPos = Globals.GetGame()?.GetPlayer()?.GetPos() ?? new(0, 0);
            int radius = 1500;


            float scale = 0.003f;
            Color color = section > -1 ? sectionColors[section] : Color.Beige;
            Rlgl.Color4ub(color.R, color.G, color.B, color.A);

            foreach (var triangle in trianglesToDraw)
            {

                if (Raylib.CheckCollisionCircleLine(currentPos, radius, new Vector2(triangle[0].X, triangle[0].Y), new Vector2(triangle[1].X, triangle[1].Y)))
                {
                    foreach (var point in triangle)
                    {
                        Rlgl.TexCoord2f(point.X * scale, point.Y * scale);
                        Rlgl.Vertex2f(point.X, point.Y);
                    }
                }
            }
            foreach (var i in neighbors)
            {
                if (drawnCells.Add(i))
                {
                    foreach (var (a, b) in cells[i].lines)
                    {
                        if (Raylib.CheckCollisionCircleLine(currentPos, radius, a, b))
                        {
                            cells[i].DrawEdges(drawnCells);
                            break;
                        }
                    }
                }
                
                
            }
            // foreach (var (a, b, index1, index2) in linkedLines)
            // {
            //     if (Raylib.CheckCollisionCircleLine(currentPos, radius, a, b))
            //     {
            //         cells[index1 == index ? index2 : index1].DrawEdges(drawnCells);
            //     }
            // }
        }
        
    }
}