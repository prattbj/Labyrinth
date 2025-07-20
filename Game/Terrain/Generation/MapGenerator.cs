using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MIConvexHull;
using Raylib_cs;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
namespace Labyrinth.Game.Terrain.Generation
{
    public static class MapGenerator
    {
        // public List<Cell> Cells = new();
        // public List<Vertex> Points;
        
        // public Rectangle bounds;
        // public Vector2 SpawnLocation;
        
        // private Dictionary<(Vector2, Vector2), (Cell, Cell?)> edgeMap = [];
        // private List<(List<Vector2> polygon, Color color)> wallEdges = [];
        public enum Algorithm { Prim, RecursiveBacktrack };
        

        public static (List<CellInitializer>, CellInitializer, List<(Vector2, Vector2)>) GenerateMap(Rectangle bounds, Algorithm algorithm)
        {
            

            var points = new List<Vertex>();
            Random rng = new();

            int numSites = EstimatePointCount((int)bounds.Width, (int)bounds.Height, 64, 16, 0.8f);
            for (int i = 0; i < numSites; i++)
            {
                points.Add(new Vector2(rng.Next(0, (int)bounds.Width), rng.Next(0, (int)bounds.Height)));
            }

            var triangulation = DelaunayTriangulation<Vertex, Tri>.Create(points, 1e-10);
            List<CellInitializer> cellInitializers = [];
            // Initialize cells
            for (int i = 0; i < points.Count; i++)
                cellInitializers.Add(new CellInitializer(i));
            Dictionary<int, List<int>> Adjacency = [];
            // Build neighbor graph from triangles
            foreach (var tri in triangulation.Cells)
            {
                for (int i = 0; i < 3; i++)
                {
                    var a = points.IndexOf(tri.Vertices[i]);
                    var b = points.IndexOf(tri.Vertices[(i + 1) % 3]);

                    if (a == b) continue;

                    if (!Adjacency.ContainsKey(a)) Adjacency[a] = [];
                    if (!Adjacency[a].Contains(b)) Adjacency[a].Add(b);

                    if (!Adjacency.ContainsKey(b)) Adjacency[b] = [];
                    if (!Adjacency[b].Contains(a)) Adjacency[b].Add(a);
                }
            }

            // Assign neighbors to cells
            foreach (var kvp in Adjacency)
            {
                int idx = kvp.Key;
                var cell = cellInitializers[idx];

                foreach (var neighborIdx in kvp.Value)
                {
                    var neighbor = cellInitializers[neighborIdx];
                    if (!cell.Neighbors.Contains(neighbor))
                        cell.Neighbors.Add(neighbor);
                }
            }

            Dictionary<Tri, Vector2> triangleCircumcenters = [];

            foreach (var tri in triangulation.Cells)
            {
                var a = tri.Vertices[0];
                var b = tri.Vertices[1];
                var c = tri.Vertices[2];
                var circumcenter = GetCircumcenter(a, b, c);

                triangleCircumcenters[tri] = circumcenter;
            }

            // For each triangle, assign its circumcenter to all three sites
            foreach (var tri in triangulation.Cells)
            {
                var circumcenter = triangleCircumcenters[tri];

                foreach (var vertex in tri.Vertices)
                {
                    int siteIndex = points.IndexOf(vertex);
                    if (siteIndex >= 0)
                    {
                        cellInitializers[siteIndex].Polygon.Add(circumcenter);
                    }
                }
            }

            // Sort polygon points counter-clockwise for each cell (optional, but cleaner)
            foreach (CellInitializer cell in cellInitializers)
            {
                var polygon = cell.Polygon;
                if (polygon.Count < 3)
                {
                    cell.IsEdgeCell = true;
                    continue;
                }


                var center = polygon.Aggregate(Vector2.Zero, (sum, p) => sum + (Vector2)p) / polygon.Count;
                polygon.Sort((a, b) =>
                {
                    float angleA = MathF.Atan2((float)a.Y - center.Y, (float)a.X - center.X);
                    float angleB = MathF.Atan2((float)b.Y - center.Y, (float)b.X - center.X);
                    return angleA.CompareTo(angleB);
                });

                bool isOutOfBounds = polygon.Any(v =>
                    v.X < bounds.X ||
                    v.X > bounds.X + bounds.Width ||
                    v.Y < bounds.Y ||
                    v.Y > bounds.Y + bounds.Height);

                cell.IsEdgeCell = isOutOfBounds;

            }
            cellInitializers = [.. cellInitializers.Where(c => !c.IsEdgeCell)];
            foreach (var cell in cellInitializers)
            {
                cell.Neighbors = [.. cell.Neighbors.Where(n => !n.IsEdgeCell)];
            }
            CellInitializer startCell = algorithm switch
            {
                Algorithm.Prim => MazeGenerator.GeneratePrimMaze(1 << 9, cellInitializers),
                Algorithm.RecursiveBacktrack => MazeGenerator.RecursiveBacktrackMaze(1 << 9, cellInitializers),
                _ => throw new NotImplementedException()
            };

            List<(Vector2, Vector2)> longestPathEdges = [];
            MazeGenerator.ComputeLongestPathEdges(ref startCell, ref longestPathEdges);

            MazeGenerator.AssignSections(5, cellInitializers, ref longestPathEdges);
            return (cellInitializers, startCell, longestPathEdges);
            //InitializeWalls();
        }
        static private int EstimatePointCount(int width, int height, int playerSize = 64, float scale = 1.5f, float density = 0.8f)
        {
            float cellSize = playerSize * scale;
            int columns = (int)(width / cellSize);
            int rows = (int)(height / cellSize);
            int totalCells = columns * rows;

            return (int)(totalCells * density);
        }
        // public List<(List<Vector2> polygon, Color color)> GetWallEdges()
        // {
        //     return wallEdges;
        // } 
        

        
        

        

        

        
        public static System.Numerics.Vector2 Intersect(System.Numerics.Vector2 p1, System.Numerics.Vector2 p2, System.Numerics.Vector2 q1, System.Numerics.Vector2 q2)
        {
            // Line AB represented as a1x + b1y = c1
            float a1 = p2.Y - p1.Y;
            float b1 = p1.X - p2.X;
            float c1 = a1 * p1.X + b1 * p1.Y;

            // Line CD represented as a2x + b2y = c2
            float a2 = q2.Y - q1.Y;
            float b2 = q1.X - q2.X;
            float c2 = a2 * q1.X + b2 * q1.Y;

            float det = a1 * b2 - a2 * b1;

            if (Math.Abs(det) < 1e-5)
                return (p1 + p2) / 2; // fallback: mid-point

            float x = (b2 * c1 - b1 * c2) / det;
            float y = (a1 * c2 - a2 * c1) / det;

            return new System.Numerics.Vector2(x, y);
        }
        public static List<Vector2> ClipPolygonToRect(List<Vector2> polygon, Rectangle rect)
        {
            List<Vector2> output = [.. polygon];

            // Define rectangle edges as 4 clipping functions
            var clipEdges = new List<Func<System.Numerics.Vector2, bool>>()
            {
                (p) => p.X >= rect.X,                             // left
                (p) => p.X <= rect.X + rect.Width,                // right
                (p) => p.Y >= rect.Y,                             // top
                (p) => p.Y <= rect.Y + rect.Height                // bottom
            };

            var clipPoints = new List<Func<Vector2, Vector2, Vector2>>()
            {
                // intersect with left
                (a, b) => Intersect(a, b, new Vector2(rect.X, 0), new Vector2(rect.X, 1)),
                // intersect with right
                (a, b) => Intersect(a, b, new Vector2(rect.X + rect.Width, 0), new Vector2(rect.X + rect.Width, 1)),
                // intersect with top
                (a, b) => Intersect(a, b, new Vector2(0, rect.Y), new Vector2(1, rect.Y)),
                // intersect with bottom
                (a, b) => Intersect(a, b, new Vector2(0, rect.Y + rect.Height), new Vector2(1, rect.Y + rect.Height))
            };

            for (int i = 0; i < 4; i++)
            {
                var input = output;
                output = new List<Vector2>();

                if (input.Count == 0)
                    break;

                var S = input[^1]; // last point

                foreach (var E in input)
                {
                    bool insideE = clipEdges[i](E);
                    bool insideS = clipEdges[i](S);

                    if (insideE)
                    {
                        if (!insideS)
                            output.Add(clipPoints[i](S, E));
                        output.Add(E);
                    }
                    else if (insideS)
                    {
                        output.Add(clipPoints[i](S, E));
                    }

                    S = E;
                }
            }

            return output;
        }

        
        

        

        
        public static Vector2 GetCircumcenter(Vector2 a, Vector2 b, Vector2 c)
        {
            float ax = (float)a.X, ay = (float)a.Y;
            float bx = (float)b.X, by = (float)b.Y;
            float cx = (float)c.X, cy = (float)c.Y;

            float d = 2 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by));
            if (Math.Abs(d) < 1e-6f) return new Vector2(ax, ay); // fallback

