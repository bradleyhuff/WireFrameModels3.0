using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics.V2
{
    internal class IntermeshIntersection
    {
        private static int _id = 0;
        public IntermeshIntersection()
        {
            Id = _id++;
        }
        public int Id { get; }
        public bool IsSet { get; set; }
        public LineSegment3D[] Intersections { get; set; }
    }
}
