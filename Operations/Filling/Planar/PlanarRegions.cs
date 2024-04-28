using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Operations.Filling.Interfaces;
using Operations.Regions;
using Double = BasicObjects.Math.Double;

namespace Operations.Filling.Planar
{
    internal class PlanarRegions : IFillingRegions
    {
        private double _testSegmentLength = 0;
        private IEnumerable<FillingSegment> _segments;
        private Plane _plane;

        public PlanarRegions(Plane plane, double testSegmentLength)
        {
            _plane = plane;
            _testSegmentLength = testSegmentLength;
        }

        private BoxBucket<FillingSegment> _bucket = null;

        private BoxBucket<FillingSegment> Bucket
        {
            get
            {
                if (_bucket is null)
                {
                    _bucket = new BoxBucket<FillingSegment>(_segments.ToArray());
                }
                return _bucket;
            }
        }

        public void Load(IEnumerable<FillingSegment> segments)
        {
            _segments = segments;
            _bucket = null;
        }

        public bool CrossesInterior(FillingSegment testSegment)
        {
            var testPoint = 0.5 * testSegment.Segment.Start + 0.5 * testSegment.Segment.End;
            return RegionOfProjectedPoint(testPoint) == Region.Interior;
        }

        public bool IsAtBoundary(FillingSegment testSegment)
        {
            var testPoint = 0.5 * testSegment.Segment.Start + 0.5 * testSegment.Segment.End;
            return RegionOfProjectedPoint(testPoint) == Region.OnBoundary;
        }

        public Region RegionOfProjectedPoint(Point3D point)
        {
            point = _plane.Projection(point);

            var xPlane = new Plane(point, Vector3D.BasisX);
            var yPlane = new Plane(point, Vector3D.BasisY);
            var zPlane = new Plane(point, Vector3D.BasisZ);

            var lineX = Plane.Intersection(xPlane, _plane);
            var lineY = Plane.Intersection(yPlane, _plane);
            var lineZ = Plane.Intersection(zPlane, _plane);

            int voteForInterior = 0;
            int voteForExterior = 0;

            if (lineX is not null)
            {
                var testSegment = new FillingSegment(point + -_testSegmentLength * lineX.Vector.Direction, point + _testSegmentLength * lineX.Vector.Direction);
                var matches = GetNonLinkingSegments(Bucket.Fetch(testSegment), testSegment).ToArray();
                var intersections = GetCrossingIntersections(testSegment, matches).DistinctBy(x => x).ToArray();
                var region = Manifold.GetRegion(point, intersections);
                if (region == Region.Interior) { voteForInterior++; }
                if (region == Region.Exterior) { voteForExterior++; }
                if (region == Region.OnBoundary) { return Region.OnBoundary; }
            }
            if (lineY is not null)
            {
                var testSegment = new FillingSegment(point + -_testSegmentLength * lineY.Vector.Direction, point + _testSegmentLength * lineY.Vector.Direction);
                var matches = GetNonLinkingSegments(Bucket.Fetch(testSegment), testSegment).ToArray();
                var intersections = GetCrossingIntersections(testSegment, matches).DistinctBy(x => x).ToArray();
                var region = Manifold.GetRegion(point, intersections);
                if (region == Region.Interior) { voteForInterior++; }
                if (region == Region.Exterior) { voteForExterior++; }
                if (region == Region.OnBoundary) { return Region.OnBoundary; }
            }
            if (voteForInterior >= 2) { return Region.Interior; }
            if (voteForExterior >= 2) { return Region.Exterior; }
            if (lineZ is not null)
            {
                var testSegment = new FillingSegment(point + -_testSegmentLength * lineZ.Vector.Direction, point + _testSegmentLength * lineZ.Vector.Direction);
                var matches = GetNonLinkingSegments(Bucket.Fetch(testSegment), testSegment).ToArray();
                var intersections = GetCrossingIntersections(testSegment, matches).DistinctBy(x => x).ToArray();
                var region = Manifold.GetRegion(point, intersections);
                if (region == Region.Interior) { voteForInterior++; }
                if (region == Region.Exterior) { voteForExterior++; }
                if (region == Region.OnBoundary) { return Region.OnBoundary; }
            }

            if (voteForInterior >= 2) { return Region.Interior; }
            if (voteForExterior >= 2) { return Region.Exterior; }
            return Region.Indeterminant;
        }

