using BasicObjects.GeometricObjects;
using Collections.Buckets.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.ParallelSurfaces.Internals
{
    public class PointNode : IBox
    {
        private static int _id = 0;
        private static object lockObject = new object();

        public PointNode(Point3D point)
        {
            Point = point;
            lock (lockObject)
            {
                Id = _id++;
            }
        }

        public int Id { get; }
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
