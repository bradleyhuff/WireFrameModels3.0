using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.Buckets.Interfaces;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshPointOLD: IBox
    {
        private static int _id = 0;
        private static object lockObject = new object();
        public IntermeshPointOLD(Point3D point) 
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

        private List<IntermeshSegmentOLD> _segments { get; } = new List<IntermeshSegmentOLD>();
        private List<IntermeshDivisionOLD> _divisions { get; } = new List<IntermeshDivisionOLD>();
        private List<IntermeshTriangleOLD> _triangles = new List<IntermeshTriangleOLD>();
        public IReadOnlyList<IntermeshTriangleOLD> VertexTriangles
        {
            get { return _triangles; }
        }
        public bool Add(IntermeshTriangleOLD triangle)
        {
            if (_triangles.Any(t => t.Id == triangle.Id)) { return false; }
            _triangles.Add(triangle);
            return true;
        }

        public IReadOnlyList<IntermeshSegmentOLD> Segments
        {
            get { return _segments; }
        }

        public IReadOnlyList<IntermeshDivisionOLD> Divisions
        {
            get { return _divisions; }
        }
        public bool Add(IntermeshSegmentOLD segment)
        {
            if (_segments.Any(t => t.Id == segment.Id)) { return false; }
            _segments.Add(segment);
            return true;
        }

        public bool Add(IntermeshDivisionOLD division)
        {
            if (_divisions.Any(t => t.Id == division.Id)) { return false; }
            _divisions.Add(division);
            return true;
        }
    }
}
