using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Labyrinth.Game.Interaction
{
    public class Vector2Source(Vector2 value, int persistence = 1) : Source<Vector2>(value, persistence)
    {
        protected override void ApplySource(ref Vector2 value)
        {
            value += this.value;
        }
    }
}