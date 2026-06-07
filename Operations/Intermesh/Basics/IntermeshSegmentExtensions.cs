using BasicObjects.GeometricObjects;
using Collections.Buckets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal static class IntermeshSegmentExtensions
    {
        public static bool IsNearParallel((IntermeshSegment, IntermeshSegment) p)
        {
            return IsNearParallel(p.Item1, p.Item2);
        }
        public static bool IsNearParallel(IntermeshSegment a, IntermeshSegment b)
        {
            if (a is null || b is null) { return false; }
            if (a.Id == b.Id) { return false; }
            if (a.Key == b.Key) { return false; }
            if (a.IsRemoved || b.IsRemoved) { return false; }
            if (Point3D.Distance(a.A.Point, b.A.Point) < 1e-9 && Point3D.Distance(a.B.Point, b.B.Point) < 1e-9) { return true; }
            if (Point3D.Distance(a.A.Point, b.B.Point) < 1e-9 && Point3D.Distance(a.B.Point, b.A.Point) < 1e-9) { return true; }
            return false;
        }

        public static bool IsNearInLineParallel((IntermeshSegment, IntermeshSegment) p)
        {
            return IsNearInLineParallel(p.Item1, p.Item2);
        }

        public static bool IsNearInLineParallel(IntermeshSegment a, IntermeshSegment b)
        {
            if (a is null || b is null) { return false; }
            if (a.Id == b.Id) { return false; }

            int matchingPoints = a.Points.IntersectBy(b.Points.Select(p => p.Id), p => p.Id).Count();
            int collinearPoints = 2 - matchingPoints;

            if (collinearPoints <= 0) { return true; }

            var distancesToA = b.Points.Select(p => (a.Segment.ProjectionWithIn(p.Point), p.Point))
                .Where(p => p.Item1 is not null).Select(d => Point3D.Distance(d.Item1, d.Item2));
            var distancesToB = a.Points.Select(p => (b.Segment.ProjectionWithIn(p.Point), p.Point))
                .Where(p => p.Item1 is not null).Select(d => Point3D.Distance(d.Item1, d.Item2));

            var distances = distancesToA;
            if (distances.Count() < collinearPoints) { distances = distancesToB; }
            if (distances.Count() < collinearPoints) { return false; }

            return distances.All(d => d < 1e-9);
        }

        public static bool IsCross((IntermeshSegment, IntermeshSegment) p)
        {
            return IsCross(p.Item1, p.Item2);
        }

        public static bool IsCross(IntermeshSegment a, IntermeshSegment b)
        {
            if (a is null || b is null) { return false; }
            if (a.Id == b.Id) { return false; }
            if (LineSegment3D.Distance(a.Segment, b.Segment) > 1e-9) { return false; }

            var lineA = a.Segment.LineExtension;
            var lineB = b.Segment.LineExtension;
            var distance1 = lineA.Distance(b.Segment.Start);
            var distance2 = lineA.Distance(b.Segment.End);
            var distance3 = lineB.Distance(a.Segment.Start);
            var distance4 = lineB.Distance(a.Segment.End);

            var maxDistanceA = Math.Max(distance1, distance2);
            var maxDistanceB = Math.Max(distance3, distance4);

            return Math.Max(maxDistanceA, maxDistanceB) > 1e-9;
        }

        public static bool IsResolved((IntermeshSegment, IntermeshSegment) p)
        {
            return IsResolved(p.Item1, p.Item2);
        }

        public static bool IsResolved(IntermeshSegment a, IntermeshSegment b)
        {
            var pointsOfA = a.Capsules.SelectMany(c => c.Points).DistinctBy(p => p.Id).ToArray();
            var pointsOfB = b.Capsules.SelectMany(c => c.Points).DistinctBy(p => p.Id).ToArray();

            var sharedPoints = pointsOfA.IntersectBy(pointsOfB.Select(p => p.Id), p => p.Id);
            if (!sharedPoints.Any()) { return false; }

            if (pointsOfB.Any(p => IsANearbyPoint(p, a) && !a.PointIsResolved(p))) { return false; }
            if (pointsOfA.Any(p => IsANearbyPoint(p, b) && !b.PointIsResolved(p))) { return false; }
            return true;
        }

        private static bool IsANearbyPoint(IntermeshPoint p, IntermeshSegment segment)
        {
            foreach (var capsule in segment.Capsules)
            {
                if (p.Id == capsule.A.Id || p.Id == capsule.B.Id) { return false; }
            }

            return segment.Capsules.Any(c => c.Segment.Distance(p.Point) < 1e-9);
        }

        public static IEnumerable<IntermeshSegment> NonAdjoining(this IEnumerable<IntermeshSegment> input)
        {
            var previousPoints = new Dictionary<int, bool>();

            foreach (var element in input)
            {
                if (previousPoints.ContainsKey(element.A.Id)) { continue; }
                if (previousPoints.ContainsKey(element.B.Id)) { continue; }
                previousPoints[element.A.Id] = true;
                previousPoints[element.B.Id] = true;
                yield return element;
            }
        }

        public static (IntermeshPoint A, IntermeshPoint B, double Distance) ShortestLink((IntermeshSegment, IntermeshSegment) p)
        {
            var distance = Double.MaxValue;
            IntermeshPoint a = null;
            IntermeshPoint b = null;

            var points1 = p.Item1.Capsules.Points();
            var points2 = p.Item2.Capsules.Points();

            foreach (var point1 in points1)
            {
                foreach (var point2 in points2)
                {
                    var distance2 = Point3D.Distance(point1.Point, point2.Point);
                    if (distance2 < distance)
                    {
                        distance = distance2;
                        a = point1;
                        b = point2;
                    }
                }
            }

            return (a, b, distance);
        }

        public static (Point3D, IntermeshCapsule, IntermeshCapsule, IntermeshSegment, IntermeshSegment) PointIntersection((IntermeshSegment, IntermeshSegment) pair)
        {
            foreach (var capsule1 in pair.Item1.Capsules)
            {
                foreach (var capsule2 in pair.Item2.Capsules)
                {
                    if (capsule1.Id == capsule2.Id) { continue; }
                    var intersection = LineSegment3D.PointIntersection(capsule1.Segment, capsule2.Segment, 1e-9);
                    if (intersection is not null)
                    {
                        return (intersection, capsule1, capsule2, pair.Item1, pair.Item2);
                    }
                }
            }
            return (null, null, null, pair.Item1, pair.Item2);
        }

        public static bool IsLinked(IntermeshSegment a, IntermeshSegment b)
        {
            if (a.A.Id == b.A.Id) { return true; }
            if (a.A.Id == b.B.Id) { return true; }
            if (a.B.Id == b.A.Id) { return true; }
            if (a.B.Id == b.B.Id) { return true; }
            return false;
        }

        public static IEnumerable<IntermeshSegment> NearestOrLinked(this IntermeshSegment given, IEnumerable<IntermeshSegment> segments)
        {
            var linkedSegments = segments.Where(s => IsLinked(given, s)).ToArray();
            if (linkedSegments.Any()) 
            {
                foreach (var linkedSegment in linkedSegments)
                {
                    yield return linkedSegment;
                }
                yield break;
            }

            var distance = System.Double.MaxValue;
            IntermeshSegment nearest = null;
            foreach (var segment in segments)
            {
                var distance2 = LineSegment3D.Distance(given.Segment, segment.Segment);
                if (distance2 < distance)
                {
                    distance = distance2;
                    nearest = segment;
                }
            }
            yield return nearest;
        }
    }
}
