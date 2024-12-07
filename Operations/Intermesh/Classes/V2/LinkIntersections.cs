using BaseObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Basics.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            var segmentsBucket = new BoxBucket<IntermeshSegment>();
            foreach (var element in intermeshTriangles)
            {
                foreach (var gathering in element.GatheringSets.Values.SelectMany(g => g.Intersections))
                {
                    var a = FetchPointAt(gathering.Start, pointsBucket);
                    var b = FetchPointAt(gathering.End, pointsBucket);

                    if (a.Id == b.Id) { continue; }

                    var segment = FetchSegmentAt(a, b, segmentsBucket);
                    a.Add(segment);
                    b.Add(segment);
                    segment.Add(element);
                    element.Add(segment);
                }
            }

            foreach (var element in intermeshTriangles)
            {
                foreach(var segment in element.Segments)
                {
                    var matches = segmentsBucket.Fetch(segment).Where(m => m.Id != segment.Id);
                    foreach(var match in matches)
                    {
                        var intersection = LineSegment3D.PointIntersection(match.Segment, segment.Segment);
                        if(intersection is not null)
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

            ConsoleLog.WriteLine($"Link intersections. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
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

        private static IntermeshSegment FetchSegmentAt(IntermeshPoint a, IntermeshPoint b, BoxBucket<IntermeshSegment> bucket)
        {
            var key = new Combination2(a.Id, b.Id);
            var match = bucket.Fetch(Rectangle3D.Containing(a.Point, b.Point));
            var found = match.FirstOrDefault(m => m.Key == key);
            if (found is not null)
            {
                return found;
            }
            var intermeshSegment = new IntermeshSegment(a, b);
            bucket.Add(intermeshSegment);
            return intermeshSegment;
        }
    }
}
