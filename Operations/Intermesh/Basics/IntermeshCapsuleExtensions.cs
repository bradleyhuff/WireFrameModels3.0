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

        public static bool CapsuleSplit(this IEnumerable<IntermeshSegment> segments, IntermeshPoint p)
        {
            var capsules = segments.SelectMany(s => s.Capsules);
            var split = capsules.GetCapsuleToSplit(p);
            if (split is not null)
            {
                var split1 = Fetch(split.A, p);
                var split2 = Fetch(p, split.B);

                if (capsules.Any(c => c.Id == split1.Id)) { return false; }
                if (capsules.Any(c => c.Id == split2.Id)) { return false; }

                bool replaced = false;
                foreach (var segment in segments)
                {
                    if (segment.CapsuleReplace(split, [split1, split2]))
                    {
                        replaced = true;
                    }
                }
                return replaced;
            }

            return false;
        }

        private static IntermeshCapsule GetCapsuleToSplit(this IEnumerable<IntermeshCapsule> capsules, IntermeshPoint p)
        {
            return capsules.GetEligibleCapsuleToSplit(p).Nearest(p);
        }
        private static IEnumerable<IntermeshCapsule> GetEligibleCapsuleToSplit(this IEnumerable<IntermeshCapsule> capsules, IntermeshPoint p)
        {
            foreach (var capsule in capsules.Where(c => c.A.Id != p.Id && c.B.Id != p.Id))
            {
                var projection = capsule.Segment.Projection(p.Point, GapConstants.Resolver);
                if (projection is null) { continue; }
                var distance = Point3D.Distance(projection, p.Point);
                if (distance < GapConstants.Resolver)
                {
                    yield return capsule;
                }
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
