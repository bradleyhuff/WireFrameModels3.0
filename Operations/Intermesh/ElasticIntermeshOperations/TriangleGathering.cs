using BasicObjects;
using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.Threading;
using Operations.Intermesh.Basics;
using Console = BaseObjects.Console;

namespace Operations.Intermesh.ElasticIntermeshOperations
{
    internal static class TriangleGathering
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var start = DateTime.Now;
            var gatheringState = new GatheringState(intermeshTriangles);
            var gatheringIterator = new Iterator<IntermeshTriangle>(intermeshTriangles.ToArray());
            gatheringIterator.Run<GatheringState, GatheringThread>(GatheringAction, gatheringState);
            AssignIntersectionNodes(intermeshTriangles);
            Console.Write("Intermesh: ", ConsoleColor.Cyan);
            Console.WriteLine($"Triangle gathering. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds. Threads {gatheringState.Threads}", ConsoleColor.Magenta);

            var totalMatches = intermeshTriangles.Sum(t => t.Gathering.Count());
            var maxMatches = intermeshTriangles.Max(t => t.Gathering.Count());
            //Console.WriteLine($"Total matches {totalMatches} Average {totalMatches / intermeshTriangles.Count()} Max {maxMatches}");
            //TableDisplays.ShowCountSpread("Gathering", intermeshTriangles, t => t.Gathering.Count());
        }

        private static void GatheringAction(IntermeshTriangle triangle, GatheringThread threadState, GatheringState state)
        {
            var boxMatches = state.Bucket.Fetch(triangle).Where(m => m.Id != triangle.Id && !m.AdjacentTriangles.Any(t => t.Id == triangle.Id));
            var planarMatches = boxMatches.Where(b => triangle.Triangle.Plane.Intersects(b.Box.Margin(1e-6)));
            var marginMatches = MarginMatches(triangle, planarMatches);

            triangle.Gathering.AddRange(marginMatches);
        }

        private static void AssignIntersectionNodes(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            foreach (var node in intermeshTriangles)
            {
                foreach (var gathering in node.Gathering)
                {
                    var intersection = new IntermeshIntersection();
                    node.IntersectionTable[gathering.Id] = intersection;
                    gathering.IntersectionTable[node.Id] = intersection;
                }
            }
        }

        private static IEnumerable<IntermeshTriangle> MarginMatches(IntermeshTriangle node, IEnumerable<IntermeshTriangle> matches)
        {
            var triangle = node.Triangle.Margin(1e-6);
            var plane = triangle.Plane;

            foreach (var match in matches)
            {
                var projectionA = triangle.EdgeAB.LineExtension.Projection(triangle.C);
                var planeA = new Plane(projectionA, (triangle.C - projectionA).Direction);

                var projectionB = triangle.EdgeBC.LineExtension.Projection(triangle.A);
                var planeB = new Plane(projectionB, (triangle.A - projectionB).Direction);

                var projectionC = triangle.EdgeCA.LineExtension.Projection(triangle.B);
                var planeC = new Plane(projectionC, (triangle.B - projectionC).Direction);

                if (match.Triangle.Verticies.All(v => !planeA.PointIsFrontOfPlane(v))) { continue; }
                if (match.Triangle.Verticies.All(v => !planeB.PointIsFrontOfPlane(v))) { continue; }
                if (match.Triangle.Verticies.All(v => !planeC.PointIsFrontOfPlane(v))) { continue; }

                yield return match;
            }
        }

        private class GatheringThread : BaseThreadState
        {
        }

        private class GatheringState : BaseState<GatheringThread>
        {
            public GatheringState(IEnumerable<IntermeshTriangle> triangles)
            {
                Bucket = new BoxBucket<IntermeshTriangle>(triangles.ToArray());
            }

            public BoxBucket<IntermeshTriangle> Bucket { get; }
        }
    }
}
