using BasicObjects.GeometricObjects;
using Collections.Buckets.Interfaces;
using Collections.Buckets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.ParallelSurfaces.Internals
{
    internal class SurfacePointNode : IBox
    {
        public SurfacePointNode(Point3D point, int circuit, int index)
        {
            Point = point;
            Circuit = circuit;
            Index = index;
        }

        public Point3D Point { get; }
        public int Circuit { get; }
        public int Index { get; }

        private Rectangle3D _box;
        public Rectangle3D Box
        {
            get
            {
                if (_box is null && Point is not null)
                {
                    _box = new Rectangle3D(Point, BoxBucket.MARGINS);
                }
                return _box;
            }
        }
    }
}
