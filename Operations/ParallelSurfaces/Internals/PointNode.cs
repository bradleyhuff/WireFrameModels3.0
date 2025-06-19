using BasicObjects.GeometricObjects;
using Collections.Buckets.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.ParallelSurfaces.Internals
{
    internal class PointNode : IBox
    {
        public PointNode(Point3D point)
        {
            Point = point;
        }
        public Point3D Point { get; }

        private Rectangle3D _box = null;
        public Rectangle3D Box
        {
            get
            {
                if (_box is null)
                {
                    _box = Point.Margin(1e-6);
                }
                return _box;
            }
        }
    }
}
