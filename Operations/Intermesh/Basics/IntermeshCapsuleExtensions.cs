using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal static class IntermeshCapsuleExtensions
    {
        public static IEnumerable<IntermeshCapsule> SplitBy(this IEnumerable<IntermeshCapsule> line, IEnumerable<IntermeshCapsule> splitBy)
        {
            var points = splitBy.SelectMany(c => c.Points).DistinctBy(p => p.Id);
            return SplitBy(line, points);
        }
        public static IEnumerable<IntermeshCapsule> SplitBy(IEnumerable<IntermeshCapsule> line, IEnumerable<IntermeshPoint> points)
        {
            if (!points.Any())
            {
                return line;
            }
            var split = SplitBy(line, points.First()).ToArray();
            foreach (var p in points.Skip(1))
            {
                split = SplitBy(split, p).ToArray();
            }
            return split;
        }

        public static IEnumerable<IntermeshCapsule> SplitBy(this IEnumerable<IntermeshCapsule> line, IntermeshPoint p)
        {
            foreach (var element in line)
            {
                foreach (var split in element.Split(p))
                {
                    yield return split;
                }
            }
        }

        public static bool IsNearParallel(IntermeshCapsule a, IntermeshCapsule b)
        {
            if (a is null || b is null) { return false; }
            if (a.Id == b.Id) { return false; }
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

        private static Combination2Dictionary<IntermeshCapsule> segmentTable = new Combination2Dictionary<IntermeshCapsule>();

        public static IntermeshCapsule Fetch(IntermeshPoint a, IntermeshPoint b)
        {
            var key = new Combination2(a.Id, b.Id);
            if (!segmentTable.ContainsKey(key)) { segmentTable[key] = new IntermeshCapsule(a, b); }
            return segmentTable[key];
        }

        public static IntermeshCapsule Fetch(Point3D a, Point3D b)
        {
            return Fetch(IntermeshPointExtensions.Fetch(a), IntermeshPointExtensions.Fetch(b));
        }
    }
}
