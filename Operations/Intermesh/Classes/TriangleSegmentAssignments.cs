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

            //Triangle perimeter assignments
            var segments  = new Combination2Dictionary<IntermeshSegment>();
            var slots = new Combination2Dictionary<IntermeshEdgeSlot>();
            foreach (var triangle in intermeshTriangles)
            {
                var a = IntermeshPointExtensions.Fetch(triangle.PositionTriangle.A.Position);
                var b = IntermeshPointExtensions.Fetch(triangle.PositionTriangle.B.Position);
                var c = IntermeshPointExtensions.Fetch(triangle.PositionTriangle.C.Position);

                triangle.AB = triangle.AB ?? FetchEdgeSlot(FetchSegment(a, b, segments), slots);
                triangle.BC = triangle.BC ?? FetchEdgeSlot(FetchSegment(b, c, segments), slots);
                triangle.CA = triangle.CA ?? FetchEdgeSlot(FetchSegment(c, a, segments), slots);
            }

            // Triangle intersection segment assignments
            foreach (var triangle in intermeshTriangles)
            {
                foreach (var set in triangle.GatheringSets)
                {
                    foreach (var intersection in set.Value.Intersections ?? new LineSegment3D[0])
                    {
                        var a = IntermeshPointExtensions.Fetch(intersection.Start);
                        var b = IntermeshPointExtensions.Fetch(intersection.End);

                        if (a.Id == b.Id) { continue; }
                        triangle.AddIntersectionSlot(FetchEdgeSlot(FetchSegment(a, b, segments), slots));
                    }
                }
            }

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Triangle segment assignments. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static IntermeshEdgeSlot FetchEdgeSlot(IntermeshSegment segment, Combination2Dictionary<IntermeshEdgeSlot> slots)
        {
            var key = segment.Key;
            if (!slots.ContainsKey(key)) { slots[key] = new IntermeshEdgeSlot(segment); }
            return slots[key];
        }

        private static IntermeshSegment FetchSegment(IntermeshPoint a, IntermeshPoint b, Combination2Dictionary<IntermeshSegment> segments)
        {
            var key = new Combination2(a.Id, b.Id);
            if (!segments.ContainsKey(key)) { segments[key] = new IntermeshSegment(a, b); }
            return segments[key];
        }
    }
}
