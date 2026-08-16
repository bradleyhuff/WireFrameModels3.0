using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Operations.Intermesh.Basics;
using Operations.Regions;
using Double = BasicObjects.Math.Double;

namespace Operations.PlanarFilling.Filling.Internals
{
    internal class PlanarRegions<T>
    {
        private double _testSegmentLength = 0;
        private IEnumerable<PlanarSegment<T>> _segments;
        private Plane _plane;

        public PlanarRegions(Plane plane, double testSegmentLength)
        {
            _plane = plane;
            _testSegmentLength = testSegmentLength;
        }

        private BoxBucket<PlanarSegment<T>> _bucket = null;

        private BoxBucket<PlanarSegment<T>> Bucket
        {
            get
            {
                if (_bucket is null)
                {
                    _bucket = new BoxBucket<PlanarSegment<T>>(_segments.ToArray());
                }
                return _bucket;
            }
        }

        public void Load(IEnumerable<PlanarSegment<T>> segments)
        {
            _segments = segments;
            _bucket = null;
        }

        public bool CrossesInterior(PlanarSegment<T> testSegment)
        {
            return RegionOfTestSegment(testSegment) == Region.Interior;
        }

        public bool IsAtBoundary(PlanarSegment<T> testSegment)
        {
            return RegionOfTestSegment(testSegment) == Region.OnBoundary;
        }

        public bool CrossesInteriorOrAtBoundary(PlanarSegment<T> testSegment)
        {
            var region = RegionOfTestSegment(testSegment);
            return region == Region.Interior || region == Region.OnBoundary;
        }

        private Region RegionOfTestSegment(PlanarSegment<T> testSegment)
        {
            var testPoint = 0.5 * testSegment.Segment.Start + 0.5 * testSegment.Segment.End;
            return RegionOfProjectedPoint(testPoint);
        }

        public Region RegionOfProjectedPoint(Point3D point)
        {
            point = _plane.Projection(point);

            var xPlane = new Plane(point, Vector3D.BasisX);
            var yPlane = new Plane(point, Vector3D.BasisY);
            var zPlane = new Plane(point, Vector3D.BasisZ);
            var crossX = Plane.Cross(xPlane, _plane);
            var crossY = Plane.Cross(yPlane, _plane);
            var crossZ = Plane.Cross(zPlane, _plane);

            var sequence =  new (double Magnitude, Plane Plane)[] { (crossX.Magnitude, xPlane), (crossY.Magnitude, yPlane), (crossZ.Magnitude, zPlane) };
            sequence = sequence.Where(o => o.Magnitude > 1e-9).OrderByDescending(o => o.Magnitude).ToArray();

            foreach (var direction in sequence)
            {
                var region = GetRegion(point, direction.Plane);
                if (region != Region.Indeterminant) { return region; }
            }
            Console.WriteLine($"Indeterminant point {point}");
            return Region.Indeterminant;
        }

        private Region GetRegion(Point3D point, Plane directionPlane)
        {
            var line = Plane.Intersection(directionPlane, _plane);
            var testSegment = new PlanarSegment<T>(point + -_testSegmentLength * line.Vector.Direction, point + _testSegmentLength * line.Vector.Direction);
            var matches = GetNonLinkingSegments(Bucket.Fetch(testSegment), testSegment).ToArray();
            if (IsOnBoundary(point, matches)) { return Region.OnBoundary; }
            var checkingPoints = GetCheckingPoints(line, matches).ToArray();
            return Manifold.GetRegion(point, checkingPoints);
        }

        private IEnumerable<PlanarSegment<T>> GetNonLinkingSegments(IEnumerable<PlanarSegment<T>> matches, PlanarSegment<T> segment)
        {
            return matches.Where(n => LineSegment3D.IsNonLinking(n.Segment, segment.Segment));
        }

        private bool IsOnBoundary(Point3D point, IEnumerable<PlanarSegment<T>> matches)
        {
            foreach (var match in matches)
            {
                if (match.Segment.PointIsOnSegment(point, GapConstants.Proximity)) { return true; }
            }
            return false;
        }

