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

        public static void CrossWithIntersectionResolve((Point3D, IntermeshCapsule, IntermeshCapsule, IntermeshSegment, IntermeshSegment) unresolvedSet, BoxBucket<IntermeshSegment> bucket)
        {
            var intersection = unresolvedSet.Item1;
            if (intersection is null) { return; }
            var capsule1 = unresolvedSet.Item2;
            var capsule2 = unresolvedSet.Item3;
            var segment1 = unresolvedSet.Item4;
            var segment2 = unresolvedSet.Item5;

            var isResolved1 = IntermeshSegmentExtensions.IsResolved(segment1, segment2);
            //{
            var point = IntermeshPointExtensions.Fetch(intersection);
            var wasSplit1 = segment1.SplitBy(point);
            var wasSplit2 = segment2.SplitBy(point);

            var nearbyPoints1 = segment1.Capsules.Points().Where(p => Point3D.Distance(p.Point, point.Point) < 1e-9).ToArray();
            var nearbyPoints2 = segment2.Capsules.Points().Where(p => Point3D.Distance(p.Point, point.Point) < 1e-9).ToArray();

            segment1.ResolvePoints(nearbyPoints1);
            segment1.ResolvePoints(nearbyPoints2);

            segment2.ResolvePoints(nearbyPoints1);
            segment2.ResolvePoints(nearbyPoints2);
            //}

            var isResolved2 = IntermeshSegmentExtensions.IsResolved(segment1, segment2);


            var distanceAA = Point3D.Distance(intersection, capsule1.A.Point);
            var distanceAB = Point3D.Distance(intersection, capsule1.B.Point);
            var distanceBA = Point3D.Distance(intersection, capsule2.A.Point);
            var distanceBB = Point3D.Distance(intersection, capsule2.B.Point);

            var code = (distanceBB < 1e-9 ? 8 : 0) | (distanceBA < 1e-9 ? 4 : 0) | (distanceAB < 1e-9 ? 2 : 0) | (distanceAA < 1e-9 ? 1 : 0);

            if (!isResolved2)
            {
                BaseObjects.Console.WriteLine($"Failed cross {segment1.Id} {segment2.Id} Code {code} Segment1 split {wasSplit1} Segment2 split {wasSplit2}", ConsoleColor.Red);
            }
            //else
            //{
            //    BaseObjects.Console.WriteLine($"Successful cross {segment1.Id} {segment2.Id} Code {code} Segment1 split {wasSplit1} Segment2 split {wasSplit2}", ConsoleColor.Green);
            //}

            //    return;
            //switch (code)
            //{
            //    case 0:
            //        {
            //            var point = IntermeshPointExtensions.Fetch(intersection);
            //            segment1.SplitBy(point);
            //            segment2.SplitBy(point);
            //        }
            //        break;
            //    case 1:
            //        {
            //            //unresolvedPair.Item2.SplitBy(unresolvedPair.Item1.A);
            //            var to = IntermeshPointExtensions.Fetch(intersection);
            //            segment2.SplitBy(to);
            //            var from = capsule1.A;
            //            bucket.PointTransferFromTo(from, to);
            //        }
            //        break;
            //    case 2:
            //        {
            //            //unresolvedPair.Value.Item2.SplitBy(unresolvedPair.Value.Item1.B);
            //            var to = IntermeshPointExtensions.Fetch(intersection);
            //            segment2.SplitBy(to);
            //            var from = capsule1.B;
            //            bucket.PointTransferFromTo(from, to);
            //        }
            //        break;
            //    case 3:
            //        {
            //            // Two ...
            //            BaseObjects.Console.WriteLine($"CASE {code}", ConsoleColor.Red);
            //        }
            //        break;
            //    case 4:
            //        {
            //            //unresolvedPair.Value.Item1.SplitBy(unresolvedPair.Value.Item2.A);
            //            var to = IntermeshPointExtensions.Fetch(intersection);
            //            segment1.SplitBy(to);
            //            var from = capsule2.A;
            //            bucket.PointTransferFromTo(from, to);
            //        }
            //        break;
            //    case 5:
            //        {
            //            // Two
            //            var to = IntermeshPointExtensions.Fetch(intersection);
            //            segment1.ReplaceWith(capsule1.A, to);
            //            segment2.ReplaceWith(capsule2.A, to);
            //            bucket.PointTransferFromTo(capsule1.A, to);
            //            bucket.PointTransferFromTo(capsule2.A, to);
            //        }
            //        break;
            //    case 6:
            //        {
            //            // Two
            //            var to = IntermeshPointExtensions.Fetch(intersection);
            //            segment1.ReplaceWith(capsule1.B, to);
            //            segment2.ReplaceWith(capsule2.A, to);
            //            bucket.PointTransferFromTo(capsule1.B, to);
            //            bucket.PointTransferFromTo(capsule2.A, to);
            //        }
            //        break;
            //    case 7:
            //        BaseObjects.Console.WriteLine($"CASE {code}", ConsoleColor.Red);
            //        break;
            //    case 8:
            //        {
            //            //unresolvedPair.Value.Item1.SplitBy(unresolvedPair.Value.Item2.B);
            //            var to = IntermeshPointExtensions.Fetch(intersection);
            //            segment1.SplitBy(to);
            //            var from = capsule2.B;
            //            bucket.PointTransferFromTo(from, to);
            //        }
            //        break;
            //    case 9:
            //        {
            //            // Two
            //            var to = IntermeshPointExtensions.Fetch(intersection);
            //            segment1.ReplaceWith(capsule1.A, to);
            //            segment2.ReplaceWith(capsule2.B, to);
            //            bucket.PointTransferFromTo(capsule1.A, to);
            //            bucket.PointTransferFromTo(capsule2.B, to);
            //        }
            //        break;
            //    case 10:
            //        {
            //            // Two
            //            var to = IntermeshPointExtensions.Fetch(intersection);
            //            segment1.ReplaceWith(capsule1.B, to);
            //            segment2.ReplaceWith(capsule2.B, to);
            //            bucket.PointTransferFromTo(capsule1.B, to);
            //            bucket.PointTransferFromTo(capsule2.B, to);
            //        }
            //        break;
            //    case 11:
            //        BaseObjects.Console.WriteLine($"CASE {code}", ConsoleColor.Red);
            //        break;
            //    case 12:
            //        {
            //            // Two ...
            //            BaseObjects.Console.WriteLine($"CASE {code}", ConsoleColor.Red);
            //        }
            //        break;
            //    case 13: BaseObjects.Console.WriteLine($"CASE {code}", ConsoleColor.Red); break;
            //    case 14: BaseObjects.Console.WriteLine($"CASE {code}", ConsoleColor.Red); break;
            //    case 15: BaseObjects.Console.WriteLine($"CASE {code}", ConsoleColor.Red); break;
            //}
        }
    }
}
