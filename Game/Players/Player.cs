using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Labyrinth.Game.Terrain;
using Labyrinth.Game.Weapons;
using Labyrinth.Game.Weapons.MeleeWeapons;
using Labyrinth.Game.Weapons.ProjectileWeapons;
using Raylib_cs;

namespace Labyrinth.Game.Entities.Players
{
    public class Player
    {
        private Texture2D texture;
        private Vector2 pos;
        //private Vector2? pathEnd;
        private Vector2 movementVector = new(0, 0);
        private float speed = 800;
        private float projectileSpeedMultiplier = 1;
        private float cooldownReduction = 1.0f;
        private Weapon abilityOne = new PeaShooter();
        private Map map;
        private int currentCellKey;
        public Player(Map map)
        {
            this.map = map;
            Image image = Raylib.LoadImage("../Assets/blank.png");
            Raylib.ImageResize(ref image, 64, 64);
            Raylib.ImageDrawRectangle(ref image, 0, 0, image.Width, image.Height, Color.Blank);
            Raylib.ImageDrawCircle(ref image, image.Width / 2, image.Height / 2, image.Width / 2, Color.Blue);
            texture = Raylib.LoadTextureFromImage(image);
            currentCellKey = map.GetSpawnCellKey();
            pos = map.GetCell(currentCellKey).GetPolygon().Aggregate(Vector2.Zero, (sum, v) => sum + v) / map.GetCell(currentCellKey).GetPolygon().Length;// ?? new(0);

        }

        
        public System.Numerics.Vector2 GetPos()
        {
            return pos;
        }
        public int GetCurrentCellKey()
        {
            return currentCellKey;
        }
        public void Draw()
        {
            Raylib.DrawTextureEx(texture, pos, 0, 1, Color.White);
        }
        public void HandleInputs(Camera2D camera)
        {
            HandleMovementInput(camera);
            HandleAbilityOneInput(camera);
        }
        private void HandleMovementInput(Camera2D camera)
        {
            movementVector = new System.Numerics.Vector2(0, 0);

            if (Raylib.IsKeyDown(KeyboardKey.W))
                movementVector.Y -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.S))
                movementVector.Y += 1;
            if (Raylib.IsKeyDown(KeyboardKey.A))
                movementVector.X -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.D))
                movementVector.X += 1;

            movementVector = Raymath.Vector2Normalize(movementVector);
            movementVector = Raymath.Vector2Scale(movementVector, speed / Globals.GetTickRate());
            //#region <Mouse Movement Input>
            /*
            if (Raylib.IsMouseButtonDown(MouseButton.Right))
            {
                Vector2 mousePos = Raylib.GetMousePosition();
                Vector2 worldMousePos = Raylib.GetScreenToWorld2D(mousePos, camera);
                pathEnd = worldMousePos;
            }
            */
            //#endregion
        }

        private void HandleAbilityOneInput(Camera2D camera)
        {
            if (Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                System.Numerics.Vector2 mousePos = Raylib.GetMousePosition();
                System.Numerics.Vector2 worldMousePos = Raylib.GetScreenToWorld2D(mousePos, camera);
                abilityOne.Activate(this, new(pos.X + texture.Width / 2, pos.Y + texture.Height / 2), Raymath.Vector2Normalize(Raymath.Vector2Subtract(worldMousePos, pos)));
            }
        }
        public void PerformActions()
        {
            Move();
            ReduceCooldowns();
        }
        private void ReduceCooldowns()
        {
            abilityOne.ReduceCooldown();
        }

        private void Move()
        {
            Cell currentCell = map.GetCell(currentCellKey);
            
            float playerRadius = texture.Width / 2f;
            Vector2 centerOffset = new Vector2(playerRadius, texture.Height / 2f);
            // Before updating pos
            Vector2 previousCenter = pos + centerOffset;
            pos += movementVector;
            Vector2 proposedCenter = pos + centerOffset;

            foreach (var (a, b, cellA, cellB) in currentCell.GetLinkedLines())
            {
                if (LinesIntersect(previousCenter, proposedCenter, a, b))
                {
                    // You're moving from currentCellKey to the other cell
                    currentCellKey = (currentCellKey == cellA) ? cellB : cellA;
                    break;
                }
            }
            
            // float playerRadius = texture.Width / 2f;
            // Vector2 centerOffset = new Vector2(playerRadius, texture.Height / 2f);
            // Vector2 proposedCenter = pos + movementVector + centerOffset;
            // List<List<Vector2>> polysToCheck = [];
            // foreach (var (poly, _) in map.GetWallEdges())
            // {
            //     foreach (var p in poly)
            //     {
            //         if (Raylib.CheckCollisionPointCircle(p, Globals.GetGame()?.GetPlayer()?.GetPos() ?? new(0, 0), 3000))
            //         {
            //             polysToCheck.Add(poly);
            //             break;
            //         }
            //     }
            // }
            // // Find all edges we're colliding with
            // List<(Vector2 a, Vector2 b)> collidedEdges = new();

            // // Check which polygon edges the player collides with
            // foreach (var polygon in polysToCheck)
            // {
            //     int count = polygon.Count;
            //     for (int i = 0; i < count; i++)
            //     {
            //         var a = polygon[i];
            //         var b = polygon[(i + 1) % count];

            //         if (Raylib.CheckCollisionCircleLine(proposedCenter, playerRadius, a, b))
            //         {
            //             collidedEdges.Add((a, b));
            //         }
            //     }
            // }

            // // If no collisions, move freely
            // if (collidedEdges.Count == 0)
            // {
            //     pos += movementVector;
            //     return;
            // }

            // // Try sliding along the direction of each collided wall edge
            // foreach (var (a, b) in collidedEdges)
            // {
            //     Vector2 wallDir = Vector2.Normalize(b - a);

            //     // Project movement onto wall direction (sliding vector)
            //     float dot = Vector2.Dot(movementVector, wallDir);
            //     Vector2 slideVector = wallDir * dot;

            //     Vector2 slidePos = pos + slideVector;
            //     Vector2 slideCenter = slidePos + centerOffset;

            //     bool blocked = false;

            //     // Recheck all edges for collisions at the new slide position
            //     foreach (var (polygon, _) in map.GetWallEdges())
            //     {
            //         int count = polygon.Count;
            //         for (int i = 0; i < count; i++)
            //         {
            //             var ea = polygon[i];
            //             var eb = polygon[(i + 1) % count];

            //             if (Raylib.CheckCollisionCircleLine(slideCenter, playerRadius, ea, eb))
            //             {
            //                 blocked = true;
            //                 break;
            //             }
            //         }

            //         if (blocked)
            //             break;
            //     }

            //     if (!blocked)
            //     {
            //         pos = slidePos;
            //         return; // Sliding succeeded
            //     }
            // }

            // AttemptAxisSeparatedMove(playerRadius, centerOffset);

            //#region <Mouse Movement>
            /* Movement based on mouse
            if (pathEnd != null)
            {
                Vector2 currentPos = new(pos.X + texture.Width / 2, pos.Y + texture.Height / 2);

                Vector2 direction = Raymath.Vector2Subtract(pathEnd.Value, currentPos);

                direction = Raymath.Vector2Normalize(direction);

                float frameSpeed = speed / Globals.GetTickRate();
                Vector2 movement = Raymath.Vector2Scale(direction, frameSpeed);

                Vector2 nextPos = Raymath.Vector2Add(currentPos, movement);
                if (Raylib.CheckCollisionPointLine(pathEnd.Value, currentPos, nextPos, 1))
                {
                    pos.X = pathEnd.Value.X - texture.Width / 2;
                    pos.Y = pathEnd.Value.Y - texture.Height / 2;
                }
                else
                {
                    pos.X += movement.X;
                    pos.Y += movement.Y;
                }
            }
            */
            //#endregion
        }
        // private void AttemptAxisSeparatedMove(float radius, Vector2 centerOffset)
        // {
        //     // Try X-only
        //     Vector2 tryX = pos + new Vector2(movementVector.X, 0);
        //     Vector2 centerX = tryX + centerOffset;
        //     bool xBlocked = false;

        //     foreach (var edge in map.GetWallEdges())
        //     {
        //         var (a, b) = edge.edge;
        //         if (Raylib.CheckCollisionCircleLine(centerX, radius, a, b))
        //         {
        //             xBlocked = true;
        //             break;
        //         }
        //     }

        //     if (!xBlocked)
        //     {
        //         pos.X = tryX.X;
        //     }

        //     // Try Y-only
        //     Vector2 tryY = pos + new Vector2(0, movementVector.Y);
        //     Vector2 centerY = tryY + centerOffset;
        //     bool yBlocked = false;

        //     foreach (var edge in map.GetWallEdges())
        //     {
        //         var (a, b) = edge.edge;
        //         if (Raylib.CheckCollisionCircleLine(centerY, radius, a, b))
        //         {
        //             yBlocked = true;
        //             break;
        //         }
        //     }

        //     if (!yBlocked)
        //     {
        //         pos.Y = tryY.Y;
        //     }
        // }
        // private void AttemptAxisSeparatedMove(float radius, Vector2 centerOffset)
        // {
        //     // --- Try X-only movement ---
        //     Vector2 tryX = pos + new Vector2(movementVector.X, 0);
        //     Vector2 centerX = tryX + centerOffset;
        //     bool xBlocked = false;
        //     List<List<Vector2>> polysToCheck = [];
        //     foreach (var (poly, _) in map.GetWallEdges())
        //     {
        //         foreach (var p in poly)
        //         {
        //             if (Raylib.CheckCollisionPointCircle(p, Globals.GetGame()?.GetPlayer()?.GetPos() ?? new(0, 0), 3000))
        //             {
        //                 polysToCheck.Add(poly);
        //                 break;
        //             }
        //         }
        //     }
        //     foreach (var polygon in polysToCheck)
        //     {
        //         int count = polygon.Count;
        //         for (int i = 0; i < count; i++)
        //         {
        //             var a = polygon[i];
        //             var b = polygon[(i + 1) % count];

        //             if (Raylib.CheckCollisionCircleLine(centerX, radius, a, b))
        //             {
        //                 xBlocked = true;
        //                 break;
        //             }
        //         }

        //         if (xBlocked) break;
        //     }

        //     if (!xBlocked)
        //     {
        //         pos.X = tryX.X;
        //     }

        //     // --- Try Y-only movement ---
        //     Vector2 tryY = pos + new Vector2(0, movementVector.Y);
        //     Vector2 centerY = tryY + centerOffset;
        //     bool yBlocked = false;

        //     foreach (var polygon in polysToCheck)
        //     {
        //         int count = polygon.Count;
        //         for (int i = 0; i < count; i++)
        //         {
        //             var a = polygon[i];
        //             var b = polygon[(i + 1) % count];

        //             if (Raylib.CheckCollisionCircleLine(centerY, radius, a, b))
        //             {
        //                 yBlocked = true;
        //                 break;
        //             }
        //         }

        //         if (yBlocked) break;
        //     }

        //     if (!yBlocked)
        //     {
        //         pos.Y = tryY.Y;
        //     }
        // }

        static bool LinesIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
        {
            float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;

            Vector2 r = p2 - p1;
            Vector2 s = q2 - q1;
            float denom = Cross(r, s);

            if (denom == 0) return false; // Parallel lines

            float u = Cross(q1 - p1, r) / denom;
            float t = Cross(q1 - p1, s) / denom;

            return t >= 0 && t <= 1 && u >= 0 && u <= 1;
        }
        public float GetSpeedMultiplier()
        {
            return projectileSpeedMultiplier;
        }

        public float GetCooldownReduction()
        {
            return cooldownReduction;
        }
    }
}