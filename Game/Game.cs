

using Labyrinth.Game.Entities;
using Labyrinth.Game.Terrain;
using Labyrinth.Game.Projectiles;
using Raylib_cs;
using Labyrinth.Menu.MenuType;
using System.Numerics;
namespace Labyrinth.Game
{
    public class Game
    {
        Player? player;
        Map map = new();
        List<Projectile> projectiles = [];
        Dictionary<int, List<Enemy>> enemies;
        Camera2D camera;
        
        Shader visibilityShader = Raylib.LoadShader(null, "./Assets/Shaders/visibility.fs");
        RenderTexture2D target = Raylib.LoadRenderTexture((int)Globals.GetScreenSize().X, (int)Globals.GetScreenSize().Y);
        int playerPosLoc;
        int radiusLoc;
        int screenSizeLoc;
        public Game()
        {
            player = new(map);
            camera = new(new(Globals.GetScreenSize().X / 2, Globals.GetScreenSize().Y / 2), player.GetPos(), 0, 0.75f);
            playerPosLoc = Raylib.GetShaderLocation(visibilityShader, "playerPos");
            radiusLoc = Raylib.GetShaderLocation(visibilityShader, "radius");
            screenSizeLoc = Raylib.GetShaderLocation(visibilityShader, "screenSize");

            // Set static values once
            Raylib.SetShaderValue(visibilityShader, screenSizeLoc, Globals.GetScreenSize(), ShaderUniformDataType.Vec2);
        }
        public Map GetMap()
        {
            return map;
        }
        public void Draw()
        {
            if (player != null)
            {
                Vector2 screenPlayerPos = Raylib.GetWorldToScreen2D(player.GetPos(), camera);
                float screenRadius = player.GetVisibilityRadius() * camera.Zoom;
                Raylib.SetShaderValue(visibilityShader, playerPosLoc, screenPlayerPos, ShaderUniformDataType.Vec2);
                Raylib.SetShaderValue(visibilityShader, radiusLoc, screenRadius, ShaderUniformDataType.Float);
            }
            Raylib.BeginTextureMode(target);
            Raylib.ClearBackground(Color.Black);
            Raylib.BeginMode2D(camera);
            player?.Draw();
            foreach (Projectile projectile in projectiles)
            {
                projectile.Draw();
            }
            map.Draw();

            Raylib.EndMode2D();
            Raylib.EndTextureMode();
            Raylib.BeginShaderMode(visibilityShader);
            Raylib.DrawTextureRec(target.Texture, new Rectangle( 0, 0, target.Texture.Width, -target.Texture.Height), new Vector2(0, 0), Color.White);
            Raylib.EndShaderMode();
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
            //TODO: add logic to draw and perform actions. 
            //TODO: We should borrow the same logic from the cell drawing to determine which enemies get to interact based on the cells that are visible to the player.
            // foreach (map.GetCell(player?.GetCurrentCellKey()))
            // {
            //     enemy.Move();
            // }
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