            float ux = ((ax * ax + ay * ay) * (by - cy) +
                        (bx * bx + by * by) * (cy - ay) +
                        (cx * cx + cy * cy) * (ay - by)) / d;

            float uy = ((ax * ax + ay * ay) * (cx - bx) +
                        (bx * bx + by * by) * (ax - cx) +
                        (cx * cx + cy * cy) * (bx - ax)) / d;

            return new Vector2(ux, uy);
        }
        // public void DrawSolution(Color pathColor)
        // {
        //     if (longestPathEdges.Count < 2) return;

        //     List<Vector2> centers = new();

        //     foreach (var (a, b) in longestPathEdges)
        //     {
        //         var mid = ((Vector2)a + (Vector2)b) / 2f;
        //         centers.Add(mid);
        //     }
        //     //Two different ways to draw the path
        //     //Raylib.DrawSplineBezierQuadratic([.. centers], centers.Count, 2.5f / (Globals.GetGame()?.GetCamera2D().Zoom ?? 5), pathColor);
        //     for (int i = 0; i < centers.Count - 1; i++)
        //     {
        //         Raylib.DrawLine(
        //             (int)centers[i].X, (int)centers[i].Y,
        //             (int)centers[i + 1].X, (int)centers[i + 1].Y,
        //             pathColor
        //         );
        //     }
        // }
        public static bool PointInPolygon(System.Numerics.Vector2 point, List<System.Numerics.Vector2> polygon)
        {
            bool inside = false;
            int count = polygon.Count;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                System.Numerics.Vector2 pi = polygon[i];
                System.Numerics.Vector2 pj = polygon[j];

                bool intersect = ((pi.Y > point.Y) != (pj.Y > point.Y)) &&
                                (point.X < (pj.X - pi.X) * (point.Y - pi.Y) / (pj.Y - pi.Y + float.Epsilon) + pi.X);
                if (intersect)
                    inside = !inside;
            }

            return inside;
        }
        // public void Debug(Camera2D camera)
        // {
        //     if (Raylib.IsMouseButtonDown(MouseButton.Left))
        //     {
        //         Vector2 mousePos = Raylib.GetMousePosition();
        //         mousePos = Raylib.GetScreenToWorld2D(mousePos, camera);    
        //         foreach (var cell in Cells)
        //         {
        //             if (PointInPolygon(mousePos, cell.Polygon.Select(p => (Vector2)p).ToList()))
        //             {
        //                 // The mouse is inside this cell
        //                 Console.WriteLine($"Clicked cell. Index: {cell.Index}");
        //                 foreach (var neighbor in cell.Links)
        //                 {
        //                     Console.WriteLine($"Neighbor Index: {neighbor.Index}");
        //                 }
        //                 break;
        //             }
        //         }
        //     }
        // }

        // public void InitializeWalls()
        // {
        //     edgeMap = [];
        //     List<((Vector2, Vector2) edge, Color color)> mapEdges = [];
        //     Color[] sectionColors = [Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Yellow, Color.Beige];
        //     foreach (var cell in Cells)
        //     {
        //         var poly = cell.Polygon;
        //         int count = poly.Count;

        //         for (int i = 0; i < count; i++)
        //         {
        //             var a = (Vector2)poly[i];
        //             var b = (Vector2)poly[(i + 1) % count];

        //             var key = NormalizeEdgeKey(a, b);

        //             if (!edgeMap.TryGetValue(key, out var pair))
        //             {
        //                 edgeMap[key] = (cell, null);
        //             }
        //             else
        //             {
        //                 edgeMap[key] = (pair.Item1, cell);
        //             }
        //         }
        //     }

        //     // Precompute list of wall edges
        //     foreach (var kvp in edgeMap)
        //     {
        //         var (a, b) = kvp.Key;
        //         var (cellA, cellB) = kvp.Value;

        //         if (cellA != null && cellB != null)
        //         {
        //             var colorRGBA1 = sectionColors[cellA.SectionIndex >= 0 ? cellA.SectionIndex : 5];
        //             var colorRGBA2 = sectionColors[cellB.SectionIndex >= 0 ? cellB.SectionIndex : 5];

        //             Color color = new((colorRGBA1.R + colorRGBA2.R) / 2, (colorRGBA1.G + colorRGBA2.G) / 2, (colorRGBA1.B + colorRGBA2.B) / 2, 255);
        //             if (!cellA.IsLinkedTo(cellB))
        //                 mapEdges.Add(((a, b), color));
        //         }
        //         else if (cellA != null)
        //         {
        //             // Edge has only one adjacent cell (outer wall)
        //             mapEdges.Add(((a, b), sectionColors[(cellA?.SectionIndex ?? 5) >= 0 ? (cellA?.SectionIndex ?? 5) : 5]));
        //         }
        //         else if (cellB != null)
        //         {
        //             // Edge has only one adjacent cell (outer wall)
        //             mapEdges.Add(((a, b), sectionColors[(cellB?.SectionIndex ?? 5) >= 0 ? (cellB?.SectionIndex ?? 5) : 5]));
        //         }
        //     }
        //     //wallEdges = ChaoticEdges(mapEdges);
            
        // }
        // static List<(List<Vector2> polygon, Color color)> ChaoticEdges(List<((Vector2, Vector2), Color)> edges, float maxDisplacement = 40f)
        // {
        //     var rand = new Random();
        //     var result = new List<(List<Vector2>, Color)>();

        //     foreach (var ((a, b), color) in edges)
        //     {
        //         Vector2 direction = b - a;
        //         float length = direction.Length();
        //         direction = Vector2.Normalize(direction);

        //         Vector2 normal1 = new Vector2(-direction.Y, direction.X);
        //         Vector2 normal2 = -normal1;

        //         int jointCount = rand.Next(10, 50);
        //         var polygon = new List<Vector2>();

        //         float step = length / jointCount;

        //         for (int i = 0; i < jointCount; i++)
        //         {
        //             Vector2 pos = a + direction * (i * step);
        //             float displacement = (float)(rand.NextDouble() * maxDisplacement * 0.5f + maxDisplacement * 0.5f); // 50%–100% of max
        //             polygon.Add(pos + normal1 * displacement);
        //         }

        //         for (int i = jointCount - 1; i >= 0; i--)
        //         {
        //             Vector2 pos = a + direction * (i * step);
        //             float displacement = (float)(rand.NextDouble() * maxDisplacement * 0.5f + maxDisplacement * 0.5f);
        //             polygon.Add(pos + normal2 * displacement);
        //         }

        //         result.Add((polygon, color));
        //     }

        //     return result;
        // }
        // public void Draw()
        // {
        //     Color[] sectionColors = [Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Yellow, Color.Beige];

        //     // foreach (var cell in Cells)
        //     // {
        //     //     if (cell.Polygon.Count < 3) continue;

        //     //     var color = cell.SectionIndex >= 0 && cell.SectionIndex < sectionColors.Length
        //     //         ? sectionColors[cell.SectionIndex]
        //     //         : Color.Gray;

        //     //     var center = cell.Polygon.Aggregate(Vector2.Zero, (sum, v) => sum + (Vector2)v) / cell.Polygon.Count;
        //     //     //Raylib.DrawCircleV(center, 250, color);
        //     // }


        //     Vector2 playerPos = Globals.GetGame()?.GetPlayer()?.GetPos() ?? new(0, 0);
        //     foreach (var (polygon, color) in wallEdges)
        //     {
        //         if (polygon.Count < 3)
        //             continue;

        //         // Compute center point for fan origin
        //         Vector2 center = Vector2.Zero;
        //         bool isWithinArea = false;
        //         foreach (var p in polygon)
        //         {
        //             if (Raylib.CheckCollisionPointCircle(p, Globals.GetGame()?.GetPlayer()?.GetPos() ?? new(0, 0), 3000))
        //             {
        //                 isWithinArea = true;
        //             }
        //             center += p;
        //         }
        //         if (!isWithinArea)
        //             continue;
        //         center /= polygon.Count;

        //         // Texture coordinates (center is at 0.5, 0.5 for simplicity)
        //         List<Vector2> texcoords =
        //         [
        //             new Vector2(0.5f, 0.5f), // Center UV
        //         ];
                
        //         foreach (var p in polygon)
        //         {
        //             Vector2 offset = p - center;
        //             // Map texture coordinates from center
        //             texcoords.Add(new Vector2(
        //                 0.5f + offset.X / caveWallTexture.Width,
        //                 0.5f + offset.Y / caveWallTexture.Height
        //             ));
        //         }

        //         // Draw using triangle fan
                
        //         Rlgl.Begin(DrawMode.Triangles);
        //         Rlgl.SetTexture(caveWallTexture.Id);
        //         Rlgl.Color4ub(color.R, color.G, color.B, 100);

        //         for (int i = 0; i < polygon.Count; i++)
        //         {
        //             // Triangle from center, i, i+1
        //             Rlgl.TexCoord2f(texcoords[0].X, texcoords[0].Y);
        //             Rlgl.Vertex2f(center.X, center.Y);

        //             Rlgl.TexCoord2f(texcoords[i + 1].X, texcoords[i + 1].Y);
        //             Rlgl.Vertex2f(polygon[i].X, polygon[i].Y);

        //             Rlgl.TexCoord2f(texcoords[(i + 1) % polygon.Count + 1].X, texcoords[(i + 1) % polygon.Count + 1].Y);
        //             Rlgl.Vertex2f(polygon[(i + 1) % polygon.Count].X, polygon[(i + 1) % polygon.Count].Y);
        //         }

        //         Rlgl.End();
        //         Rlgl.SetTexture(0);
        //     }
        //     // foreach (var (edge, color) in wallEdges)
        //     // {
        //     //     var (a, b) = edge;

        //     //     if (Raylib.CheckCollisionCircleLine(playerPos, 2048, a, b))
        //     //     {
        //     //         Raylib.DrawLine((int)a.X, (int)a.Y, (int)b.X, (int)b.Y, color);
        //     //     }

        //     // }


        //     // Optional: draw solution
        //     DrawSolution(Color.Red);
        // }
        
        private static (Vector2, Vector2) NormalizeEdgeKey(System.Numerics.Vector2 a, System.Numerics.Vector2 b)
        {
            // Sort the edge endpoints to ensure uniqueness
            return (a.X < b.X || (a.X == b.X && a.Y <= b.Y)) ? (a, b) : (b, a);
        }
    }
}