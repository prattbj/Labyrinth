using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;
namespace Labyrinth.Game.Interaction
{
    public class AdditiveSource(float value, int persistence = 1, bool perTick = false) : Source<float>(value / (perTick ? Globals.GetTickRate() : 1), persistence)
    {
        protected override void ApplySource(ref float value)
        {
            value += this.value;
        }
    }
}