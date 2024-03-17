using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.SurfaceSegmentChaining.Basics
{
    internal class SurfaceSegmentContainer<T>
    {
        public SurfaceSegmentContainer(SurfaceRayContainer<T> a, SurfaceRayContainer<T> b)
        {
            A = a;
            B = b;
            Segment = new SurfaceLineSegment(a, b);
        }

        public SurfaceLineSegment Segment { get; }

        public SurfaceRayContainer<T> A { get; }
        public SurfaceRayContainer<T> B { get; }
    }
}
