using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshIntersection
    {
        private static int _id = 0;
        private static object lockObject = new object();
        public IntermeshIntersection()
        {
            lock (lockObject)
            {
                Id = _id++;
            }
        }
        public int Id { get; }
        public bool IsSet { get; set; }
        public LineSegment3D[] Intersections { get; set; }
        public Triangle3D IntersectedTriangle { get; set; }
        public Triangle3D GatheringTriangle { get; set; }
    }
}
