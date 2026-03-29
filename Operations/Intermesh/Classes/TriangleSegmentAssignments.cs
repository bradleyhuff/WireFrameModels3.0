using BaseObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Operations.Basics;
using Operations.Intermesh.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Classes
{
    internal class TriangleSegmentAssignments
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;

            var pointsBucket = new BoxBucket<IntermeshPoint>();

            //Triangle vertex assignments
            foreach (var triangle in intermeshTriangles)
            {
                var a = FetchPointAt(triangle.PositionTriangle.A.Position, pointsBucket);
                var b = FetchPointAt(triangle.PositionTriangle.B.Position, pointsBucket);
                var c = FetchPointAt(triangle.PositionTriangle.C.Position, pointsBucket);
                triangle.A = a;
                triangle.B = b;
                triangle.C = c;

                //a.Add(triangle);
                //b.Add(triangle);
                //c.Add(triangle);
            }

            var segmentTable = new Combination2Dictionary<IntermeshSegment>();

            //Triangle side assignments
            foreach (var triangle in intermeshTriangles)
            {
                if (triangle.AB == null)
                {
                    var key = new Combination2(triangle.A.Id, triangle.B.Id);
                    if (!segmentTable.ContainsKey(key)) { segmentTable[key] = new IntermeshSegment(triangle.A, triangle.B); }
                    triangle.AB = segmentTable[key];
                    //var segment = segmentTable[key];
                    //triangle.A.Add(segment);
                    //triangle.B.Add(segment);
                    //triangle.Add(segment);
                    //segment.Add(triangle);
                }
                if (triangle.BC == null)
                {
                    var key = new Combination2(triangle.B.Id, triangle.C.Id);
                    if (!segmentTable.ContainsKey(key)) { segmentTable[key] = new IntermeshSegment(triangle.B, triangle.C); }
                    triangle.BC = segmentTable[key];
                    //triangle.B.Add(segment);
                    //triangle.C.Add(segment);
                    //triangle.Add(segment);
                    //segment.Add(triangle);
                }
                if (triangle.CA == null)
                {
                    var key = new Combination2(triangle.C.Id, triangle.A.Id);
                    if (!segmentTable.ContainsKey(key)) { segmentTable[key] = new IntermeshSegment(triangle.C, triangle.A); }
                    triangle.CA = segmentTable[key];
                    //triangle.C.Add(segment);
                    //triangle.A.Add(segment);
                    //triangle.Add(segment);
                    //segment.Add(triangle);
                }
            }

            //BaseObjects.Console.WriteLine($"New Intersections: {intermeshTriangles.Sum(t => t.GatheringSets.Sum(g => g.Value.Intersections.Count()))}");

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

                        //a.Add(segment);
                        //b.Add(segment);
                        //segment.Add(triangle);
                        triangle.AddIntersection(segment);
                    }
                }
            }

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Triangle segment assignments. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        internal static IntermeshPoint FetchPointAt(Point3D point, BoxBucket<IntermeshPoint> bucket)
        {
            var match = bucket.Fetch(new Rectangle3D(point, BoxBucket.MARGINS));
            var found = match.Where(m => Point3D.AreEqual(m.Point, point, 1e-15)).MinBy(p => Point3D.Distance(p.Point, point));
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
