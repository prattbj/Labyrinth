using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;
namespace Labyrinth.Game.Terrain.Generation
{
    public class CellInitializer
        {
            public int Index;
            public int SectionIndex = -1;
            public List<CellInitializer> Neighbors = new();
            public List<CellInitializer> Links = new();
            public List<Vector2> Polygon = new();
            public bool Visited = false;
            public CellInitializer? Parent = null;
            public bool IsEdgeCell = false;
            public CellInitializer(int index)
            {
                Index = index;
            }

            public void Link(CellInitializer other)
            {
                if (!Links.Contains(other))
                    Links.Add(other);
                if (!other.Links.Contains(this))
                    other.Links.Add(this);
            }
            
            public bool IsUnlinked => Links.Count == 0;

            public bool IsLinkedTo(CellInitializer other) => Links.Contains(other);
        }
}