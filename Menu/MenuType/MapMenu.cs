using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Labyrinth.Game.Terrain;
using System.Numerics;
using Raylib_cs;
using Labyrinth.Game.Entities;
namespace Labyrinth.Menu.MenuType
{
    public class MapMenu : IMenu
    {
        public void Draw()
        {
            Map? map = Globals.GetGame()?.GetMap();
            Player? player = Globals.GetGame()?.GetPlayer();
            int currentCellKey = 0;
            if (player != null)
            {
                currentCellKey = player.GetCurrentCellKey();
            }
            if (map != null)
            {
                Rectangle bounds = map.GetBounds();

                float screenW = Raylib.GetScreenWidth();
                float screenH = Raylib.GetScreenHeight();

                float scaleX = screenW / bounds.Width;
                float scaleY = screenH / bounds.Height;
                float scale = MathF.Min(scaleX, scaleY); // Uniform scaling

                // Offset to center the map
                Vector2 offset = new(
                    (screenW - bounds.Width * scale) / 2f,
                    (screenH - bounds.Height * scale) / 2f
                );
                
                // foreach ((Vector2 a, Vector2 b) in map.GetPath())
                // {
                //     Vector2 sa = new Vector2((a.X - bounds.X) * scale, (a.Y - bounds.Y) * scale) + offset;
                //     Vector2 sb = new Vector2((b.X - bounds.X) * scale, (b.Y - bounds.Y) * scale) + offset;
                //     Raylib.DrawLineV(sa, sb, Color.Red);
                // }

                foreach (Cell c in map.GetCells())
                {
                    if (!c.IsSeen()) continue;

                    if (c.GetIndex() == currentCellKey)
                    {
                        // Get scaled center
                        Vector2 polyCenter = c.GetPolygon().Aggregate(Vector2.Zero, (sum, v) => sum + v) / c.GetPolygon().Length;
                        Vector2 screenCenter = new Vector2((polyCenter.X - bounds.X) * scale, (polyCenter.Y - bounds.Y) * scale) + offset;

                        Raylib.DrawCircleV(screenCenter, 200 * scale, Color.Red); // Optional: scale the radius too
                    }
                    foreach ((Vector2 a, Vector2 b) in c.GetLines())
                    {
                        // Convert world position to screen position
                        Vector2 sa = new Vector2((a.X - bounds.X) * scale, (a.Y - bounds.Y) * scale) + offset;
                        Vector2 sb = new Vector2((b.X - bounds.X) * scale, (b.Y - bounds.Y) * scale) + offset;

                        Raylib.DrawLineV(sa, sb, c.GetSection() > -1 ? Cell.sectionColors[c.GetSection()] : Color.Beige); // You can customize the color
                    }
                }
            }
        }
        public void HandleInput()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                Menu.SetCurrentMenu(null);
            }
        }

        public void PerformActions()
        {

        }
    }
}