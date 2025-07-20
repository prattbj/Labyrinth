using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Numerics;
using Raylib_cs;

namespace Labyrinth.Game.Projectiles
{
    public abstract class Projectile
    {
        protected Texture2D texture;
        protected float speed;
        private Vector2 pos;
        private Vector2 direction;
        protected Projectile(Vector2 pos, Vector2 direction)
        {
            this.pos = pos;
            this.direction = direction;
            Globals.GetGame()?.AddProjectile(this);
        }
        public void Draw()
        {
            //Raylib.DrawTextureEx(texture, pos, (float)Math.Atan2(direction.Y, direction.X), 1, Color.White); // This will be once texture is created
            Raylib.DrawCircle((int)pos.X, (int)pos.Y, 3, Color.Green);
        }

        public Texture2D GetTexture()
        {
            return texture;
        }
        public void Move()
        {
            Vector2 currentPos = new(pos.X + texture.Width / 2, pos.Y + texture.Height / 2);
            
            float frameSpeed = speed / Globals.GetTickRate();
            Vector2 movement = Raymath.Vector2Scale(direction, frameSpeed);
            
            Vector2 nextPos = Raymath.Vector2Add(currentPos, movement);

            //Check collision here
            pos = nextPos;
            
        }

    }
}