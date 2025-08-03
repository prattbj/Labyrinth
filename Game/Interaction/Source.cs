using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Numerics;
namespace Labyrinth.Game.Interaction
{
    public abstract class Source<T>(T value)
    {
        protected T value = value;
        protected abstract void ApplySource(ref T value);

        public static void ApplySourceCollection<F>(ref T value, List<F> collection) where F : Source<T>
        {
            foreach (Source<T> source in collection)
            {
                source.ApplySource(ref value);
            }
            collection.Clear();
        }
    }
}