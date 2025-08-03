using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;
namespace Labyrinth.Game.Interaction
{
    public class MultiplicativeSource(float value) : Source<float>(value)
    {
        protected override void ApplySource(ref float value)
        {
            value *= this.value;
        }

        public static void ApplyCollectionToBaseValue(ref float value, in float baseValue, List<MultiplicativeSource> collection)
        {
            foreach (MultiplicativeSource source in collection)
            {
                value += source.value * baseValue;

            }
            collection.Clear();
        }
    }
}