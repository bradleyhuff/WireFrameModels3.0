using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Operations.Regions;
using Double = BasicObjects.Math.Double;

namespace Operations.PlanarFilling.Filling.Internals
{
    internal class PlanarRegions
    {
        private double _testSegmentLength = 0;
        private IEnumerable<PlanarSegment> _segments;
        private Plane _plane;

        public PlanarRegions(Plane plane, double testSegmentLength)
        {
            _plane = plane;
            _testSegmentLength = testSegmentLength;
        }

        private BoxBucket<PlanarSegment> _bucket = null;

        private BoxBucket<PlanarSegment> Bucket
        {
            get
            {
                if (_bucket is null)
                {
                    _bucket = new BoxBucket<PlanarSegment>(_segments.ToArray());
                }
                return _bucket;
            }
        }

        public void Load(IEnumerable<PlanarSegment> segments)
        {
            _segments = segments;
            _bucket = null;
        }

        public bool CrossesInterior(PlanarSegment testSegment)
        {
            return RegionOfTestSegment(testSegment) == Region.Interior;
        }

        public bool IsAtBoundary(PlanarSegment testSegment)
        {
            return RegionOfTestSegment(testSegment) == Region.OnBoundary;
        }

        private Region RegionOfTestSegment(PlanarSegment testSegment)
        {
            var testPoint = 0.5 * testSegment.Segment.Start + 0.5 * testSegment.Segment.End;
            return RegionOfProjectedPoint(testPoint);
        }

        public Region RegionOfProjectedPoint(Point3D point)
        {
            return RegionOfProjectedPointNEW(point);
            //var result = RegionOfProjectedPointNEW(point);
            //Console.WriteLine($"Region {point} -> {result}");
            //return result;
        }

        private Region RegionOfProjectedPointNEW(Point3D point)
        {
            point = _plane.Projection(point);

            var xPlane = new Plane(point, Vector3D.BasisX);
            var lineX = Plane.Intersection(xPlane, _plane);
            if (lineX is not null)
            {
                var testSegment = new PlanarSegment(point + -_testSegmentLength * lineX.Vector.Direction, point + _testSegmentLength * lineX.Vector.Direction);
                var matches = GetNonLinkingSegments(Bucket.Fetch(testSegment), testSegment).ToArray();
                if (IsOnBoundary(point, matches)) { return Region.OnBoundary; }
                var checkingPoints = GetCheckingPoints(lineX, matches).ToArray();
                var region = Manifold.GetRegion(point, checkingPoints);
                //Console.WriteLine($"Region {point} {region}");
                if (region != Region.Indeterminant) { return region; }
            }

            var yPlane = new Plane(point, Vector3D.BasisY);
            var lineY = Plane.Intersection(yPlane, _plane);
            if (lineY is not null)
            {
                var testSegment = new PlanarSegment(point + -_testSegmentLength * lineY.Vector.Direction, point + _testSegmentLength * lineY.Vector.Direction);
                var matches = GetNonLinkingSegments(Bucket.Fetch(testSegment), testSegment).ToArray();
                if (IsOnBoundary(point, matches)) { return Region.OnBoundary; }
                var checkingPoints = GetCheckingPoints(lineY, matches).ToArray();
                var region = Manifold.GetRegion(point, checkingPoints);
                //Console.WriteLine($"Region {point} {region}");
                if (region != Region.Indeterminant) { return region; }
            }

            var zPlane = new Plane(point, Vector3D.BasisZ);
            var lineZ = Plane.Intersection(zPlane, _plane);
            if (lineZ is not null)
            {
                var testSegment = new PlanarSegment(point + -_testSegmentLength * lineZ.Vector.Direction, point + _testSegmentLength * lineZ.Vector.Direction);
                var matches = GetNonLinkingSegments(Bucket.Fetch(testSegment), testSegment).ToArray();
                if (IsOnBoundary(point, matches)) { return Region.OnBoundary; }
                var checkingPoints = GetCheckingPoints(lineZ, matches).ToArray();
                var region = Manifold.GetRegion(point, checkingPoints);
                //Console.WriteLine($"Region {point} {region}");
                if (region != Region.Indeterminant) { return region; }
            }
            Console.WriteLine($"Indeterminant point {point}");
            return Region.Indeterminant;
        }

        private IEnumerable<PlanarSegment> GetNonLinkingSegments(IEnumerable<PlanarSegment> matches, PlanarSegment segment)
        {
            return matches.Where(n => LineSegment3D.IsNonLinking(n.Segment, segment.Segment));
        }

        private bool IsOnBoundary(Point3D point, IEnumerable<PlanarSegment> matches)
        {
            foreach (var match in matches)
            {
                if (match.Segment.PointIsOnSegment(point)) { return true; }
            }
            return false;
        }

