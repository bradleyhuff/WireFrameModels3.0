using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshDivision
    {
        private static int _id = 0;
        private static object lockObject = new object();
        public IntermeshDivision(IntermeshPoint a, IntermeshPoint b)
        {
            A = a;
            B = b;
            lock (lockObject)
            {
                Id = _id++;
            }
            Key = new Combination2(a.Id, b.Id);
            Segment = new LineSegment3D(a.Point, b.Point);
        }

        public int Id { get; }
        public Combination2 Key { get; }

        private List<IntermeshSegment> _relatedParentSegments = new List<IntermeshSegment>();
        public void Add(IntermeshSegment others)
        {
            if (_relatedParentSegments.Any(r => r.Id == others.Id)) { return; }
            _relatedParentSegments.Add(others);
        }
        public IReadOnlyList<IntermeshSegment> RelatedParentSegments { get { return _relatedParentSegments; } }

        public IntermeshPoint A { get; }
        public IntermeshPoint B { get; }
        public LineSegment3D Segment { get; }
        public IEnumerable<IntermeshPoint> Points 
        {
            get { yield return A; yield return B; }
        }
        public override string ToString()
        {
            return $"Intermesh Division {Key}";
        }
    }
}
