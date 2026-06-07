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
            TriangleSegmentResolve.Action(intermeshTriangles);

            SegmentContactAssignments.Action(intermeshTriangles);
            TriangleSegmentResolve.Action(intermeshTriangles, true);

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Triangle segment contact resolve. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