        private IEnumerable<Point3D> GetCheckingPoints(Line3D test, IEnumerable<PlanarSegment> matches)
        {
            SeparateSegments(test, matches, out List<PlanarSegment> links, out List<PlanarSegment> collinears);

            var checkingPoints = GetLinks(test, links).DistinctBy(x => x).ToArray();
            var groups = BuildCollinearGroups(collinears, links);
            if (!groups.Any()) { return checkingPoints; }
            var applicableGroups = groups.Where(g => g.IsApplicable()).ToArray();
            checkingPoints = checkingPoints.Where(c => !groups.Any(g => g.Segment.PointIsOnSegment(c))).ToArray();
            checkingPoints = checkingPoints.Concat(applicableGroups.Select(ag => ag.GetApplicablePoint())).ToArray();
            return checkingPoints;
        }

        private void SeparateSegments(Line3D test, IEnumerable<PlanarSegment> matches, out List<PlanarSegment> links, out List<PlanarSegment> collinears)
        {
            links = new List<PlanarSegment>();
            collinears = new List<PlanarSegment>();
            foreach (var match in matches)
            {
                if (test.SegmentIsOnLine(match.Segment)) { collinears.Add(match); }
                else
                {
                    links.Add(match);
                }
            }
        }

        private IEnumerable<Point3D> GetLinks(Line3D test, IEnumerable<PlanarSegment> links)
        {
            foreach (var link in links)
            {
                var intersection = Line3D.PointIntersection(link.Segment, test);
                if (intersection is null) { continue; }
                yield return intersection;
            }
        }

        private List<CollinearGroup> BuildCollinearGroups(IEnumerable<PlanarSegment> collinears, IEnumerable<PlanarSegment> links)
        {
            List<CollinearGroup> groups = new List<CollinearGroup>();
            var nonLinkedCollinears = collinears.ToList();
            //if (collinears.Any()) { Console.WriteLine($"Collinears {collinears.Count()}"); }

            while (nonLinkedCollinears.Any())
            {
                var group = new CollinearGroup(_plane);
                var groupAdded = false;
                do
                {
                    groupAdded = false;
                    foreach (var collinear in collinears)
                    {
                        if (group.AddCollinear(collinear)) { nonLinkedCollinears.Remove(collinear); groupAdded = true; }
                    }
                    collinears = nonLinkedCollinears.ToList();
                } while (groupAdded);
                if (group.HasElements) { groups.Add(group); }
            }

            foreach (var group in groups)
            {
                foreach (var link in links)
                {
                    group.AddLink(link);
                }
            }
            return groups;
        }



        //private Region RegionOfProjectedPointOLD(Point3D point)
        //{
        //    point = _plane.Projection(point);

        //    var xPlane = new Plane(point, Vector3D.BasisX);
        //    var yPlane = new Plane(point, Vector3D.BasisY);
        //    var zPlane = new Plane(point, Vector3D.BasisZ);

        //    var lineX = Plane.Intersection(xPlane, _plane);
        //    var lineY = Plane.Intersection(yPlane, _plane);
        //    var lineZ = Plane.Intersection(zPlane, _plane);

        //    int voteForInterior = 0;
        //    int voteForExterior = 0;

        //    if (lineX is not null)
        //    {
        //        var testSegment = new InternalPlanarSegment(point + -_testSegmentLength * lineX.Vector.Direction, point + _testSegmentLength * lineX.Vector.Direction);
        //        var matches = GetNonLinkingSegments(Bucket.Fetch(testSegment), testSegment).ToArray();

        //        //check for on boundary
        //        //

        //        var intersections = GetCrossingIntersections(testSegment, matches).DistinctBy(x => x).ToArray();
        //        var region = Manifold.GetRegion(point, intersections);
        //        if (region == Region.Interior) { voteForInterior++; }
        //        if (region == Region.Exterior) { voteForExterior++; }
        //        if (region == Region.OnBoundary) { return Region.OnBoundary; }
        //    }
        //    if (lineY is not null)
        //    {
        //        var testSegment = new InternalPlanarSegment(point + -_testSegmentLength * lineY.Vector.Direction, point + _testSegmentLength * lineY.Vector.Direction);
        //        var matches = GetNonLinkingSegments(Bucket.Fetch(testSegment), testSegment).ToArray();

        //        //check for on boundary
        //        //

