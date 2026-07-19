using BaseObjects;
using BasicObjects.MathExtensions;
using Operations.PlanarFilling.Basics;
using Operations.SurfaceSegmentChaining.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal static class IntermeshTriangleExtensions
    {
        public static IEnumerable<SurfaceSegmentSets<PlanarFillingGroup, IntermeshPoint>> CreateSurfaceSegmentSets(this IntermeshTriangle triangle)
        {
            var perimeterSegments = triangle.PerimeterSlots.SelectMany(s => s.Segments).NonRepeating().NoSpurs().ToArray();
            var intersectionSegments = triangle.IntersectionSlots.Where(s => !triangle.PerimeterSlots.Any(p => p.Key == s.Key)).SelectMany(s => s.Segments).ToArray();
            intersectionSegments = intersectionSegments.ExceptBy(perimeterSegments.Select(s => s.Key), s => s.Key, Combination2Comparer.Comparer).ToArray();/*.DistinctBy(i => i.Key, Combination2Comparer.Comparer)*//*.NoSpurs()*/

            RemoveSpurs(perimeterSegments, ref intersectionSegments);

            var splits = BoundaryPointSplit(perimeterSegments, intersectionSegments);

            splits = splits.Select(s => (Perimeter: RemoveSpurs(s.Perimeter), Intersecting: s.Intersecting)).Where(s => s.Perimeter.Any()).ToArray();

            foreach (var split in splits)
            {
                yield return new SurfaceSegmentSets<PlanarFillingGroup, IntermeshPoint>
                {
                    NodeId = triangle.Id,
                    GroupObject = new PlanarFillingGroup(triangle.Triangle.Plane, triangle.Triangle.Box.Diagonal),
                    DividingSegments = GetSurfaceSegments(triangle, split.Intersecting).ToArray(),
                    PerimeterSegments = GetSurfaceSegments(triangle, split.Perimeter).ToArray()
                };
            }
        }

        private static IEnumerable<(IntermeshSegment[] Perimeter, IntermeshSegment[] Intersecting)> BoundaryPointSplit(IntermeshSegment[] perimeterSegments, IntermeshSegment[] intersectionSegments)
        {
            var boundaryPoints = perimeterSegments.SelectMany(ss => ss.Points).GroupBy(g => g.Id).Where(g => g.Count() > 2).Select(g => g.Key).ToArray();
            if (!boundaryPoints.Any()) {
                yield return new(perimeterSegments, intersectionSegments);
                yield break;
            }

            var allPoints = perimeterSegments.SelectMany(s => s.Points).Concat(intersectionSegments.SelectMany(s => s.Points)).DistinctBy(p => p.Id).ToArray();
            var nonBoundaryPoints = allPoints.ExceptBy(boundaryPoints, p => p.Id).ToArray();

            var links = new GroupingDictionary<int, List<(IntermeshSegment Segment, Boolean IsPerimeter)>>(() => new List<(IntermeshSegment Segment, Boolean IsPerimeter)>());
            var usedPoints = new Dictionary<int, bool>();
            var usedSegments = new Dictionary<int, bool>();
            foreach (var point in boundaryPoints)
            {
                usedPoints[point] = true;
            }

            foreach (var segment in perimeterSegments)
            {
                links[segment.A.Id].Add((segment, true));
                links[segment.B.Id].Add((segment, true));
            }
            foreach (var segment in intersectionSegments)
            {
                links[segment.A.Id].Add((segment, false));
                links[segment.B.Id].Add((segment, false));
            }

            var trees = new List<(IntermeshSegment Segment, Boolean IsPerimeter)[]>();

            foreach (var seedPoint in nonBoundaryPoints)
            {
                if (usedPoints.ContainsKey(seedPoint.Id)) { continue; }
                var treeSegments = BuildSpanningTree(seedPoint, links, usedPoints, usedSegments).ToArray();
                trees.Add(treeSegments);
            }

            foreach (var tree in trees)
            {
                var perimeters = tree.Where(t => t.IsPerimeter).Select(t => t.Segment).ToArray();
                var intersections = tree.Where(t => !t.IsPerimeter).Select(t => t.Segment).ToArray();
                yield return new (perimeters, intersections);
            }
        }

        private static IEnumerable<(IntermeshSegment Segment, Boolean IsPerimeter)> BuildSpanningTree(IntermeshPoint seed,
            GroupingDictionary<int, List<(IntermeshSegment Segment, Boolean IsPerimeter)>> links, Dictionary<int, bool> usedPoints, Dictionary<int, bool> usedSegments)
        {
            var seedPoints = new List<int>() { seed.Id};

            while (seedPoints.Any())
            {
                var newSeeds = new List<int>();

                foreach (var seedPoint in seedPoints)
                {
                    var segments = links[seedPoint];

                    foreach (var segment in segments)
                    {
                        if (usedSegments.ContainsKey(segment.Segment.Id)) { continue; }
                        var oppositePoint = segment.Segment.Points.Single(p => p.Id != seedPoint);
                        if (!usedPoints.ContainsKey(oppositePoint.Id)) { newSeeds.Add(oppositePoint.Id); }
                        usedSegments[segment.Segment.Id] = true;
                        yield return segment;
                    }
                    usedPoints[seedPoint] = true;
                }
                seedPoints = newSeeds;
            }
        }

        private static void RemoveSpurs(IntermeshSegment[] perimeterSegments, ref IntermeshSegment[] intersectionSegments)
        {
            while (true)
            {
                var pointCount = new GroupingDictionary<int, List<IntermeshSegment>>(() => new List<IntermeshSegment>());
                foreach (var element in perimeterSegments)
                {
                    pointCount[element.A.Id].Add(element);
                    pointCount[element.B.Id].Add(element);
                }
                foreach (var element in intersectionSegments)
                {
                    pointCount[element.A.Id].Add(element);
                    pointCount[element.B.Id].Add(element);
                }

                var spurs = pointCount.Values.Where(v => v.Count == 1).Select(l => l.Single()).ToArray();

                if (!spurs.Any()) { return; }

                intersectionSegments = intersectionSegments.Where(i => !spurs.Any(s => s.Id == i.Id)).ToArray();
            }
        }

        private static IntermeshSegment[] RemoveSpurs(IntermeshSegment[] perimeterSegments)
        {
            while (true)
            {
                var pointCount = new GroupingDictionary<int, List<IntermeshSegment>>(() => new List<IntermeshSegment>());
                foreach (var element in perimeterSegments)
                {
                    pointCount[element.A.Id].Add(element);
                    pointCount[element.B.Id].Add(element);
                }

                var spurs = pointCount.Values.Where(v => v.Count == 1).Select(l => l.Single()).ToArray();

                if (!spurs.Any()) { return perimeterSegments; }

                perimeterSegments = perimeterSegments.Where(i => !spurs.Any(s => s.Id == i.Id)).ToArray();
            }
        }

        private static IEnumerable<SurfaceSegmentContainer<IntermeshPoint>> GetSurfaceSegments(IntermeshTriangle triangle, IntermeshSegment[] segments)
        {
            foreach (var segment in segments)
            {
                yield return new SurfaceSegmentContainer<IntermeshPoint>(
                    new SurfaceRayContainer<IntermeshPoint>(triangle.RayFromProjectedPoint(segment.A.Point), triangle.Triangle.Normal, segment.A.Id, segment.A),
                    new SurfaceRayContainer<IntermeshPoint>(triangle.RayFromProjectedPoint(segment.B.Point), triangle.Triangle.Normal, segment.B.Id, segment.B));
            }
        }
    }
}
