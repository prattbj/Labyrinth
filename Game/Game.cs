

using Labyrinth.Game.Entities.Players;
using Labyrinth.Game.Terrain;
using Labyrinth.Game.Projectiles;
using Raylib_cs;
using Labyrinth.Menu.MenuType;

namespace Labyrinth.Game
{
    public class Game
    {
        Player? player;
        Map map = new();
        List<Projectile> projectiles = [];
        Camera2D camera;
        Texture2D checkerboard;
        public Game()
        {
            player = new(map);
            camera = new(new(Globals.GetScreenSize().X / 2, Globals.GetScreenSize().Y / 2), player.GetPos(), 0, 1.0f);
            Image image = Raylib.LoadImage("../Assets/Checkerboard_pattern.svg.png");
            checkerboard = Raylib.LoadTextureFromImage(image);
            Raylib.UnloadImage(image);
        }
        public Map GetMap()
        {
            return map;
        }
        public void Draw()
        {
            Raylib.BeginMode2D(camera);
            //Raylib.DrawTexture(checkerboard, 0, 0, Color.White);
            player?.Draw();
            foreach (Projectile projectile in projectiles)
            {
                projectile.Draw();
            }
            map.Draw();
            Raylib.EndMode2D();
            //Raylib.DrawText($"{player?.GetPos().X ?? 0}, {player?.GetPos().Y ?? 0}", 0, 0, 16, Color.Brown);
        }

        public void HandleInput()
        {
            player?.HandleInputs(camera);
            if (Raylib.IsKeyPressed(KeyboardKey.M))
            {
                Menu.Menu.SetCurrentMenu(new MapMenu());
            }
            //map.Debug(camera);
        }

        public void PerformActions()
        {
            player?.PerformActions();
            camera.Offset = new(Globals.GetScreenSize().X / 2, Globals.GetScreenSize().Y / 2);
            camera.Target = player?.GetPos() ?? new(Globals.GetScreenSize().X / 2, Globals.GetScreenSize().Y / 2);
            foreach (Projectile projectile in projectiles)
            {
                projectile.Move();
            }

            float scroll = Raylib.GetMouseWheelMove();

            if (scroll != 0)
            {
                // Sensitivity factor — tweak this for feel
                float zoomFactor = 1.1f;

                if (scroll > 0)
                    camera.Zoom *= zoomFactor;
                else
                    camera.Zoom /= zoomFactor;

                // Clamp to a sane range (to avoid flipping or infinitesimal zoom)
                camera.Zoom = Math.Clamp(camera.Zoom, 0.01f, 100.0f);
            }
        }
        public Camera2D GetCamera2D()
        {
            return camera;
        }
        public Player? GetPlayer()
        {
            return player;
        }

        public void AddProjectile(Projectile projectile)
        {
            projectiles.Add(projectile);
        }
    }
}