using BaseObjects;
using BasicObjects.GeometricObjects;
using Collections.Threading;
using Operations.Intermesh.Basics.V1;
using Console = BaseObjects.Console;

namespace Operations.Intermesh.Classes.V1
{
    internal static class CalculateIntersections
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;
            var intersectionState = new IntersectionState();
            var intersectionIterator = new Iterator<IntermeshTriangle>(intermeshTriangles.ToArray());
            intersectionIterator.RunSingle<IntersectionState, IntersectionThread>(IntersectionAction, intersectionState);
            foreach (var triangle in intermeshTriangles) { triangle.ClearNullIntersections(); }
            ConsoleLog.WriteLine($"Calculate intersections. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds. Threads {intersectionState.Threads}");
        }
        private static void IntersectionAction(IntermeshTriangle node, IntersectionThread threadState, IntersectionState state)
        {
            foreach (var gathering in node.Gathering)
            {
                var intersectionSet = node.IntersectionTable[gathering.Id];
                if (intersectionSet.IsSet) { continue; }

                var intersections = Triangle3D.LineSegmentIntersections(node.Triangle, gathering.Triangle).ToArray();

                lock (node)
                {
                    if (!intersectionSet.IsSet)
                    {
                        intersectionSet.IsSet = true;
                        intersectionSet.Intersections = intersections.Length > 0 ? intersections.Select(i =>
                            new IntermeshIntersection() { Intersection = i, IntersectorA = node, IntersectorB = gathering }).ToArray() : null;
                        if (intersectionSet.Intersections is not null) { threadState.Intersections += intersectionSet.Intersections.Length; }
                    }
                }
            }

            threadState.Gatherings += node.Gathering.Count();
        }

        internal class IntersectionThread : BaseThreadState
        {
            public int Gatherings { get; set; }
            public int Intersections { get; set; }
            public int Divisions { get; set; }
        }

        internal class IntersectionState : BaseState<IntersectionThread>
        {
            public IntersectionState()
            {
            }

            public int Gatherings { get; private set; }
            public int Intersections { get; private set; }
            public int Divisions { get; private set; }

            public override void Finish(IEnumerable<IntersectionThread> threadStates)
            {
                Gatherings = threadStates.Select(s => s.Gatherings).Sum();
                Intersections = threadStates.Select(s => s.Intersections).Sum();
                Divisions = threadStates.Select(s => s.Divisions).Sum();
            }
        }
    }
}
