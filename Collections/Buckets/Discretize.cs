using BasicObjects.GeometricObjects;
using Collections.Buckets.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Collections.Buckets
{
    public class Discretize<S, T> where T : IBox
    {
        BoxBucket<T> _boxBucket;
        Func<S, Rectangle3D> _inputConversion;
        Func<S, T, bool> _equals;
        Func<S, T> _create;

        public Discretize(Func<S, Rectangle3D> inputConversion, Func<S, T, bool> equals, Func<S, T> create) 
        {
            _boxBucket = new BoxBucket<T>();
            _inputConversion = inputConversion;
            _equals = equals;
            _create = create;
        }

        public T Fetch(S input)
        {
            var match = _boxBucket.Fetch(_inputConversion(input)).FirstOrDefault(m => _equals(input, m));
            if (match is null)
            {
                T add = _create(input);
                _boxBucket.Add(add);
                return add;
            }
            return match;
        }
    }
}
