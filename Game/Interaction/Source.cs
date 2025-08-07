using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Numerics;
namespace Labyrinth.Game.Interaction
{
    public abstract class Source<T>(T value, int persistence = 1)
    {
        protected T value = value;
        protected abstract void ApplySource(ref T value);
        protected int persistence = persistence;
        public static void ApplySourceCollection<F>(ref T value, List<F> collection) where F : Source<T>
        {
            foreach (Source<T> source in collection)
            {
                source.ApplySource(ref value);
                source.persistence -= 1;
            }
            collection.RemoveAll(s => s.persistence <= 0);
        }
        
    }
}