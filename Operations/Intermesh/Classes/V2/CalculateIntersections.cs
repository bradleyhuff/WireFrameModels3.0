using BaseObjects;
using BasicObjects.GeometricObjects;
using Collections.Threading;
using Operations.Intermesh.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Operations.Intermesh.Classes.CalculateIntersections;

namespace Operations.Intermesh.Classes.V2
{
    internal class CalculateIntersections
    {
        internal static void Action(IEnumerable<Basics.V2.IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;
            foreach (var element in intermeshTriangles)
            {
                foreach (var gathering in element.Gathering)
                {
                    var intersectionSet = element.GatheringSets[gathering.Id];
                    if (intersectionSet.IsSet) { continue; }

                    var intersections = Triangle3D.LineSegmentIntersections(element.Triangle, gathering.Triangle).ToArray();
                    intersectionSet.Intersections = intersections;
                    intersectionSet.IsSet = true;

                }
            }
            ConsoleLog.WriteLine($"Calculate intersections. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

    }
}
