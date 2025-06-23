using BaseObjects;
using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.Threading;
using Operations.Basics;
using Operations.Intermesh.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Classes
{
    internal class TriangleGathering
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var start = DateTime.Now;
            var gatheringState = new GatheringState(intermeshTriangles);
            var gatheringIterator = new Iterator<IntermeshTriangle>(intermeshTriangles.ToArray());
            gatheringIterator.Run<GatheringState, GatheringThread>(GatheringAction, gatheringState);
            AssignIntersectionNodes(intermeshTriangles);
            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Triangle gathering. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds. Threads {gatheringState.Threads}");
        }

        internal static void ActionSingle(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var start = DateTime.Now;
            var gatheringState = new GatheringState(intermeshTriangles);
            var gatheringIterator = new Iterator<IntermeshTriangle>(intermeshTriangles.ToArray());
            gatheringIterator.RunSingle<GatheringState, GatheringThread>(GatheringAction, gatheringState);
            AssignIntersectionNodes(intermeshTriangles);
            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Triangle gathering. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static void GatheringAction(IntermeshTriangle triangle, GatheringThread threadState, GatheringState state)
        {
            var boxMatches = state.Bucket.Fetch(triangle).Where(m => m.Id != triangle.Id);
            var planarMatches = boxMatches.Where(b => triangle.Triangle.Plane.Intersects(b.Box.Margin(BoxBucket.MARGINS)));

            triangle.Gathering.AddRange(planarMatches);
        }

        private static void AssignIntersectionNodes(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            foreach (var element in intermeshTriangles)
            {
                foreach (var gathering in element.Gathering)
                {
                    if (element.GatheringSets.ContainsKey(gathering.Id)) { continue; }
                    var intersection = new Basics.IntermeshIntersection();
                    element.GatheringSets[gathering.Id] = intersection;
                    gathering.GatheringSets[element.Id] = intersection;
                }
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
