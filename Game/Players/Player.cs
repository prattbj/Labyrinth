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
        private int radius = 32;
        public Player(Map map)
        {
            this.map = map;
            Image image = Raylib.LoadImage("./Assets/Textures/Rocks.png");
            Raylib.ImageResize(ref image, radius * 2, radius * 2);
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
            return pos + new Vector2(radius, radius);
        }
        private void Move()
        {
            Cell currentCell = map.GetCell(currentCellKey);

            float playerRadius = radius;
            Vector2 centerOffset = new(radius, radius);

            Vector2 previousCenter = pos + centerOffset;

            pos += movementVector;
            Vector2 newCenter = pos + centerOffset;

            foreach (var (a, b) in currentCell.GetCollisionLines())
            {
                Vector2 closest = ClosestPointOnLineSegment(a, b, newCenter);
                float distSq = Vector2.DistanceSquared(newCenter, closest);

                if (distSq < playerRadius * playerRadius)
                {
                    Vector2 collisionNormal = Vector2.Normalize(newCenter - closest);
                    float penetrationDepth = playerRadius - MathF.Sqrt(distSq);
                    pos += collisionNormal * penetrationDepth;

                    newCenter = pos + centerOffset;
                }
            }

            Vector2 proposedCenter = pos + centerOffset;


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