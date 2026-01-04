using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.Buckets.Interfaces;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshPoint: IBox
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
        public bool IsVertex { 
            get
            {
                return VertexTriangles.Any();
            }
        }
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
        private List<IntermeshDivision> _divisions { get; } = new List<IntermeshDivision>();
        private List<IntermeshTriangle> _triangles = new List<IntermeshTriangle>();
        public IReadOnlyList<IntermeshTriangle> VertexTriangles
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

        public IReadOnlyList<IntermeshDivision> Divisions
        {
            get { return _divisions; }
        }
        public bool Add(IntermeshSegment segment)
        {
            if (_segments.Any(t => t.Id == segment.Id)) { return false; }
            _segments.Add(segment);
            return true;
        }

        public bool Add(IntermeshDivision division)
        {
            if (_divisions.Any(t => t.Id == division.Id)) { return false; }
            _divisions.Add(division);
            return true;
        }
    }
}
