using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Labyrinth.Game.Terrain;
using Raylib_cs;
namespace Labyrinth.Game.Entities.Enemies
{
    public class Spikes : Enemy
    {
        public Spikes(Map map, Dictionary<int, List<Enemy>> enemies) : base(map, enemies)
        {
            Image image = Raylib.LoadImage("./Assets/Textures/Rocks.png");
            Raylib.ImageResize(ref image, radius * 2, radius * 2);
            Raylib.ImageDrawRectangle(ref image, 0, 0, image.Width, image.Height, Color.Blank);
            Raylib.ImageDrawCircle(ref image, image.Width / 2, image.Height / 2, image.Width / 2, Color.Blue);
            texture = Raylib.LoadTextureFromImage(image);
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