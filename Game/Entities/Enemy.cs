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
        public Enemy(Map map, List<Enemy> enemies, Vector2 pos, int currentCellKey)
        {
            this.enemies = enemies;
            this.map = map;
            this.pos = pos;
            this.currentCellKey = currentCellKey;
        }
        List<Enemy> enemies;
        protected abstract Texture2D Texture { get; }
        private int currentCellKey;
        protected Map map;
        protected int range;
        protected int originCell;
        protected Vector2 pos;
        protected int radius;
        protected Vector2 movementVector;
        public void Draw()
        {
            Raylib.DrawTextureEx(Texture, pos - new Vector2(radius, radius), 0, 1, Color.White);
            Raylib.DrawText(currentCellKey.ToString(), (int)pos.X, (int)pos.Y, 16, Color.Brown);
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
        static Vector2 ClosestPointOnLineSegment(Vector2 a, Vector2 b, Vector2 point)
        {
            Vector2 ab = b - a;
            float abLengthSq = ab.LengthSquared();
            if (abLengthSq == 0) return a;

            float t = Vector2.Dot(point - a, ab) / abLengthSq;
            t = Math.Clamp(t, 0f, 1f);
            return a + ab * t;
        }

        protected void SetDirection(float range)
        {
            Player? player = Globals.GetGame()?.GetPlayer();
            if (player != null)
            {
                Vector2 playerPos = player.GetPos();

                Cell currentCell = map.GetCell(GetCurrentCellKey());
                Vector2 pointThrowaway = new();
                bool los = true;
                foreach (var (a, b) in currentCell.GetCollisionLines())
                {
                    if (Raylib.CheckCollisionLines(pos, playerPos, a, b, ref pointThrowaway))
                    {
                        los = false;
                        break;
                    }
                }
                if (los && Vector2.Distance(playerPos, pos) > range)
                {
                    movementVector = Vector2.Normalize(playerPos - pos);
                }
                else
                {
                    movementVector = new(0, 0);
                }

            }
        }
        protected void PathFind()
        {
            Player? player = Globals.GetGame()?.GetPlayer();
            if (player != null)
            {
                Vector2 playerPos = player.GetPos();

                Cell currentCell = map.GetCell(GetCurrentCellKey());
                List<(Vector2, Vector2)> lines = currentCell.GetCollisionLines();
                movementVector = new(0, 0);
            }
        }
        protected void Move()
        {
            Cell currentCell = map.GetCell(currentCellKey);

            //Vector2 centerOffset = new(radius, radius);

            Vector2 previousCenter = pos;// + centerOffset;

            pos += movementVector + GetDisplacement();
            Vector2 newCenter = pos;// + centerOffset;

            foreach (var (a, b) in currentCell.GetCollisionLines())
            {
                Vector2 closest = ClosestPointOnLineSegment(a, b, newCenter);
                float distSq = Vector2.DistanceSquared(newCenter, closest);

                if (distSq < radius * radius)
                {
                    Vector2 collisionNormal = Vector2.Normalize(newCenter - closest);
                    float penetrationDepth = radius - MathF.Sqrt(distSq);
                    pos += collisionNormal * penetrationDepth;

                    newCenter = pos;// + centerOffset;
                }
            }

            Vector2 proposedCenter = pos;// + centerOffset;
            // Cell currentCell = map.GetCell(currentCellKey);
            // Vector2 previousCenter = new();
            // Vector2 proposedCenter = new();
            foreach (var (a, b, cellA, cellB) in currentCell.GetLinkedLines())
            {
                if (LinesIntersect(previousCenter, proposedCenter, a, b))
                {
                    currentCellKey = (currentCellKey == cellA) ? cellB : cellA;
                    break;
                }
            }
        }
        
        static bool LinesIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
        {
            static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;

            Vector2 r = p2 - p1;
            Vector2 s = q2 - q1;
            float denom = Cross(r, s);

            if (denom == 0) return false; // Parallel lines

            float u = Cross(q1 - p1, r) / denom;
            float t = Cross(q1 - p1, s) / denom;

            return t >= 0 && t <= 1 && u >= 0 && u <= 1;
        }
    }
}