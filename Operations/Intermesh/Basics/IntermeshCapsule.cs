using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshCapsule
    {
        private static int _id = 0;
        private static object lockObject = new object();

        public IntermeshCapsule(IntermeshPoint a, IntermeshPoint b)
        {
            A = a;
            B = b;
            lock (lockObject)
            {
                Id = _id++;
            }
            Key = new Combination2(A.Id, B.Id);
            Segment = new LineSegment3D(A.Point, B.Point);
        }
        public int Id { get; }
        public Combination2 Key { get; private set; }
        public IntermeshPoint A { get; private set; }
        public IntermeshPoint B { get; private set; }
        public LineSegment3D Segment { get; private set; }

        public IEnumerable<IntermeshPoint> Points
        {
            get { yield return A; yield return B; }
        }
        public override string ToString()
        {
            return $"Intermesh Capsule {Key}";
        }

        public bool IsInteriorNearParallel(IntermeshCapsule element)
        {
            if (Key == element.Key) { return false; }
            if (!Segment.PointIsWithIn(element.A.Point)) { return false; }
            if (!Segment.PointIsWithIn(element.B.Point)) { return false; }
            return Segment.Distance(element.A.Point) < 1e-9 && Segment.Distance(element.B.Point) < 1e-9;
        }

        internal bool CanSplit(IntermeshPoint p, double error = BasicObjects.Math.Double.DifferenceError)
        {
            if (A.Id == p.Id || B.Id == p.Id) { return false; }
            var projection = Segment.Projection(p.Point, error);
            return Segment.PointIsAtOrBetweenEndpoints(projection, error);
        }
    }
}