        private IEnumerable<FillingSegment> GetNonLinkingSegments(IEnumerable<FillingSegment> matches, FillingSegment segment)
        {
            return matches.Where(n => LineSegment3D.IsNonLinking(n.Segment, segment.Segment));
        }

        private IEnumerable<Point3D> GetCrossingIntersections(FillingSegment testSegment, IEnumerable<FillingSegment> matches)
        {
            var pointSegments = matches.Select(m =>
                    new { Intersection = LineSegment3D.PointIntersection(testSegment.Segment, m.Segment), m.Segment }).
                    Where(a => a.Intersection is not null);

            var orientedSegments = new List<LineSegment3D>();
            foreach (var pointSegment in pointSegments)
            {
                if (pointSegment.Segment.PointIsAtAnEndpoint(pointSegment.Intersection))
                {
                    orientedSegments.Add(pointSegment.Segment.Orient(pointSegment.Intersection));
                }
                else
                {
                    yield return pointSegment.Intersection;
                }
            }

            var crossingPoints = PointOppositeSegments(orientedSegments).
                Where(s => s.Item2.Any(c => LineSegment3D.PointIntersection(c, testSegment.Segment) is not null)).Select(s => s.Item1);
            foreach (var crossingPoint in crossingPoints)
            {
                yield return crossingPoint;
            }
        }

        private List<Tuple<Point3D, LineSegment3D[]>> PointOppositeSegments(List<LineSegment3D> orientedSegments)
        {
            var distincts = new List<List<Point3D>>();

            foreach (var orientedSegment in orientedSegments)
            {
                if (!distincts.Any(d => d[0] == orientedSegment.Start)) { distincts.Add(new List<Point3D>() { orientedSegment.Start }); }
                var distinct = distincts.Single(d => d[0] == orientedSegment.Start);
                distinct.Add(orientedSegment.End);
            }

            return distincts.Where(d => d.Count >= 3).Select(d => new Tuple<Point3D, LineSegment3D[]>(d[0],
                LineSegmentsFromPoints(d).ToArray())).ToList();
        }

        private IEnumerable<LineSegment3D> LineSegmentsFromPoints(List<Point3D> points)
        {
            for (int i = 2; i < points.Count; i++)
            {
                yield return new LineSegment3D(points[1], points[i]);
            }
        }

        public bool HasIntersection(FillingSegment testSegment)
        {
            var nonLinkingSegments = GetNonLinkingSegments(Bucket.Fetch(testSegment), testSegment);
            var intersectionDistances = nonLinkingSegments.Select(m => new
            {
                Distance = Point3D.Distance(testSegment.Segment.Start, LineSegment3D.PointIntersection(m.Segment, testSegment.Segment))
            }).Where(d => Double.IsValid(d.Distance) && d.Distance > Double.ProximityError);
            return intersectionDistances.Any();
        }

        public FillingSegment GetNearestIntersectingSegment(FillingSegment testSegment)
        {
            var nonLinkingSegments = GetNonLinkingSegments(Bucket.Fetch(testSegment), testSegment);

            var intersectionDistances = nonLinkingSegments.Select(m => new
            {
                Segment = m,
                Distance = Point3D.Distance(testSegment.Segment.Start, LineSegment3D.PointIntersection(m.Segment, testSegment.Segment))
            }).Where(d => Double.IsValid(d.Distance) && d.Distance > Double.ProximityError);

            if (!intersectionDistances.Any()) { return null; }

            var minDistance = intersectionDistances.Min(d => d.Distance);

            return intersectionDistances.FirstOrDefault(d => d.Distance == minDistance)?.Segment;
        }
    }
}
