using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshDivisionOLD
    {
        private static int _id = 0;
        private static object lockObject = new object();
        public IntermeshDivisionOLD(IntermeshPointOLD a, IntermeshPointOLD b)
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

        private List<IntermeshSegmentOLD> _relatedParentSegments = new List<IntermeshSegmentOLD>();
        public void Add(IntermeshSegmentOLD others)
        {
            if (_relatedParentSegments.Any(r => r.Id == others.Id)) { return; }
            _relatedParentSegments.Add(others);
        }
        public IReadOnlyList<IntermeshSegmentOLD> RelatedParentSegments { get { return _relatedParentSegments; } }

        public IntermeshPointOLD A { get; }
        public IntermeshPointOLD B { get; }
        public LineSegment3D Segment { get; }
        public IEnumerable<IntermeshPointOLD> Points 
        {
            get { yield return A; yield return B; }
        }
        public override string ToString()
        {
            return $"Intermesh Division {Key}";
        }
    }
}
