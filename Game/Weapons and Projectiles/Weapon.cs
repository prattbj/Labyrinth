using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;
using Labyrinth.Game.Entities.Players;

namespace Labyrinth.Game.Weapons
{
    public abstract class Weapon
    {

        protected float baseCooldown;
        protected float currentCooldown = 0;
        abstract public void Activate(Player player, Vector2 pos, Vector2 direction);
        public void ReduceCooldown()
        {
            if (currentCooldown > 0)
            {
                currentCooldown -= 1.0f / Globals.GetTickRate();
            }
        }
        protected void ResetCooldown(Player player)
        {
            currentCooldown = baseCooldown * player.GetCooldownReduction();
        }
    }
}