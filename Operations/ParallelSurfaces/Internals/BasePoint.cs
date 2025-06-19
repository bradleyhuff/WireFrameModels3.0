using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.ParallelSurfaces.Internals
{
    internal class BasePoint
    {
        private static int _id = 0;
        private static object lockObject = new object();
        public BasePoint(PositionNormal pn)
        {
            lock (lockObject)
            {
                Id = _id++;
            }
            SurfaceNormal = pn.Normal;
            Position = pn.Position;
            var surfacePlane = new Plane(Point3D.Zero, pn.Normal);
            EdgeNormals = pn.PositionObject.PositionNormals.Where(pn2 => pn2.Id != pn.Id).Select(pn3 => pn3.Normal).Select(pn4 => surfacePlane.Projection(pn4)).ToList();
        }
        public int Id { get; }
        public Vector3D SurfaceNormal { get; }
        public Point3D Position { get; }
        public IReadOnlyList<Vector3D> EdgeNormals { get; }
    }
}
