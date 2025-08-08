using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Labyrinth.Game.Terrain;
using Labyrinth.Game.Entities.Weapons;
using Labyrinth.Game.Interaction;
using Labyrinth.Game.Entities.Weapons.MeleeWeapons;
using Labyrinth.Game.Entities.Weapons.ProjectileWeapons;
using Raylib_cs;
using System.Security.Cryptography.X509Certificates;

namespace Labyrinth.Game.Entities
{
    public class Player : Interactable
    {
        private Texture2D texture;
        private Vector2 pos;
        //private Vector2? pathEnd;
        private Vector2 movementVector = new(0, 0);
        //private float speed = 800;
        //private float projectileSpeedMultiplier = 1;
        private float cooldownReduction = 1.0f;
        private Weapon abilityOne = new PeaShooter();
        private Map map;
        private int currentCellKey;
        private int radius = 32;
        private int visibilityRadius = 1000;
        public Player(Map map)
        {
            this.map = map;
            Image image = Raylib.LoadImage("./Assets/Textures/Rocks.png");
            Raylib.ImageResize(ref image, radius * 2, radius * 2);
            Raylib.ImageDrawRectangle(ref image, 0, 0, image.Width, image.Height, Color.Blank);
            Raylib.ImageDrawCircle(ref image, image.Width / 2, image.Height / 2, image.Width / 2, Color.Blue);
            texture = Raylib.LoadTextureFromImage(image);
            Raylib.UnloadImage(image);
            currentCellKey = map.GetSpawnCellKey();
            pos = map.GetCell(currentCellKey).GetPolygon().Aggregate(Vector2.Zero, (sum, v) => sum + v) / map.GetCell(currentCellKey).GetPolygon().Length;// ?? new(0);

        }

        public int GetRadius()
        {
            return radius;
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
            Raylib.DrawTextureEx(texture, pos - new Vector2(radius, radius), 0, 1, Color.White);
            Raylib.DrawText(currentHealth.ToString(), (int)pos.X, (int)pos.Y, 16, Color.Pink);
        }
        public void HandleInputs(Camera2D camera)
        {
            HandleMovementInput(camera);
            HandleAbilityOneInput(camera);
        }
        private void HandleMovementInput(Camera2D camera)
        {
            movementVector = new Vector2(0, 0);

            if (Raylib.IsKeyDown(KeyboardKey.W))
                movementVector.Y -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.S))
                movementVector.Y += 1;
            if (Raylib.IsKeyDown(KeyboardKey.A))
                movementVector.X -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.D))
                movementVector.X += 1;

            movementVector = Raymath.Vector2Normalize(movementVector);
            movementVector = Raymath.Vector2Scale(movementVector, GetSpeed() / Globals.GetTickRate());
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
                Vector2 mousePos = Raylib.GetMousePosition();
                Vector2 worldMousePos = Raylib.GetScreenToWorld2D(mousePos, camera);
                abilityOne.Activate(this, Raymath.Vector2Normalize(Raymath.Vector2Subtract(worldMousePos, pos)));
            }
        }
        public void PerformActions()
        {
            Move();
            ReduceCooldowns();
            ApplyRegen();
            if (ApplyDamage())
            {
                Menu.Menu.SetCurrentMenu(new Menu.MenuType.DeathMenu());
            }
        }
        private void ReduceCooldowns()
        {
            abilityOne.ReduceCooldown();
        }
        Vector2 ClosestPointOnLineSegment(Vector2 a, Vector2 b, Vector2 point)
        {
            Vector2 ab = b - a;
            float abLengthSq = ab.LengthSquared();
            if (abLengthSq == 0) return a;

            float t = Vector2.Dot(point - a, ab) / abLengthSq;
            t = Math.Clamp(t, 0f, 1f);
            return a + ab * t;
        }
        public Vector2 GetCenter()
        {
            return pos;
        }
        private void Move()
        {
            Cell currentCell = map.GetCell(currentCellKey);

           

            Vector2 previousCenter = pos;

            pos += movementVector + GetDisplacement();
            Vector2 newCenter = pos;

            foreach (var (a, b) in currentCell.GetCollisionLines())
            {
                Vector2 closest = ClosestPointOnLineSegment(a, b, newCenter);
                float distSq = Vector2.DistanceSquared(newCenter, closest);

                if (distSq < radius * radius)
                {
                    Vector2 collisionNormal = Vector2.Normalize(newCenter - closest);
                    float penetrationDepth = radius - MathF.Sqrt(distSq);
                    pos += collisionNormal * penetrationDepth;

                    newCenter = pos;
                }
            }

            Vector2 proposedCenter = pos;


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
        // public float GetSpeedMultiplier()
        // {
        //     return projectileSpeedMultiplier;
        // }

        public float GetCooldownReduction()
        {
            return cooldownReduction;
        }

        public int GetVisibilityRadius()
        {
            return visibilityRadius;
        }
    }
}