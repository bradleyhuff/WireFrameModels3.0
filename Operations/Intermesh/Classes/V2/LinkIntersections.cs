using BaseObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Basics.V2;

namespace Operations.Intermesh.Classes.V2
{
    internal class LinkIntersections
    {
        internal static void Action(IEnumerable<Basics.V2.IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;
            var pointsBucket = new BoxBucket<IntermeshPoint>();
            foreach (var element in intermeshTriangles)
            {
                var a = FetchPointAt(element.PositionTriangle.A.Position, pointsBucket);
                var b = FetchPointAt(element.PositionTriangle.B.Position, pointsBucket);
                var c = FetchPointAt(element.PositionTriangle.C.Position, pointsBucket);
                element.A = a;
                element.B = b;
                element.C = c;

                a.Add(element);
                b.Add(element);
                c.Add(element);
            }

            ConsoleLog.WriteLine($"Link intersections 1. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");

            var table = new Combination2Dictionary<IntermeshSegment>();
            foreach (var element in intermeshTriangles)
            {
                foreach (var gathering in element.GatheringSets.Values.SelectMany(g => g.Intersections))
                {
                    var a = FetchPointAt(gathering.Start, pointsBucket);
                    var b = FetchPointAt(gathering.End, pointsBucket);

                    if (a.Id == b.Id) { continue; }
                    var key = new Combination2(a.Id, b.Id);
                    if (!table.ContainsKey(key)) { table[key] = new IntermeshSegment(a, b); }

                    var segment = table[key];

                    a.Add(segment);
                    b.Add(segment);
                    segment.Add(element);
                    element.Add(segment);
                }
            }

            ConsoleLog.WriteLine($"Link intersections 2. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
            var segmentTable = new Dictionary<int, bool>();
            foreach(var triangle in intermeshTriangles)
            {
                var gatherings = triangle.Gathering.Where(t => t.Id != triangle.Id).SelectMany(t => t.Segments).DistinctBy(g => g.Id).ToArray();
                foreach(var segment in triangle.Segments)
                {
                    if (segmentTable.ContainsKey(segment.Id)) { continue; }
                    segmentTable[segment.Id] = true;

                    foreach(var match in gatherings.Where(g => Rectangle3D.Overlaps(g.Box, segment.Box)))
                    {
                        var intersection = LineSegment3D.PointIntersection(match.Segment, segment.Segment);
                        if (intersection is not null)
                        {
                            var i = FetchPointAt(intersection, pointsBucket);
                            segment.Add(i);
                            i.Add(segment);
                            continue;
                        }

                        var intersection2 = LineSegment3D.LineSegmentIntersection(match.Segment, segment.Segment);
                        if (intersection2 is not null)
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

            ConsoleLog.WriteLine($"Link intersections 3. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static IntermeshPoint FetchPointAt(Point3D point, BoxBucket<IntermeshPoint> bucket)
        {
            var match = bucket.Fetch(new Rectangle3D(point, BoxBucket.MARGINS));
            var found = match.FirstOrDefault(m => Point3D.AreEqual(m.Point, point, GapConstants.Resolution));
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
