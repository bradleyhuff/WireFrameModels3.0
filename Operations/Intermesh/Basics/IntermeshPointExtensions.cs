using BasicObjects.GeometricObjects;
using Collections.Buckets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal static class IntermeshPointExtensions
    {
        public static void PointTransferFromTo(this BoxBucket<IntermeshSegment> bucket, IntermeshPoint from, IntermeshPoint to, IntermeshSegment containing = null)
        {
            foreach (var removal in bucket.LinkingSegments(from))
            {
                removal.ReplaceStartAndEndWith(
                    from.ReplaceRemovalPoint(removal.A, to),
                    from.ReplaceRemovalPoint(removal.B, to));
            }

            if (containing is not null)
            {
                foreach (var segment in bucket.LinkingSegments(to))
                {
                    segment.AddRangeContacts(containing.Contacts);
                }
            }
            else
            {
                foreach (var segment in bucket.LinkingSegments(to))
                {
                    segment.AddRangeContacts(bucket.LinkingSegments(from));
                    segment.AddRangeContacts(bucket.LinkingSegments(to));
                }
            }
        }

        public static IntermeshPoint NearestPoint(this IEnumerable<IntermeshPoint> points, IntermeshPoint point)
        {
            var distance = Double.MaxValue;
            IntermeshPoint nearestPoint = null;
            foreach (var p in points)
            {
                var distance2 = Point3D.Distance(point.Point, p.Point);
                if (distance2 < distance) { distance = distance2; nearestPoint = p; }
            }
            return nearestPoint;
        }

        public static IEnumerable<IntermeshSegment> LinkingSegments(this BoxBucket<IntermeshSegment> bucket, IntermeshPoint point)
        {
            return bucket.Fetch(point, 1e-5).Where(p => !p.IsRemoved && (p.A.Id == point.Id || p.B.Id == point.Id));
        }

        private static IntermeshPoint ReplaceRemovalPoint(this IntermeshPoint point, IntermeshPoint removalPoint, IntermeshPoint addPoint)
        {
            return point.Id == removalPoint.Id ? addPoint : removalPoint;
        }


        private static BoxBucket<IntermeshPoint> bucket = new BoxBucket<IntermeshPoint>();

        internal static IntermeshPoint Fetch(Point3D point)
        {
            var match = bucket.Fetch(new Rectangle3D(point, BoxBucket.MARGINS));
            var found = match.Where(m => Point3D.AreEqual(m.Point, point, 1e-15)).MinBy(p => Point3D.Distance(p.Point, point));
            if (found is not null)
            {
                return found;
            }
            var intermeshPoint = new IntermeshPoint(point);
            bucket.Add(intermeshPoint);
            return intermeshPoint;
        }
    }
}
