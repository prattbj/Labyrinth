using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Labyrinth.Game.Entities;
using Labyrinth.Game.Entities.Projectiles;

namespace Labyrinth.Game.Entities.Weapons.ProjectileWeapons
{
    abstract public class ProjectileWeapon<T> : Weapon
    {
        protected float projectileSpeed;
        override public void Activate(Player player, Vector2 direction)
        {
            if (currentCooldown <= 0)
            {
                Globals.GetGame()?.AddProjectile(CreateProjectile(player,direction));
                ResetCooldown(player);
            }

        }

        //Would love to do a T for an easier time doing this, but this works as well.
        abstract protected Projectile CreateProjectile(Player player, Vector2 direction);
        // protected Projectile CreateProjectile(Player player, Vector2 pos, Vector2 direction, int currentCellKey) where T : Projectile
        // {
        //     return new T(pos, direction, projectileSpeed, currentCellKey);
        // }
    }
}