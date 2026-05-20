using BaseObjects;
using Collections.WireFrameMesh.Interfaces;
using Operations.Basics;
using Operations.Intermesh.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Classes
{
    internal class UpdateResultsGrid
    {
        internal static void Action(IWireFrameMesh mesh, IEnumerable<IntermeshTriangle> processTriangles)
        {
            var start = DateTime.Now;


            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Update result grid: Triangle removals:  Fills:  Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
