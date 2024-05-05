using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshIntersectionSet
    {
        private static int _id = 0;

        public IntermeshIntersectionSet()
        {
            Id = _id++;
        }
        public int Id { get; }
        public bool IsSet { get; set; }

        public IntermeshIntersection[] Intersections { get; set; }
    }
}
