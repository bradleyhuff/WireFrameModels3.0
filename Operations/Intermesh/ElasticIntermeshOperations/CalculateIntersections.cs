using BaseObjects;
using BasicObjects.GeometricObjects;
using Collections.Threading;
using Operations.Intermesh.Basics;
using Console = BaseObjects.Console;

namespace Operations.Intermesh.ElasticIntermeshOperations
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

            //Console.WriteLine($"Non distinct intersections {intermeshTriangles.SelectMany(t => t.Intersections).Count()}");
            //var intersectionNodes = intermeshTriangles.SelectMany(t => t.Intersections).DistinctBy(t => t.Id);
            //Console.WriteLine($"Gatherings {intersectionState.Gatherings} Intersections {intersectionNodes.Count()}:{intersectionState.Intersections}");
            //Console.WriteLine();
        }
        private static void IntersectionAction(IntermeshTriangle node, IntersectionThread threadState, IntersectionState state)
        {
            foreach (var gathering in node.Gathering)
            {
                var intersectionNode = node.IntersectionTable[gathering.Id];
                if (intersectionNode.IsSet) { continue; }

                var intersection = Triangle3D.LineSegmentIntersections(node.Triangle, gathering.Triangle).FirstOrDefault();

                lock (node)
                {
                    if (!intersectionNode.IsSet)
                    {
                        intersectionNode.IsSet = true;
                        intersectionNode.Intersection = intersection;
                        intersectionNode.IntersectorA = node;
                        intersectionNode.IntersectorB = gathering;
                        if (intersectionNode.Intersection is not null) { threadState.Intersections++; }
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
