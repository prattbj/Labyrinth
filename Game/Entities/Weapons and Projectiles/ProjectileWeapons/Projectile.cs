using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Numerics;
using Raylib_cs;
using Labyrinth.Game.Terrain;

namespace Labyrinth.Game.Entities.Projectiles
{
    public abstract class Projectile
    {
        protected Texture2D texture;
        protected float speed;
        private Vector2 pos;
        private Vector2 direction;
        private int currentCellKey;
        private float radius;
        protected Projectile(Vector2 pos, Vector2 direction, float speed, int currentCellKey)
        {
            this.pos = pos;
            this.direction = direction;
            this.currentCellKey = currentCellKey;
            this.speed = speed;
            radius = texture.Width / 2;
            Globals.GetGame()?.AddProjectile(this);
        }
        public void Draw()
        {
            //Raylib.DrawTextureEx(texture, pos, (float)Math.Atan2(direction.Y, direction.X), 1, Color.White); // This will be once texture is created
            Raylib.DrawCircle((int)(pos.X + radius), (int)(pos.Y + radius), 3, Color.Green);
        }

        public Texture2D GetTexture()
        {
            return texture;
        }
        public void Move()
        {
            Vector2 currentPos = pos;

            float frameSpeed = speed / Globals.GetTickRate();
            Vector2 movement = Raymath.Vector2Scale(direction, frameSpeed);

            Vector2 nextPos = Raymath.Vector2Add(currentPos, movement);

            //Check collision here



            Cell? currentCell = Globals.GetGame()?.GetMap().GetCell(currentCellKey);
            if (currentCell != null)
            {
                foreach (var (a, b) in currentCell.GetCollisionLines())
                {
                    if (LinesIntersect(currentPos, nextPos, a, b))
                    {
                        Globals.GetGame()?.RemoveProjectile(this);
                        break;
                    }
                }

                foreach (var (a, b, cellA, cellB) in currentCell.GetLinkedLines())
                {
                    if (LinesIntersect(currentPos, nextPos, a, b))
                    {
                        currentCellKey = (currentCellKey == cellA) ? cellB : cellA;
                        break;
                    }
                }
            }
            pos = nextPos;
            
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