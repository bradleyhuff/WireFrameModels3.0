using BaseObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Operations.Basics;
using Operations.Intermesh.Basics;
using Console = BaseObjects.Console;

namespace Operations.Intermesh.Classes
{
    internal class LinkIntersections
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;

            var pointsBucket = new BoxBucket<IntermeshPoint>();

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

            var table = new Combination2Dictionary<IntermeshSegment>();

            //Triangle side assignments
            foreach (var triangle in intermeshTriangles)
            {
                if (triangle.AB == null)
                {
                    var key = new Combination2(triangle.A.Id, triangle.B.Id);
                    if (!table.ContainsKey(key)) { table[key] = new IntermeshSegment(triangle.A, triangle.B); }
                    var segment = table[key];
                    triangle.A.Add(segment);
                    triangle.B.Add(segment);
                    triangle.Add(segment);
                }
                if (triangle.BC == null)
                {
                    var key = new Combination2(triangle.B.Id, triangle.C.Id);
                    if (!table.ContainsKey(key)) { table[key] = new IntermeshSegment(triangle.B, triangle.C); }
                    var segment = table[key];
                    triangle.B.Add(segment);
                    triangle.C.Add(segment);
                    triangle.Add(segment);
                }
                if (triangle.CA == null)
                {
                    var key = new Combination2(triangle.C.Id, triangle.A.Id);
                    if (!table.ContainsKey(key)) { table[key] = new IntermeshSegment(triangle.C, triangle.A); }
                    var segment = table[key];
                    triangle.C.Add(segment);
                    triangle.A.Add(segment);
                    triangle.Add(segment);
                }
            }

            //ConsoleLog.WriteLine($"Link intersections 1. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");

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
                        if (!table.ContainsKey(key)) { table[key] = new IntermeshSegment(a, b); }
                        var segment = table[key];

                        var segment2 = segment.Segment;
                        if (!set.Value.IntersectedTriangle.PointIsOn(intersection.Start) || !set.Value.IntersectedTriangle.PointIsOn(intersection.End))
                        {
                                 Console.WriteLine($"Linking Action {triangle.Id} {set.Key} Intersection Id {set.Value.Id}\n{set.Value.IntersectedTriangle}\n{set.Value.GatheringTriangle}");
                        }

                        a.Add(segment);
                        b.Add(segment);
                        segment.Add(triangle);
                        triangle.Add(segment);
                    }
                }
            }
            //ConsoleLog.WriteLine($"Link intersections 2. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");

            var segmentTable = new Dictionary<int, bool>();

            // Division point assignments from intersection of segments
            foreach (var triangle in intermeshTriangles)
            {
                var gatherings = triangle.Gathering.Where(t => t.Id != triangle.Id).SelectMany(t => t.Segments).DistinctBy(g => g.Id).ToArray();
                foreach (var segment in triangle.Segments)
                {
                    if (segmentTable.ContainsKey(segment.Id)) { continue; }
                    segmentTable[segment.Id] = true;

                    foreach (var match in gatherings.Where(g => Rectangle3D.Overlaps(g.Box, segment.Box)))
                    {
                        var intersection = LineSegment3D.PointIntersection(match.Segment, segment.Segment);
                        if (intersection is not null && triangle.Triangle.PointIsContainedOn(intersection))
                        {
                            var i = FetchPointAt(intersection, pointsBucket);
                            segment.Add(i);
                            i.Add(segment);
                            continue;
                        }

                        var intersection2 = LineSegment3D.Intersection(match.Segment, segment.Segment);
                        if (intersection2 is not null &&
                            triangle.Triangle.PointIsContainedOn(intersection2.Start) &&
                            triangle.Triangle.PointIsContainedOn(intersection2.End))
                        {
                            var i = FetchPointAt(intersection2.Start, pointsBucket);
                            var j = FetchPointAt(intersection2.End, pointsBucket);
                            segment.Add(i);
                            i.Add(segment);
                            segment.Add(j);
                            j.Add(segment);
                        }
                    }
                }
            }

            //Consolidate near collinear segments to division points

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Link intersections. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static IntermeshPoint FetchPointAt(Point3D point, BoxBucket<IntermeshPoint> bucket)
        {
            var match = bucket.Fetch(new Rectangle3D(point, BoxBucket.MARGINS));
            var found = match.Where(m => Point3D.AreEqual(m.Point, point, GapConstants.Resolution)).MinBy(p => Point3D.Distance(p.Point, point));
            if (found is not null)
            {
                return found;
            }
            var intermeshPoint = new IntermeshPoint(point, false);
            bucket.Add(intermeshPoint);
            return intermeshPoint;
        }

        private static IntermeshPoint FetchVertexPointAt(Point3D point, BoxBucket<IntermeshPoint> bucket)
        {
            var match = bucket.Fetch(new Rectangle3D(point, BoxBucket.MARGINS));
            var found = match.Where(m => Point3D.AreEqual(m.Point, point, GapConstants.Resolution)).MinBy(p => Point3D.Distance(p.Point, point));
            if (found is not null)
            {
                return found;
            }
            var intermeshPoint = new IntermeshPoint(point, true);
            bucket.Add(intermeshPoint);
            return intermeshPoint;
        }
    }
}
