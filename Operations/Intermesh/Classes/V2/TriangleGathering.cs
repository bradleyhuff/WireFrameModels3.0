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
            ConsoleLog.WriteLine($"Triangle gathering 1. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds. Threads {gatheringState.Threads}");
            AssignIntersectionNodes(intermeshTriangles);
            ConsoleLog.WriteLine($"Triangle gathering 2. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds. Threads {gatheringState.Threads}");
        }

        private static void GatheringAction(Basics.V2.IntermeshTriangle triangle, GatheringThread threadState, GatheringState state)
        {
            var boxMatches = state.Bucket.Fetch(triangle).Where(m => m.Id != triangle.Id);
            var planarMatches = boxMatches.Where(b => triangle.Triangle.Plane.Intersects(b.Box.Margin(BoxBucket.MARGINS)));

            triangle.Gathering.AddRange(planarMatches);
        }

        private static void AssignIntersectionNodes(IEnumerable<Basics.V2.IntermeshTriangle> intermeshTriangles)
        {
            foreach (var element in intermeshTriangles)
            {
                foreach (var gathering in element.Gathering)
                {
                    if (element.GatheringSets.ContainsKey(gathering.Id)) { continue; }
                    var intersection = new Basics.V2.IntermeshIntersection();
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
            public GatheringState(IEnumerable<Basics.V2.IntermeshTriangle> triangles)
            {
                Bucket = new BoxBucket<Basics.V2.IntermeshTriangle>(triangles.ToArray());
            }

            public BoxBucket<Basics.V2.IntermeshTriangle> Bucket { get; }
        }
    }
}