        private IEnumerable<Point3D> GetCheckingPoints(Line3D test, IEnumerable<PlanarSegment<T>> matches)
        {
            SeparateSegments(test, matches, out List<PlanarSegment<T>> links, out List<PlanarSegment<T>> collinears);

            var checkingPoints = GetIntersections(test, links).ToArray();// regular point by point checking

            var groups = BuildCollinearGroups(collinears, links);
            if (!groups.Any()) { return checkingPoints; }
            var applicableGroups = groups.Where(g => g.IsApplicable()).ToArray();
            checkingPoints = checkingPoints.Where(c => !groups.Any(g => g.Segment.PointIsOnSegment(c))).ToArray();
            checkingPoints = checkingPoints.Concat(applicableGroups.Select(ag => ag.GetApplicablePoint())).ToArray();
            return checkingPoints;
        }

        private void SeparateSegments(Line3D test, IEnumerable<PlanarSegment<T>> matches, out List<PlanarSegment<T>> links, out List<PlanarSegment<T>> collinears)
        {
            links = new List<PlanarSegment<T>>();
            collinears = new List<PlanarSegment<T>>();
            foreach (var match in matches)
            {
                if (test.SegmentIsOnLine(match.Segment)) { collinears.Add(match); }
                else
                {
                    links.Add(match);
                }
            }
        }

        private IEnumerable<Point3D> GetIntersections(Line3D test, IEnumerable<PlanarSegment<T>> links)
        {
            foreach (var link in links)
            {
                var intersection = Line3D.PointIntersectionOpen(link.Segment, test);
                if (intersection is null) { continue; }
                yield return intersection;
            }

            var rays = GetTouchingRays(test, links).ToArray();

            var groups = rays.GroupBy(r => r.Point).Where(g => g.Count() == 2).ToArray();

            foreach(var group in groups)
            {
                var testRays = group.ToArray();
                var testPoints = testRays.Select(r => r.Point + r.Normal).ToArray();
                var testSegment = new LineSegment3D(testPoints[0], testPoints[1]);

                var intersection = Line3D.PointIntersection(testSegment, test);
                if(intersection is not null)
                {
                    yield return group.Key;
                }
            }
        }

        private IEnumerable<Ray3D> GetTouchingRays(Line3D test, IEnumerable<PlanarSegment<T>> links)
        {
            foreach(var link in links)
            {
                if (test.PointIsOnLine(link.Segment.Start) && !test.PointIsOnLine(link.Segment.End)) 
                {
                    yield return new Ray3D(link.Segment.Start, (link.Segment.End - link.Segment.Start).Direction);
                }
                if (test.PointIsOnLine(link.Segment.End) && !test.PointIsOnLine(link.Segment.Start))
                {
                    yield return new Ray3D(link.Segment.End, (link.Segment.Start - link.Segment.End).Direction);
                }
            }
        }

        private List<CollinearGroup<T>> BuildCollinearGroups(IEnumerable<PlanarSegment<T>> collinears, IEnumerable<PlanarSegment<T>> links)
        {
            List<CollinearGroup<T>> groups = new List<CollinearGroup<T>>();
            var nonLinkedCollinears = collinears.ToList();

            while (nonLinkedCollinears.Any())
            {
                var group = new CollinearGroup<T>(_plane);
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

        public bool HasIntersection(PlanarSegment<T> testSegment)
        {
            var nonLinkingSegments = GetNonLinkingSegments(Bucket.Fetch(testSegment), testSegment);
            var intersectionDistances = nonLinkingSegments.Select(m => new
            {
                Distance = Point3D.Distance(testSegment.Segment.Start, LineSegment3D.PointIntersection(m.Segment, testSegment.Segment))
            }).Where(d => Double.IsValid(d.Distance) && d.Distance > GapConstants.Proximity);
            return intersectionDistances.Any();
        }

        public PlanarSegment<T> GetNearestIntersectingSegment(PlanarSegment<T> testSegment)
        {
            var nonLinkingSegments = GetNonLinkingSegments(Bucket.Fetch(testSegment), testSegment);

            var intersectionDistances = nonLinkingSegments.Select(m => new
            {
                Segment = m,
                Distance = Point3D.Distance(testSegment.Segment.Start, LineSegment3D.PointIntersection(m.Segment, testSegment.Segment))
            }).Where(d => Double.IsValid(d.Distance) && d.Distance > GapConstants.Proximity);

            if (!intersectionDistances.Any()) { return null; }

            var minDistance = intersectionDistances.Min(d => d.Distance);

            return intersectionDistances.FirstOrDefault(d => d.Distance == minDistance)?.Segment;
        }
    }
}
