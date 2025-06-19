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
        private static int _id = 0;
        private static object lockObject = new object();
        public SurfaceTriangleContainer(SurfaceRayContainer<T> a, SurfaceRayContainer<T> b, SurfaceRayContainer<T> c, int fillId)
        {
            A = a;
            B = b;
            C = c;
            Triangle = new SurfaceTriangle(a, b, c);
            lock (lockObject)
            {
                Id = _id++;
            }
            FillId = fillId;
        }

        public int Id { get; }
        public int FillId { get; }
        public SurfaceTriangle Triangle { get; }

        public SurfaceRayContainer<T> A { get; }
        public SurfaceRayContainer<T> B { get; }
        public SurfaceRayContainer<T> C { get; }
    }
}
