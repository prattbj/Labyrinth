using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;
using Raylib_cs;
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
        private List<(Vector2, Vector2, int, int)> linkedLines = [];
        private List<int> neighbors = [];
        private List<int> links = [];
        private bool seen = false;
        private int section = -1;
        private int index;
        public Cell(Generation.CellInitializer cellInitializer, Dictionary<int, Cell> cells)
        {
            Polygon = [.. cellInitializer.Polygon];
            section = cellInitializer.SectionIndex;
            this.cells = cells;
            index = cellInitializer.Index;
            foreach (var c in cellInitializer.Neighbors)
            {
                neighbors.Add(c.Index);
            }
            foreach (var c in cellInitializer.Links)
            {
                links.Add(c.Index);
            }





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
        public void Draw()
        {
            DrawSelf();
            foreach (var l in links)
            {
                cells[l].DrawSelf();
            }
        }
        public bool IsSeen()
        {
            return seen;
        }
        private void DrawSelf()
        {
            seen = true;
            foreach (var (a, b) in lines)
            {
                Raylib.DrawLineV(a, b, section > -1 ? sectionColors[section] : Color.Beige); // You can change the color
            }
        }
        
    }
}