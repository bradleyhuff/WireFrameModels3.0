using BaseObjects;
using BasicObjects.GeometricObjects;
using Collections.Threading;
using Operations.Basics;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Interfaces;

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
            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Calculate intersections. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds. Threads {calculationState.Threads}");
        }

        internal static void ActionSingle(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;

            var calculationState = new CalculationState();
            var calculationIterator = new Iterator<IntermeshTriangle>(intermeshTriangles.ToArray());
            calculationIterator.RunSingle<CalculationState, CalculationThread>(CalculationAction, calculationState);
            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Calculate intersections. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static void CalculationAction(IntermeshTriangle triangle, CalculationThread threadState, CalculationState state)
        {
            foreach (var gathering in triangle.Gathering)
            {
                var intersectionSet = triangle.GatheringSets[gathering.Id];
                if (intersectionSet.IsSet) { continue; }
                intersectionSet.IsSet = true;

                var gatheringTriangle = gathering.Triangle;
                var triangleTriangle = triangle.Triangle;
                if (Triangle3D.AreCoplanar(triangle.Triangle, gathering.Triangle, 1e-9))
                {
                    //gatheringTriangle = gathering.Triangle.ProjectionOnto(triangle.Triangle.Plane);
                    triangleTriangle = triangle.Triangle.ProjectionOnto(gathering.Triangle.Plane);

                    //if (Point3D.Distance(gatheringTriangle.A, gathering.Triangle.A) > 1e-12)
                    //{
                    //    BaseObjects.Console.WriteLine($"Parallel triangle distance {Point3D.Distance(gatheringTriangle.A, gathering.Triangle.A).ToString("E3")}");
                    //}
                }

                var intersections = Triangle3D.LineSegmentIntersections(triangleTriangle, gatheringTriangle).ToArray();//
                intersectionSet.Intersections = intersections;
                intersectionSet.IntersectedTriangle = triangle.Triangle;
                intersectionSet.GatheringTriangle = gathering.Triangle;
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
