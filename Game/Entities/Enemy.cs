using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Labyrinth.Game.Terrain;
using Raylib_cs;

namespace Labyrinth.Game.Entities
{
    public abstract class Enemy : Interactable
    {
        public Enemy(Map map, Dictionary<int, List<Enemy>> enemies, Vector2 pos)
        {
            this.enemies = enemies;
            this.map = map;
            this.pos = pos;
        }
        Dictionary<int, List<Enemy>> enemies;
        protected abstract Texture2D Texture { get; }
        private int currentCellKey;
        protected Map map;
        protected int range;
        protected int originCell;
        protected Vector2 pos;
        protected int radius;
        public void Draw()
        {
            Raylib.DrawTextureEx(Texture, pos, 0, 1, Color.White);
        }
        abstract protected void Interact();
        public void PerformActions()
        {
            Move();
            Interact();
        }
        public int GetCurrentCellKey()
        {
            return currentCellKey;
        }
        public void Move()
        {
            Cell currentCell = map.GetCell(currentCellKey);
            Vector2 previousCenter = new();
            Vector2 proposedCenter = new();
            Vector2 pThrowAway = new();
            foreach (var (a, b, cellA, cellB) in currentCell.GetLinkedLines())
            {
                if (Raylib.CheckCollisionLines(previousCenter, proposedCenter, a, b, ref pThrowAway))
                {
                    enemies[currentCellKey].Remove(this);
                    currentCellKey = (currentCellKey == cellA) ? cellB : cellA;
                    enemies[currentCellKey].Add(this);
                    break;
                }
            }
        }
    }
}