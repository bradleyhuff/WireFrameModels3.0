using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.SurfaceSegmentChaining.Basics
{
    internal class SurfaceTriangleContainer<T>
    {
        public SurfaceTriangleContainer(SurfaceRayContainer<T> a, SurfaceRayContainer<T> b, SurfaceRayContainer<T> c)
        {
            A = a;
            B = b;
            C = c;
            Triangle = new SurfaceTriangle(a, b, c);
        }
        public SurfaceTriangle Triangle { get; }

        public SurfaceRayContainer<T> A { get; }
        public SurfaceRayContainer<T> B { get; }
        public SurfaceRayContainer<T> C { get; }
    }
}
