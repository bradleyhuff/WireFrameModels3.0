using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Operations.Intermesh.Basics;
using Operations.ParallelSurfaces.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Extensions
{
    public static class FillTriangleExtensions
    {
        public static IEnumerable<FillTriangle> DivideFrom(this FillTriangle fillA, FillTriangle fillB)
        {
            var intersections = Triangle3D.LineSegmentIntersections(fillA.Triangle, fillB.Triangle);
            if (!intersections.Any())
            {
                yield return fillA;
                yield break;
            }
            var discretize = new Discretize<Point3D, DivideNode>(p => new Rectangle3D(p, BoxBucket.MARGINS), (s, n) => n.Point == s, p => new DivideNode(p));
            var allPoints = new List<DivideNode>();
            var intersectionPoints = intersections.SelectMany(i => i.Points).Select(discretize.Fetch).DistinctBy(p => p.Id).ToArray();
            allPoints.AddRange(intersectionPoints);
            foreach (var segment in intersections) { DivideNode.LinkIntersection(discretize.Fetch(segment.Start), discretize.Fetch(segment.End)); }
            var splits = fillA.Triangle.Edges.SelectMany(e => e.PointSplit(intersectionPoints.Select(p => p.Point).ToArray()));
            foreach (var split in splits) { DivideNode.LinkDifference(discretize.Fetch(split.Start), discretize.Fetch(split.End)); }
            allPoints.AddRange(fillA.Triangle.Vertices.Select(discretize.Fetch));
            allPoints = allPoints.DistinctBy(p => p.Id).ToList();

            var inflectionPoints = allPoints.Where(p => p.IntersectLinks.Any() && p.DifferenceLinks.Any());
            Point3D checkPoint = null;

            foreach (var inflectionPoint in inflectionPoints)
            {
                if (!inflectionPoint.IntersectLinks.Any() || !inflectionPoint.DifferenceLinks.Any()) { continue; }
                var intersectionLink = inflectionPoint.IntersectLinks.SingleOrDefault(l => !inflectionPoint.DifferenceLinks.Any(ll => ll.Id == l.Id));
                var differenceLink = inflectionPoint.DifferenceLinks.FirstOrDefault(l => !inflectionPoint.IntersectLinks.Any(ll => ll.Id == l.Id));
                if (intersectionLink is null || differenceLink is null) { continue; }

                Point3D a = inflectionPoint.Point;
                Point3D b = differenceLink.Point;
                Point3D c = intersectionLink.Point;

                var fillTriangle = new FillTriangle(a, fillA.NormalA, b, fillA.NormalB, c, fillA.NormalC);
                checkPoint = fillTriangle.Triangle.Center;

                yield return fillTriangle;

                if (DivideNode.HasDifferenceLink(differenceLink, intersectionLink))
                {
                    DivideNode.BreakDifference(differenceLink, intersectionLink);
                }
                else
                {
                    DivideNode.LinkDifference(differenceLink, intersectionLink);
                }
                DivideNode.BreakIntersection(inflectionPoint, intersectionLink);
                DivideNode.BreakDifference(inflectionPoint, differenceLink);
            }

            var differencePoints = allPoints.Where(p => !p.IntersectLinks.Any() && p.DifferenceLinks.Count == 2);

            foreach (var differencePoint in differencePoints)
            {
                if (differencePoint.DifferenceLinks.Count < 2) { continue; }

                var a = differencePoint;
                var b = differencePoint.DifferenceLinks[0];
                var c = differencePoint.DifferenceLinks[1];

                var fillTriangle = new FillTriangle(a.Point, fillA.NormalA, b.Point, fillA.NormalB, c.Point, fillA.NormalC);

                if (checkPoint is not null && fillTriangle.Triangle.PointIsIn(checkPoint)) { continue; }

                yield return fillTriangle;

                if (DivideNode.HasDifferenceLink(differencePoint.DifferenceLinks[0], differencePoint.DifferenceLinks[1]))
                {
                    DivideNode.BreakDifference(differencePoint.DifferenceLinks[0], differencePoint.DifferenceLinks[1]);
                }
                else
                {
                    DivideNode.LinkDifference(differencePoint.DifferenceLinks[0], differencePoint.DifferenceLinks[1]);
                }
                DivideNode.BreakDifference(differencePoint, differencePoint.DifferenceLinks[0]);
                DivideNode.BreakDifference(differencePoint, differencePoint.DifferenceLinks[0]);
            }

            if (intersectionPoints.Length == 3)
            {
                Point3D a = intersectionPoints[0].Point;
                Point3D b = intersectionPoints[1].Point;
                Point3D c = intersectionPoints[2].Point;

                yield return new FillTriangle(a, fillA.NormalA, b, fillA.NormalB, c, fillA.NormalC);

            }
            else
            {
                var center = Point3D.Average(intersectionPoints.Select(p => p.Point));
                foreach (var segment in intersections)
                {
                    yield return new FillTriangle(discretize.Fetch(center).Point, fillA.NormalA, discretize.Fetch(segment.Start).Point, fillA.NormalB, discretize.Fetch(segment.End).Point, fillA.NormalC);
                }
            }
        }

        private class DivideNode : PointNode
        {
            public DivideNode(Point3D point) : base(point) { }

            public List<DivideNode> IntersectLinks { get; } = new List<DivideNode>();
            public List<DivideNode> DifferenceLinks { get; } = new List<DivideNode>();

            public static bool HasIntersectionLink(DivideNode a, DivideNode b)
            {
                return a.IntersectLinks.Any(l => l.Id == b.Id) && b.IntersectLinks.Any(l => l.Id == a.Id);
            }
            public static bool HasDifferenceLink(DivideNode a, DivideNode b)
            {
                return a.DifferenceLinks.Any(l => l.Id == b.Id) && b.DifferenceLinks.Any(l => l.Id == a.Id);
            }
            public static void LinkIntersection(DivideNode a, DivideNode b)
            {
                if (!a.IntersectLinks.Any(l => l.Id == b.Id)) { a.IntersectLinks.Add(b); }
                if (!b.IntersectLinks.Any(l => l.Id == a.Id)) { b.IntersectLinks.Add(a); }
            }

            public static void LinkDifference(DivideNode a, DivideNode b)
            {
                if (!a.DifferenceLinks.Any(l => l.Id == b.Id)) { a.DifferenceLinks.Add(b); }
                if (!b.DifferenceLinks.Any(l => l.Id == a.Id)) { b.DifferenceLinks.Add(a); }
            }

            public static void BreakIntersection(DivideNode a, DivideNode b)
            {
                a.IntersectLinks.RemoveAll(l => l.Id == b.Id);
                b.IntersectLinks.RemoveAll(l => l.Id == a.Id);
            }
            public static void BreakDifference(DivideNode a, DivideNode b)
            {
                a.DifferenceLinks.RemoveAll(l => l.Id == b.Id);
                b.DifferenceLinks.RemoveAll(l => l.Id == a.Id);
            }
        }
    }
}
