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
        public static IntermeshCapsule Nearest(this IEnumerable<IntermeshCapsule> line, IntermeshPoint p)
        {
            IntermeshCapsule output = null;
            double outSideDistance = System.Double.MaxValue;

            foreach (var element in line)
            {
                var distance = element.Segment.OutsideDistance(p.Point);
                if (distance < outSideDistance)
                {
                    outSideDistance = distance;
                    output = element;
                }
            }

            return output;
        }

        public static bool IsNearParallel(IntermeshCapsule a, IntermeshCapsule b)
        {
            if (a is null || b is null) { return false; }
            if (a.Key == b.Key) { return false; }
            if (Point3D.Distance(a.A.Point, b.A.Point) < GapConstants.Resolver && Point3D.Distance(a.B.Point, b.B.Point) < GapConstants.Resolver) { return true; }
            if (Point3D.Distance(a.A.Point, b.B.Point) < GapConstants.Resolver && Point3D.Distance(a.B.Point, b.A.Point) < GapConstants.Resolver) { return true; }
            return false;
        }

        public static IEnumerable<IntermeshPoint> Points(this IEnumerable<IntermeshCapsule> capsules)
        {
            if (!capsules.Any()) { yield break; }

            yield return capsules.First().A;
            foreach (var capsule in capsules)
            {
                yield return capsule.B;
            }
        }

        private static Dictionary<(int, int), IntermeshCapsule> segmentTable = new Dictionary<(int, int), IntermeshCapsule>();

        public static IntermeshCapsule Fetch(IntermeshPoint a, IntermeshPoint b)
        {
            if (!segmentTable.ContainsKey((a.Id, b.Id))) { segmentTable[(a.Id, b.Id)] = new IntermeshCapsule(a, b); }
            return segmentTable[(a.Id, b.Id)];
        }

        public static IntermeshCapsule Fetch(Point3D a, Point3D b)
        {
            return Fetch(IntermeshPointExtensions.Fetch(a), IntermeshPointExtensions.Fetch(b));
        }
    }
}
