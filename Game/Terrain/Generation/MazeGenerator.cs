using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;
namespace Labyrinth.Game.Terrain.Generation
{

    public static class MazeGenerator
    {
        public static void AssignSections(int sectionCount, List<CellInitializer> cellInitializers, ref List<(Vector2, Vector2)> longestPathEdges)
        {
            if (longestPathEdges.Count < sectionCount)
                return;

            // Reset all section assignments
            foreach (var cell in cellInitializers)
                cell.SectionIndex = -1;

            int edgesPerSection = longestPathEdges.Count / sectionCount;
            int remaining = longestPathEdges.Count % sectionCount;

            int currentIndex = 0;

            // Track path cells and assign them by hard-cut sections
            Dictionary<int, List<CellInitializer>> sectionCells = [];

            for (int section = 0; section < sectionCount; section++)
            {
                sectionCells[section] = [];
                int count = edgesPerSection + (section < remaining ? 1 : 0);
                int start = currentIndex;
                int end = currentIndex + count;

                for (int i = start; i < end && i < longestPathEdges.Count; i++)
                {
                    var (a, b) = longestPathEdges[i];

                    var cellsWithEdge = cellInitializers.Where(c =>
                        CommonPoints(c.Polygon, [a, b]).Count == 2
                    );

                    foreach (var cell in cellsWithEdge)
                    {
                        if (cell.SectionIndex == -1)
                        {
                            cell.SectionIndex = section;
                            sectionCells[section].Add(cell);
                        }
                    }
                }

                currentIndex = end;
            }

            // Now flood-fill from each section's path cells to assign the rest
            HashSet<CellInitializer> visited = [.. cellInitializers.Where(c => c.SectionIndex != -1)];
            Queue<(CellInitializer cell, int section)> queue = new();

            foreach (var kvp in sectionCells)
            {
                int section = kvp.Key;
                foreach (var cell in kvp.Value)
                {
                    queue.Enqueue((cell, section));
                }
            }

            while (queue.Count > 0)
            {
                var (cell, section) = queue.Dequeue();

                foreach (var neighbor in cell.Links)
                {
                    if (neighbor.SectionIndex == -1)
                    {
                        neighbor.SectionIndex = section;
                        queue.Enqueue((neighbor, section));
                    }
                }
            }

            Console.WriteLine("Section assignment complete.");
        }
        private static List<(Vector2, Vector2)> GetEdges(List<Vector2> polygon)
        {
            var result = new List<(Vector2, Vector2)>();
            int count = polygon.Count;

            for (int i = 0; i < count; i++)
            {
                var a = (System.Numerics.Vector2)polygon[i];
                var b = (System.Numerics.Vector2)polygon[(i + 1) % count];
                result.Add((a, b));
            }

            return result;
        }
        private static (Vector2, Vector2)? FindSharedEdge(List<Vector2> a, List<Vector2> b)
        {
            var edgesA = GetEdges(a);
            var edgesB = GetEdges(b);

            foreach (var ea in edgesA)
            {
                foreach (var eb in edgesB)
                {
                    if (EdgesMatch(ea, eb, 0.1f))
                        return ea;
                }
            }

            return null;
        }
        private static bool EdgesMatch((Vector2, Vector2) a, (Vector2, Vector2) b, float tolerance)
        {
            return (System.Numerics.Vector2.Distance((System.Numerics.Vector2)a.Item1, (System.Numerics.Vector2)b.Item1) < tolerance && System.Numerics.Vector2.Distance((System.Numerics.Vector2)a.Item2, (System.Numerics.Vector2)b.Item2) < tolerance)
                || (System.Numerics.Vector2.Distance((System.Numerics.Vector2)a.Item1, (System.Numerics.Vector2)b.Item2) < tolerance && System.Numerics.Vector2.Distance((System.Numerics.Vector2)a.Item2, (System.Numerics.Vector2)b.Item1) < tolerance);
        }
        public static void ComputeLongestPathEdges(ref CellInitializer startCell, ref List<(Vector2, Vector2)> longestPathEdges)
        {
            ComputeLongestPath(ref startCell, ref longestPathEdges);
            longestPathEdges = [];
            ComputeLongestPath(ref startCell, ref longestPathEdges);
        }
        private static void ComputeLongestPath(ref CellInitializer startCell, ref List<(Vector2, Vector2)> longestPathEdges)
        {
            var rand = new Random();
            CellInitializer start = startCell;

            // BFS to find distances
            Dictionary<CellInitializer, CellInitializer?> cameFrom = [];
            Dictionary<CellInitializer, int> distance = [];

            Queue<CellInitializer> queue = new();
            queue.Enqueue(start);
            cameFrom[start] = null;
            distance[start] = 0;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var neighbor in current.Links)
                {
                    if (!cameFrom.ContainsKey(neighbor))
                    {
                        cameFrom[neighbor] = current;
                        distance[neighbor] = distance[current] + 1;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            // Find furthest
            CellInitializer furthest = distance.OrderByDescending(kvp => kvp.Value).First().Key;
            startCell = furthest;
            // Reconstruct path
            var path = new List<CellInitializer>();
            var walker = furthest;
            while (walker != null)
            {
                path.Add(walker);
                walker = cameFrom[walker];
            }

            path.Reverse();

            // Convert path into edge lines
            for (int i = 0; i < path.Count - 1; i++)
            {
                var a = path[i].Polygon;
                var b = path[i + 1].Polygon;

                var edge = FindSharedEdge(a, b);
                if (edge != null)
                {
                    longestPathEdges.Add(edge.Value);
                }
            }

        }
        private static float LineLength(List<Vector2> points)
        {
            return points.Count == 2 ? Vector2.Distance(points[0], points[1]) : 0f;
        }
        private static bool SharedSideIsWideEnough(CellInitializer a, CellInitializer b, float minWidth)
        {
            var polyA = a.Polygon;
            var polyB = b.Polygon;
            var shared = CommonPoints(polyA, polyB);
            return LineLength(shared) >= minWidth;
        }
        private static List<Vector2> CommonPoints(List<Vector2> a, List<Vector2> b)
        {
            var common = new List<Vector2>();
            foreach (var pa in a)
            {
                foreach (var pb in b)
                {
                    if (Vector2.Distance(pa, pb) < 0.01f)
                        common.Add(pa);
                }
            }

            return common;
        }
        public static CellInitializer GeneratePrimMaze(float minWidth, List<CellInitializer> cellInitializers)
        {
            var rand = new Random();
            var frontier = new List<CellInitializer>();
            var start = cellInitializers[rand.Next(cellInitializers.Count)];
            var startCell = start;
            start.Visited = true;

            foreach (var n in start.Neighbors.Where(n => SharedSideIsWideEnough(start, n, minWidth)))
            {
                frontier.Add(n);
                n.Parent = start;
            }

            while (frontier.Count > 0)
            {
                // Pick a random frontier cell
                var index = rand.Next(frontier.Count);
                var current = frontier[index];
                frontier.RemoveAt(index);

                // Check valid visited neighbors (could use Parent link or check all neighbors)
                var visitedNeighbors = current.Neighbors
                    .Where(n => n.Visited && SharedSideIsWideEnough(current, n, minWidth))
                    .ToList();

                if (visitedNeighbors.Count > 0)
                {
                    var neighbor = visitedNeighbors[rand.Next(visitedNeighbors.Count)];
                    current.Link(neighbor);
                    current.Visited = true;

                    foreach (var n in current.Neighbors
                            .Where(n => !n.Visited && SharedSideIsWideEnough(current, n, minWidth)))
                    {
                        if (!frontier.Contains(n))
                        {
                            frontier.Add(n);
                            n.Parent = current;
                        }
                    }
                }
            }
            return startCell;
        }
        public static CellInitializer RecursiveBacktrackMaze(float minWidth, List<CellInitializer> cellInitializers)
        {
            var rand = new Random();
            var stack = new Stack<CellInitializer>();
            var start = cellInitializers[rand.Next(cellInitializers.Count)];
            var startCell = start;
            stack.Push(start);

            while (stack.Count > 0)
            {
                var current = stack.Peek();

                var neighbors = current.Neighbors
                    .Where(n => n.IsUnlinked && SharedSideIsWideEnough(current, n, minWidth))
                    .ToList();

                if (neighbors.Count == 0)
                {
                    stack.Pop();
                }
                else
                {
                    var next = neighbors[rand.Next(neighbors.Count)];
                    current.Link(next);
                    stack.Push(next);
                }
            }
            return startCell;
        }
    }
}