        //        var intersections = GetCrossingIntersections(testSegment, matches).DistinctBy(x => x).ToArray();
        //        var region = Manifold.GetRegion(point, intersections);
        //        if (region == Region.Interior) { voteForInterior++; }
        //        if (region == Region.Exterior) { voteForExterior++; }
        //        if (region == Region.OnBoundary) { return Region.OnBoundary; }
        //    }
        //    if (voteForInterior >= 2) { return Region.Interior; }
        //    if (voteForExterior >= 2) { return Region.Exterior; }
        //    if (lineZ is not null)
        //    {
        //        var testSegment = new InternalPlanarSegment(point + -_testSegmentLength * lineZ.Vector.Direction, point + _testSegmentLength * lineZ.Vector.Direction);
        //        var matches = GetNonLinkingSegments(Bucket.Fetch(testSegment), testSegment).ToArray();

        //        //check for on boundary
        //        //

        //        var intersections = GetCrossingIntersections(testSegment, matches).DistinctBy(x => x).ToArray();
        //        var region = Manifold.GetRegion(point, intersections);
        //        if (region == Region.Interior) { voteForInterior++; }
        //        if (region == Region.Exterior) { voteForExterior++; }
        //        if (region == Region.OnBoundary) { return Region.OnBoundary; }
        //    }

        //    if (voteForInterior >= 2) { return Region.Interior; }
        //    if (voteForExterior >= 2) { return Region.Exterior; }
        //    Console.WriteLine($"Indeterminant point {point}");
        //    return Region.Indeterminant;
        //}

        //private IEnumerable<Point3D> GetCollinearCheckingPoints(IEnumerable<InternalPlanarSegment> collinears, IEnumerable<InternalPlanarSegment> links)
        //{
        //    var groups = BuildCollinearGroups(collinears, links);
        //    //Console.WriteLine($"Collinear groups {groups.Count}");
        //    foreach (var group in groups)
        //    {
        //        var applicablePoint = group.GetApplicablePoint();
        //        if (applicablePoint is not null)
        //        {
        //            Console.WriteLine($"Applicable point {applicablePoint}");
        //            yield return applicablePoint;
        //        }
        //    }
        //}

        //private IEnumerable<Point3D> GetCrossingIntersections(InternalPlanarSegment testSegment, IEnumerable<InternalPlanarSegment> matches)
        //{
        //    var pointSegments = matches.Select(m =>
        //            new { Intersection = LineSegment3D.PointIntersection(testSegment.Segment, m.Segment), Segment = m.Segment }).
        //            Where(a => a.Intersection is not null);

        //    var orientedSegments = new List<LineSegment3D>();
        //    foreach (var pointSegment in pointSegments)
        //    {
        //        if (pointSegment.Segment.PointIsAtAnEndpoint(pointSegment.Intersection))
        //        {
        //            orientedSegments.Add(pointSegment.Segment.Orient(pointSegment.Intersection));
        //        }
        //        else
        //        {
        //            yield return pointSegment.Intersection;
        //        }
        //    }

        //    var crossingPoints = PointOppositeSegments(orientedSegments).
        //        Where(s => s.Item2.Any(c => LineSegment3D.PointIntersection(c, testSegment.Segment) is not null)).Select(s => s.Item1);
        //    foreach (var crossingPoint in crossingPoints)
        //    {
        //        yield return crossingPoint;
        //    }
        //}

        //private List<Tuple<Point3D, LineSegment3D[]>> PointOppositeSegments(List<LineSegment3D> orientedSegments)
        //{
        //    var distincts = new List<List<Point3D>>();

        //    foreach (var orientedSegment in orientedSegments)
        //    {
        //        if (!distincts.Any(d => d[0] == orientedSegment.Start)) { distincts.Add(new List<Point3D>() { orientedSegment.Start }); }
        //        var distinct = distincts.Single(d => d[0] == orientedSegment.Start);
        //        distinct.Add(orientedSegment.End);
        //    }

        //    return distincts.Where(d => d.Count >= 3).Select(d => new Tuple<Point3D, LineSegment3D[]>(d[0],
        //        LineSegmentsFromPoints(d).ToArray())).ToList();
        //}

        //private IEnumerable<LineSegment3D> LineSegmentsFromPoints(List<Point3D> points)
        //{
        //    for (int i = 2; i < points.Count; i++)
        //    {
        //        yield return new LineSegment3D(points[1], points[i]);
        //    }
        //}

        public bool HasIntersection(PlanarSegment testSegment)
        {
            var nonLinkingSegments = GetNonLinkingSegments(Bucket.Fetch(testSegment), testSegment);
            var intersectionDistances = nonLinkingSegments.Select(m => new
            {
                Distance = Point3D.Distance(testSegment.Segment.Start, LineSegment3D.PointIntersection(m.Segment, testSegment.Segment))
            }).Where(d => Double.IsValid(d.Distance) && d.Distance > Double.ProximityError);
            return intersectionDistances.Any();
        }

        public PlanarSegment GetNearestIntersectingSegment(PlanarSegment testSegment)
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
