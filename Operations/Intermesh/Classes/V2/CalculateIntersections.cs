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
using System.Xml.Linq;
using static Operations.Intermesh.Classes.V1.CalculateIntersections;

namespace Operations.Intermesh.Classes.V2
{
    internal class CalculateIntersections
    {
        internal static void Action(IEnumerable<Basics.V2.IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;

            var calculationState = new CalculationState();
            var calculationIterator = new Iterator<Basics.V2.IntermeshTriangle>(intermeshTriangles.ToArray());
            calculationIterator.Run<CalculationState, CalculationThread>(CalculationAction, calculationState);
            ConsoleLog.WriteLine($"Calculate intersections. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds. Threads {calculationState.Threads}");           
        }

        private static void CalculationAction(Basics.V2.IntermeshTriangle triangle, CalculationThread threadState, CalculationState state)
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
