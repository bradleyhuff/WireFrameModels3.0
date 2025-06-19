using BaseObjects;
using BasicObjects.GeometricObjects;
using Collections.Threading;
using Operations.Intermesh.Basics;

namespace Operations.Intermesh.Classes
{
    internal class CalculateIntersections
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;

            var calculationState = new CalculationState();
            var calculationIterator = new Iterator<IntermeshTriangle>(intermeshTriangles.ToArray());
            calculationIterator.Run<CalculationState, CalculationThread>(CalculationAction, calculationState);
            if (GridIntermesh.ShowLog) ConsoleLog.WriteLine($"Calculate intersections. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds. Threads {calculationState.Threads}");           
        }

        internal static void ActionSingle(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;

            var calculationState = new CalculationState();
            var calculationIterator = new Iterator<IntermeshTriangle>(intermeshTriangles.ToArray());
            calculationIterator.RunSingle<CalculationState, CalculationThread>(CalculationAction, calculationState);
            if (GridIntermesh.ShowLog) ConsoleLog.WriteLine($"Calculate intersections. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static void CalculationAction(IntermeshTriangle triangle, CalculationThread threadState, CalculationState state)
        {
            foreach (var gathering in triangle.Gathering)
            {
                var intersectionSet = triangle.GatheringSets[gathering.Id];
                if (intersectionSet.IsSet) { continue; }
                intersectionSet.IsSet = true;

                var intersections = Triangle3D.LineSegmentIntersections(triangle.Triangle, gathering.Triangle).ToArray();
                intersectionSet.Intersections = intersections;
            }
            foreach (var gathering in triangle.Gathering)
            {
                var intersectionSet = triangle.GatheringSets[gathering.Id];
                var intersections = intersectionSet.Intersections;
                if (intersections is not null && intersections.Any())
                {
                    triangle.IntersectingTriangles.Add(gathering);
                }
            }
        }

        private class CalculationThread : BaseThreadState
        {
        }

        private class CalculationState : BaseState<CalculationThread>
        {
        }

    }
}
