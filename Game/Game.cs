

using Labyrinth.Game.Entities.Enemies;
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
        List<Enemy> enemies = [];
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
            foreach (var cell in map.GetCells())
            {
                if (cell.GetIndex() != player.GetCurrentCellKey())
                    enemies.Add(new Slime(map, enemies, cell.GetCenter(), cell.GetIndex()));
            }
            // Set static values once
            Raylib.SetShaderValue(visibilityShader, screenSizeLoc, Globals.GetScreenSize(), ShaderUniformDataType.Vec2);
        }
        public Map GetMap()
        {
            return map;
        }
        public void AddEnemy(Enemy enemy)
        {
            enemies.Add(enemy);
        }
        public List<Enemy> GetEnemies(int index)
        {
            return enemies.Where(e => e.GetCurrentCellKey() == index).ToList();
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
            Raylib.DrawTextureRec(target.Texture, new Rectangle(0, 0, target.Texture.Width, -target.Texture.Height), new Vector2(0, 0), Color.White);
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
        bool IsLineOnScreen(Vector2 a, Vector2 b)
        {
            Vector2 screenSize = Globals.GetScreenSize(); // Or new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight())

            float zoom = 1f;

            // Convert screen rect (centered around camera.target) to world-space rectangle
            float halfWidth = screenSize.X / 2f / zoom;
            float halfHeight = screenSize.Y / 2f / zoom;

            Vector2 topLeft = new Vector2(camera.Target.X - halfWidth, camera.Target.Y - halfHeight);
            Rectangle screenRect = new Rectangle(topLeft.X, topLeft.Y, halfWidth * 2, halfHeight * 2);
            return Raylib.CheckCollisionRecs(screenRect, GetBoundingBox(a, b));
        }

        Rectangle GetBoundingBox(Vector2 a, Vector2 b)
        {
            float minX = MathF.Min(a.X, b.X);
            float minY = MathF.Min(a.Y, b.Y);
            float maxX = MathF.Max(a.X, b.X);
            float maxY = MathF.Max(a.Y, b.Y);
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
        public void PerformActions()
        {
            if (player != null)
            {
                player.PerformActions();

                HashSet<int> visibleCellKeys = new();
                Queue<int> toProcess = new();

                int startKey = player.GetCurrentCellKey();
                visibleCellKeys.Add(startKey);
                toProcess.Enqueue(startKey);

                while (toProcess.Count > 0)
                {
                    int currentKey = toProcess.Dequeue();
                    Cell current = map.GetCell(currentKey);

                    foreach (var (a, b, i1, i2) in current.GetLinkedLines())
                    {
                        if (IsLineOnScreen(a, b))
                        {
                            // Add both ends of the line
                            foreach (int neighborKey in new[] { i1, i2 })
                            {
                                if (!visibleCellKeys.Contains(neighborKey))
                                {
                                    visibleCellKeys.Add(neighborKey);
                                    toProcess.Enqueue(neighborKey);
                                }
                            }
                        }
                    }
                }
                foreach (var enemy in enemies)
                {
                    int cellKey = enemy.GetCurrentCellKey();
                    if (visibleCellKeys.Contains(cellKey))
                    {
                        enemy.PerformActions();
                    }
                }
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