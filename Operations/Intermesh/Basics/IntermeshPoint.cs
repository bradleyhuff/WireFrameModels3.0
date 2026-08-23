using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.Buckets.Interfaces;
using Collections.WireFrameMesh.Basics;

namespace Operations.Intermesh.Basics
{
    public class IntermeshPoint : IBox, IId
    {
        private static int _id = 0;
        private static object lockObject = new object();
        public IntermeshPoint(Point3D point)
        {
            Point = point;
            lock (lockObject)
            {
                Id = _id++;
            }
        }

        public int Id { get; }
        public Point3D Point { get; }

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
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
