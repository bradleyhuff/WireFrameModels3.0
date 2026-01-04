using BaseObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Operations.Basics;
using Operations.Intermesh.Basics;

namespace Operations.Intermesh.Classes
{
    internal class LinkIntersections
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles, out BoxBucket<IntermeshPoint> pointsBucket, out Combination2Dictionary<IntermeshSegment> segmentTable)
        {
            DateTime start = DateTime.Now;

            pointsBucket = new BoxBucket<IntermeshPoint>();

            //Triangle vertex assignments
            foreach (var triangle in intermeshTriangles)
            {
                var a = FetchVertexPointAt(triangle.PositionTriangle.A.Position, pointsBucket);
                var b = FetchVertexPointAt(triangle.PositionTriangle.B.Position, pointsBucket);
                var c = FetchVertexPointAt(triangle.PositionTriangle.C.Position, pointsBucket);
                triangle.A = a;
                triangle.B = b;
                triangle.C = c;

                a.Add(triangle);
                b.Add(triangle);
                c.Add(triangle);
            }

            segmentTable = new Combination2Dictionary<IntermeshSegment>();

            //Triangle side assignments
            foreach (var triangle in intermeshTriangles)
            {
                if (triangle.AB == null)
                {
                    var key = new Combination2(triangle.A.Id, triangle.B.Id);
                    if (!segmentTable.ContainsKey(key)) { segmentTable[key] = new IntermeshSegment(triangle.A, triangle.B); }
                    var segment = segmentTable[key];
                    triangle.A.Add(segment);
                    triangle.B.Add(segment);
                    triangle.Add(segment);
                    segment.Add(triangle);
                }
                if (triangle.BC == null)
                {
                    var key = new Combination2(triangle.B.Id, triangle.C.Id);
                    if (!segmentTable.ContainsKey(key)) { segmentTable[key] = new IntermeshSegment(triangle.B, triangle.C); }
                    var segment = segmentTable[key];
                    triangle.B.Add(segment);
                    triangle.C.Add(segment);
                    triangle.Add(segment);
                    segment.Add(triangle);
                }
                if (triangle.CA == null)
                {
                    var key = new Combination2(triangle.C.Id, triangle.A.Id);
                    if (!segmentTable.ContainsKey(key)) { segmentTable[key] = new IntermeshSegment(triangle.C, triangle.A); }
                    var segment = segmentTable[key];
                    triangle.C.Add(segment);
                    triangle.A.Add(segment);
                    triangle.Add(segment);
                    segment.Add(triangle);
                }
            }

            // Triangle intersection segment assignments
            foreach (var triangle in intermeshTriangles)
            {
                foreach (var set in triangle.GatheringSets)
                {
                    foreach (var intersection in set.Value.Intersections ?? new LineSegment3D[0])
                    {
                        var a = FetchPointAt(intersection.Start, pointsBucket);
                        var b = FetchPointAt(intersection.End, pointsBucket);

                        if (a.Id == b.Id) { continue; }
                        var key = new Combination2(a.Id, b.Id);
                        if (!segmentTable.ContainsKey(key)) { segmentTable[key] = new IntermeshSegment(a, b); }
                        var segment = segmentTable[key];

                        a.Add(segment);
                        b.Add(segment);
                        segment.Add(triangle);
                        triangle.Add(segment);
                    }
                }
            }

            var range = 1e-9;

            // Division point assignments from intersection of segments
            foreach (var triangle in intermeshTriangles)
            {
                var gatherings = triangle.Gathering.Where(t => t.Id != triangle.Id).SelectMany(t => t.Segments).DistinctBy(g => g.Id).ToArray();
                foreach (var segment in triangle.Segments)
                {
                    var matches = gatherings.Where(g => Rectangle3D.Overlaps(g.Box, segment.Box)).Where(m => m.Key != segment.Key);
                    foreach (var match in matches)
                    {
                        var intersection = LineSegment3D.PointIntersection(match.Segment, segment.Segment, range);//
                        if (intersection is not null && triangle.Triangle.PointIsContainedOn(intersection, range))//
                        {
                            var i = FetchPointAt(intersection, pointsBucket);
                            segment.Add(i);
                            i.Add(segment);
                            segment.Add(triangle);
                        }

                        var intersection2 = LineSegment3D.Intersection(match.Segment, segment.Segment, range);//
                        if (intersection2 is not null &&
                            triangle.Triangle.PointIsContainedOn(intersection2.Start, range) &&//
                            triangle.Triangle.PointIsContainedOn(intersection2.End, range))//
                        {
                            var i = FetchPointAt(intersection2.Start, pointsBucket);
                            var j = FetchPointAt(intersection2.End, pointsBucket);
                            segment.Add(i);
                            i.Add(segment);
                            segment.Add(j);
                            j.Add(segment);
                            segment.Add(triangle);
                        }
                    }
                }
            }

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Link intersections. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        internal static IntermeshPoint FetchPointAt(Point3D point, BoxBucket<IntermeshPoint> bucket)
        {
            var match = bucket.Fetch(new Rectangle3D(point, BoxBucket.MARGINS));
            //var found = match.Where(m => Point3D.AreEqual(m.Point, point, GapConstants.Filler)).MinBy(p => Point3D.Distance(p.Point, point));
            var found = match.Where(m => Point3D.AreEqual(m.Point, point, 1e-9)).MinBy(p => Point3D.Distance(p.Point, point));
            if (found is not null)
            {
                return found;
            }
            var intermeshPoint = new IntermeshPoint(point);
            bucket.Add(intermeshPoint);
            return intermeshPoint;
        }

        private static IntermeshPoint FetchVertexPointAt(Point3D point, BoxBucket<IntermeshPoint> bucket)
        {
            var match = bucket.Fetch(new Rectangle3D(point, BoxBucket.MARGINS));
            //var found = match.Where(m => Point3D.AreEqual(m.Point, point, GapConstants.Resolution)).MinBy(p => Point3D.Distance(p.Point, point));
            var found = match.Where(m => Point3D.AreEqual(m.Point, point, 1e-16)).MinBy(p => Point3D.Distance(p.Point, point));
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
