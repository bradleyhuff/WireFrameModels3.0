using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collections.Buckets.Interfaces
{
    public interface IBoxBucket<T> where T : IBox
    {
        T[] Fetch(T input);
        T[] Fetch<G>(G input) where G : IBox;
        T[] Fetch(Rectangle3D box);

        void Add(T box);
        void AddRange(IEnumerable<T> boxes);
    }
}
