using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.ParallelSurfaces.Internals
{
    internal class Quadrangle
    {
        private static int _id = 0;
        private static object lockObject = new object();

        public Quadrangle(IPerimeterChainLink link)
        {
            lock (lockObject)
            {
                Id = _id++;
            }

            BaseA = link.A;
            BaseB = link.B;
            NormalA = link.BitangentA;
            NormalB = link.BitangentB;
        }
        public int Id { get; }
        public Point3D BaseA { get; }
        public Point3D BaseB { get; }
        public Point3D SurfaceA { get; set; }
        public Point3D SurfaceB { get; set; }

        public Vector3D NormalA { get; }
        public Vector3D NormalB { get; }
    }
}
