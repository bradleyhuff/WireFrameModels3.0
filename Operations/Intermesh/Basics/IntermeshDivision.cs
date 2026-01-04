using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshDivision
    {
        private static int _id = 0;
        private static object lockObject = new object();
        public IntermeshDivision(IntermeshPoint a, IntermeshPoint b, IntermeshSegment parentSegment)
        {
            A = a;
            B = b;
            lock (lockObject)
            {
                Id = _id++;
            }
            Key = new Combination2(a.Id, b.Id);
            Segment = new LineSegment3D(a.Point, b.Point);
            ParentSegment = parentSegment;
        }

        public int Id { get; }
        public Combination2 Key { get; }

        public IntermeshSegment ParentSegment { get; }

        public IEnumerable<IntermeshTriangle> Triangles
        {
            get
            {
                return ParentSegment.Triangles;
            }
        }

        public IntermeshPoint A { get; }
        public IntermeshPoint B { get; }
        public LineSegment3D Segment { get; }
        public IEnumerable<IntermeshPoint> Points 
        {
            get { yield return A; yield return B; }
        }
    }
}
