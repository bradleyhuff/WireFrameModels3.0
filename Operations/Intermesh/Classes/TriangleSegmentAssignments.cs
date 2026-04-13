using BaseObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Operations.Basics;
using Operations.Intermesh.Basics;
using Operations.ParallelSurfaces.Internals;

namespace Operations.Intermesh.Classes
{
    internal static class TriangleSegmentAssignments
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;

            ////Triangle vertex assignments
            foreach (var triangle in intermeshTriangles)
            {
                triangle.A = IntermeshPoint.Fetch(triangle.PositionTriangle.A.Position);
                triangle.B = IntermeshPoint.Fetch(triangle.PositionTriangle.B.Position);
                triangle.C = IntermeshPoint.Fetch(triangle.PositionTriangle.C.Position);
            }

            //Triangle perimeter assignments
            var segmentTable = new Combination2Dictionary<IntermeshSegment>();
            foreach (var triangle in intermeshTriangles)
            {
                triangle.AB = triangle.AB ?? BuildSegment(triangle.A, triangle.B, segmentTable);
                triangle.BC = triangle.BC ?? BuildSegment(triangle.B, triangle.C, segmentTable);
                triangle.CA = triangle.CA ?? BuildSegment(triangle.C, triangle.A, segmentTable);
            }

            // Triangle intersection segment assignments
            foreach (var triangle in intermeshTriangles)
            {
                foreach (var set in triangle.GatheringSets)
                {
                    foreach (var intersection in set.Value.Intersections ?? new LineSegment3D[0])
                    {
                        var a = IntermeshPoint.Fetch(intersection.Start);
                        var b = IntermeshPoint.Fetch(intersection.End);

                        if (a.Id == b.Id) { continue; }
                        triangle.AddIntersection(BuildSegment(a, b, segmentTable));
                    }
                }
            }

            foreach (var triangle in intermeshTriangles) { triangle.SwitchEdges(); }

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Triangle segment assignments. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static IntermeshSegment BuildSegment(IntermeshPoint a, IntermeshPoint b, Combination2Dictionary<IntermeshSegment> table)
        {
            var key = new Combination2(a.Id, b.Id);
            if (!table.ContainsKey(key)) { table[key] = new IntermeshSegment(a, b); }
            return table[key];
        }
    }
}
