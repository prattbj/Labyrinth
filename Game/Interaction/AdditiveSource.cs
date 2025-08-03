using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;
namespace Labyrinth.Game.Interaction
{
    public class AdditiveSource(float value) : Source<float>(value)
    {
        protected override void ApplySource(ref float value)
        {
            value += this.value;
        }
    }
}