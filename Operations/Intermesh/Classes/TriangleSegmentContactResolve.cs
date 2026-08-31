using BaseObjects;
using Operations.Basics;
using Operations.Intermesh.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Classes
{
    internal class TriangleSegmentContactResolve
    {
        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;

            SegmentContactAssignments.Action(intermeshTriangles);
            var wasChanged = TriangleSegmentResolve.Action(intermeshTriangles);
            BaseObjects.Console.WriteLine($"1 Was changed {wasChanged}");
            if (!wasChanged) return;

            SegmentContactAssignments.Action(intermeshTriangles);
            wasChanged = TriangleSegmentResolve.Action(intermeshTriangles);
            BaseObjects.Console.WriteLine($"2 Was changed {wasChanged}");
            if (!wasChanged) return;

            SegmentContactAssignments.Action(intermeshTriangles);
            wasChanged = TriangleSegmentResolve.Action(intermeshTriangles);
            BaseObjects.Console.WriteLine($"3 Was changed {wasChanged}");
            if (!wasChanged) return;

            SegmentContactAssignments.Action(intermeshTriangles);
            wasChanged = TriangleSegmentResolve.Action(intermeshTriangles);
            BaseObjects.Console.WriteLine($"4 Was changed {wasChanged}");
            if (!wasChanged) return;

            SegmentContactAssignments.Action(intermeshTriangles);
            wasChanged = TriangleSegmentResolve.Action(intermeshTriangles);
            BaseObjects.Console.WriteLine($"5 Was changed {wasChanged}");
            if (!wasChanged) return;

            //SegmentContactAssignments.Action(intermeshTriangles);
            wasChanged = TriangleSegmentResolve.Action(intermeshTriangles);
            BaseObjects.Console.WriteLine($"6 Was changed {wasChanged}");
            if (!wasChanged) return;

            //SegmentContactAssignments.Action(intermeshTriangles);
            wasChanged = TriangleSegmentResolve.Action(intermeshTriangles);
            BaseObjects.Console.WriteLine($"7 Was changed {wasChanged}");
            if (!wasChanged) return;

            //SegmentContactAssignments.Action(intermeshTriangles);
            wasChanged = TriangleSegmentResolve.Action(intermeshTriangles);
            BaseObjects.Console.WriteLine($"8 Was changed {wasChanged}");
            if (!wasChanged) return;

            //SegmentContactAssignments.Action(intermeshTriangles);
            wasChanged = TriangleSegmentResolve.Action(intermeshTriangles);
            BaseObjects.Console.WriteLine($"9 Was changed {wasChanged}");
            if (!wasChanged) return;

            //SegmentContactAssignments.Action(intermeshTriangles);
            wasChanged = TriangleSegmentResolve.Action(intermeshTriangles);
            BaseObjects.Console.WriteLine($"10 Was changed {wasChanged}");

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Triangle segment contact resolve. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
