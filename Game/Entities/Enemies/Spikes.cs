using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Labyrinth.Game.Terrain;
using Raylib_cs;
namespace Labyrinth.Game.Entities.Enemies
{
    public class Spikes : Enemy
    {
        private static readonly Texture2D texture = LoadTexture();
        protected override Texture2D Texture => texture;
        public Spikes(Map map, Dictionary<int, List<Enemy>> enemies, Vector2 pos) : base(map, enemies, pos)
        {
            radius = texture.Height / 2;

        }
        private static Texture2D LoadTexture()
        {
            Image image = Raylib.LoadImage("./Assets/Textures/Rocks.png");
            Raylib.ImageResize(ref image, 32 * 2, 32 * 2);
            Raylib.ImageDrawRectangle(ref image, 0, 0, image.Width, image.Height, Color.Blank);
            Raylib.ImageDrawCircle(ref image, image.Width / 2, image.Height / 2, image.Width / 2, Color.Purple);
            Texture2D tex = Raylib.LoadTextureFromImage(image);
            Raylib.UnloadImage(image);
            return tex;
        }
        protected override void Interact()
        {
            Player? player = Globals.GetGame()?.GetPlayer();
            if (player != null)
            {
                if (Raylib.CheckCollisionCircles(pos, radius, player.GetPos(), player.GetRadius()))
                {
                    player.flatDamage.Add(new(100 / Globals.GetTickRate()));
                }
            }
        }
    }
}