using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Labyrinth.Game.Entities;
using Labyrinth.Game.Projectiles;

namespace Labyrinth.Game.Weapons.ProjectileWeapons
{
    abstract public class ProjectileWeapon : Weapon
    {
        protected float projectileSpeed;
        override public void Activate(Player player, Vector2 pos, Vector2 direction)
        {
            if (currentCooldown <= 0)
            {
                Globals.GetGame()?.AddProjectile(CreateProjectile(player, pos, direction));
                ResetCooldown(player);
            }
            
        }

        //Would love to do a T for an easier time doing this, but this works as well.
        abstract protected Projectile CreateProjectile(Player player, Vector2 pos, Vector2 direction);
    }
}