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

            int count = 0;
            while (true)
            {
                SegmentContactAssignments.Action(intermeshTriangles);
                var wasChanged = TriangleSegmentResolve.Action(intermeshTriangles);
                count++;

                BaseObjects.Console.WriteLine($"{count} Was changed {wasChanged}", 
                    count > 20 ? ConsoleColor.White : ConsoleColor.Gray, 
                    count > 20 ? ConsoleColor.Red : ConsoleColor.Black);
                if (!wasChanged || count > 20) { break; }
            }

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Triangle segment contact resolve. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
