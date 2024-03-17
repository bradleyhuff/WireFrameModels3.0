using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.SurfaceSegmentChaining.Basics
{
    internal class LinkedSurfaceSegment<G> where G : class
    {
        public LinkedSurfaceSegment(Ray3D a, Ray3D b, Rank rank, int groupKey = 0, G groupObject = null)
        {
            A = a;
            B = b;
            Rank = rank;
            GroupKey = groupKey;
            GroupObject = groupObject;
        }

        public int GroupKey { get; }
        public G GroupObject { get; }
        public Ray3D A { get; }
        public Ray3D B { get; }
        public Rank Rank { get; }
    }
}
