using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Labyrinth.Game.Entities;
using Labyrinth.Game.Entities.Projectiles;

namespace Labyrinth.Game.Entities.Weapons.ProjectileWeapons
{
    public class PeaShooter : ProjectileWeapon<PeaShooterProjectile>
    {
        public PeaShooter()
        {
            baseCooldown = .5f;
            projectileSpeed = 600.0f;
        }

        override protected Projectile CreateProjectile(Player player, Vector2 direction)
        {
            return new PeaShooterProjectile(player.GetPos(), direction, projectileSpeed, player.GetCurrentCellKey());
        }
    }
}

namespace Labyrinth.Game.Entities.Projectiles
{
    public class PeaShooterProjectile(Vector2 pos, Vector2 direction, float speed, int currentCellKey) : Projectile(pos, direction, speed, currentCellKey)
    {
    }
}