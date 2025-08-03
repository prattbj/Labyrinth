using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Labyrinth.Game.Entities;
using Labyrinth.Game.Projectiles;

namespace Labyrinth.Game.Weapons.ProjectileWeapons
{
    public class PeaShooter : ProjectileWeapon
    {
        public PeaShooter()
        {
            baseCooldown = .5f;
            projectileSpeed = 600.0f;
        }

        override protected Projectile CreateProjectile(Player player, Vector2 pos, Vector2 direction)
        {
            return new PeaShooterProjectile(pos, direction, projectileSpeed);
        }
    }
}

namespace Labyrinth.Game.Projectiles
{
    public class PeaShooterProjectile : Projectile
    {
        
        public PeaShooterProjectile(Vector2 pos, Vector2 direction, float speed) : base(pos, direction)
        {
            this.speed = speed;//(Globals.GetGame()?.GetPlayer()?.GetSpeedMultiplier() ?? 1.0f) * speed;
        }
    }
}