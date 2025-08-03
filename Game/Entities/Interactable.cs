using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Labyrinth.Game.Interaction;

namespace Labyrinth.Game.Entities
{
    public abstract class Interactable(float baseSpeed = 800, float baseRegen = .2f, float maxHealth = 1000)
    {
        float baseSpeed = baseSpeed;
        internal List<MultiplicativeSource> speedMultiplier = [];
        internal List<AdditiveSource> bonusSpeed = [];
        protected float GetSpeed()
        {
            float speed = baseSpeed;
            Source<float>.ApplySourceCollection(ref speed, bonusSpeed);
            Source<float>.ApplySourceCollection(ref speed, speedMultiplier);
            return speed;
        }
        internal List<Vector2Source> displacement = [];
        protected Vector2 GetDisplacement()
        {
            Vector2 d = new();
            Source<Vector2>.ApplySourceCollection(ref d, displacement);
            return d;
        }
        float baseRegen = baseRegen;
        internal List<AdditiveSource> bonusRegen = [];
        internal List<MultiplicativeSource> percentBonusRegen = [];
        internal List<MultiplicativeSource> percentBaseHealthRegen = [];
        float maxHealth = maxHealth;
        float currentHealth = maxHealth;
        protected void ApplyRegen()
        {
            float regen = baseRegen;
            MultiplicativeSource.ApplyCollectionToBaseValue(ref currentHealth, maxHealth, percentBaseHealthRegen);
            Source<float>.ApplySourceCollection(ref regen, bonusRegen);
            Source<float>.ApplySourceCollection(ref regen, percentBonusRegen);
            currentHealth = currentHealth > maxHealth ? maxHealth : currentHealth;

        }
        internal List<MultiplicativeSource> percentDamageReduction = [];
        internal List<AdditiveSource> flatDamageReduction = [];
        internal List<AdditiveSource> flatDamage = [];
        internal List<MultiplicativeSource> percentMaxHealthDamage = [];
        internal List<MultiplicativeSource> percentCurrentHealthDamage = [];
        protected bool ApplyDamage()
        {
            float current = currentHealth;
            MultiplicativeSource.ApplyCollectionToBaseValue(ref currentHealth, currentHealth, percentCurrentHealthDamage);
            Source<float>.ApplySourceCollection(ref currentHealth, percentMaxHealthDamage);
            Source<float>.ApplySourceCollection(ref currentHealth, flatDamage);
            float delta = current - currentHealth;
            float oldDelta = delta;
            //We store the total damage dealt (delta) this frame so that we can apply percent reductions to it
            Source<float>.ApplySourceCollection(ref delta, percentDamageReduction);
            //Restore the percent reduced damage
            currentHealth += oldDelta + delta;

            float damageReduction = 0;
            Source<float>.ApplySourceCollection(ref damageReduction, flatDamageReduction);
            //total flat damage reduction is calculated by each damage source * flat damage reduction value
            currentHealth += damageReduction * (percentCurrentHealthDamage.Count + percentMaxHealthDamage.Count + flatDamage.Count);

            return currentHealth < 0;
        }

        

    }
}