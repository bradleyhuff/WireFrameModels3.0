using BaseObjects;
using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.Threading;
using Operations.Intermesh.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Classes.V2
{
    internal class TriangleGathering
    {
        internal static void Action(IEnumerable<Basics.V2.IntermeshTriangle> intermeshTriangles)
        {
            var start = DateTime.Now;
            var gatheringState = new GatheringState(intermeshTriangles);
            var gatheringIterator = new Iterator<Basics.V2.IntermeshTriangle>(intermeshTriangles.ToArray());
            gatheringIterator.Run<GatheringState, GatheringThread>(GatheringAction, gatheringState);
            AssignIntersectionNodes(intermeshTriangles);
            ConsoleLog.WriteLine($"Triangle gathering. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds. Threads {gatheringState.Threads}");
        }

        private static void GatheringAction(Basics.V2.IntermeshTriangle triangle, GatheringThread threadState, GatheringState state)
        {
            var boxMatches = state.Bucket.Fetch(triangle).Where(m => m.Id != triangle.Id);
            var planarMatches = boxMatches.Where(b => triangle.Triangle.Plane.Intersects(b.Box.Margin(BoxBucket.MARGINS)));
            var marginMatches = MarginMatches(triangle, planarMatches);

            triangle.Gathering.AddRange(marginMatches);
        }

        private static void AssignIntersectionNodes(IEnumerable<Basics.V2.IntermeshTriangle> intermeshTriangles)
        {
            foreach (var element in intermeshTriangles)
            {
                foreach (var gathering in element.Gathering)
                {
                    var intersection = new Basics.V2.IntermeshIntersection();
                    element.GatheringSets[gathering.Id] = intersection;
                    gathering.GatheringSets[element.Id] = intersection;
                }
            }
        }

        private static IEnumerable<Basics.V2.IntermeshTriangle> MarginMatches(Basics.V2.IntermeshTriangle node, IEnumerable<Basics.V2.IntermeshTriangle> matches)
        {
            var triangle = node.Triangle.Margin(BoxBucket.MARGINS);
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
            public GatheringState(IEnumerable<Basics.V2.IntermeshTriangle> triangles)
            {
                Bucket = new BoxBucket<Basics.V2.IntermeshTriangle>(triangles.ToArray());
            }

            public BoxBucket<Basics.V2.IntermeshTriangle> Bucket { get; }
        }
    }
}
