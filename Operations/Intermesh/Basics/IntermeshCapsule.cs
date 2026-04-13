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
            Key = new Combination2(a.Id, b.Id);
            Segment = new LineSegment3D(a.Point, b.Point);
        }
        public int Id { get; }
        public Combination2 Key { get; }
        public IntermeshPoint A { get; }
        public IntermeshPoint B { get; }
        public LineSegment3D Segment { get; }
        public IEnumerable<IntermeshPoint> Points
        {
            get { yield return A; yield return B; }
        }
        public override string ToString()
        {
            return $"Intermesh Capsule {Key}";
        }

        public static bool IsNearParallel(IntermeshCapsule a, IntermeshCapsule b)
        {
            if (a is null || b is null) { return false; }
            if (Point3D.Distance(a.A.Point, b.A.Point) < 1e-9 && Point3D.Distance(a.B.Point, b.B.Point) < 1e-9) { return true; }
            if (Point3D.Distance(a.A.Point, b.B.Point) < 1e-9 && Point3D.Distance(a.B.Point, b.A.Point) < 1e-9) { return true; }
            return false;
        }

        public static bool SharesOnlyOnePoint(IntermeshCapsule a, IntermeshCapsule b)
        {
            if (a.A.Id == b.A.Id && a.B.Id != b.B.Id) { return true; }
            if (a.A.Id == b.B.Id && a.B.Id != b.A.Id) { return true; }
            if (a.B.Id == b.A.Id && a.A.Id != b.B.Id) { return true; }
            if (a.B.Id == b.B.Id && a.A.Id != b.A.Id) { return true; }
            return false;
        }

        public bool IsResolved(IntermeshCapsule c)
        {
            if (A.Id == c.A.Id && Segment.Distance(c.B.Point) > 1e-9) { return true; }
            if (B.Id == c.B.Id && Segment.Distance(c.A.Point) > 1e-9) { return true; }
            if (A.Id == c.B.Id && Segment.Distance(c.A.Point) > 1e-9) { return true; }
            if (B.Id == c.A.Id && Segment.Distance(c.B.Point) > 1e-9) { return true; }
            return false;
        }

        private static Combination2Dictionary<IntermeshCapsule> segmentTable = new Combination2Dictionary<IntermeshCapsule>();

        public static IntermeshCapsule Fetch(IntermeshPoint a, IntermeshPoint b)
        {
            var key = new Combination2(a.Id, b.Id);
            if (!segmentTable.ContainsKey(key)) { segmentTable[key] = new IntermeshCapsule(a, b); }
            return segmentTable[key];
        }

        public static IntermeshCapsule Fetch(Point3D a, Point3D b)
        {
            return Fetch(IntermeshPoint.Fetch(a), IntermeshPoint.Fetch(b));
        }
    }
}
