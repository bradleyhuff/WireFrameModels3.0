using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.Buckets.Interfaces;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshPoint: IBox
    {
        private static int _id = 0;
        public IntermeshPoint(Point3D point) 
        { 
            Point = point;
            Id = _id++;
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

        private List<IntermeshSegment> _segments { get; } = new List<IntermeshSegment>();
        private List<IntermeshTriangle> _triangles = new List<IntermeshTriangle>();
        public IReadOnlyList<IntermeshTriangle> Triangles
        {
            get { return _triangles; }
        }
        public bool Add(IntermeshTriangle triangle)
        {
            if (_triangles.Any(t => t.Id == triangle.Id)) { return false; }
            _triangles.Add(triangle);
            return true;
        }

        public IReadOnlyList<IntermeshSegment> Segments
        {
            get { return _segments; }
        }
        public bool Add(IntermeshSegment segment)
        {
            if (_segments.Any(t => t.Id == segment.Id)) { return false; }
            _segments.Add(segment);
            return true;
        }
    }
}
