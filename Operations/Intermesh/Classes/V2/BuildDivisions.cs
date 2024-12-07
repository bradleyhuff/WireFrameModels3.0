using BaseObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Classes.V2
{
    internal class BuildDivisions
    {
        internal static void Action(IEnumerable<Basics.V2.IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;

            foreach (var element in intermeshTriangles)
            {
                foreach (var segment in element.Segments)
                {
                    var divisions = segment.BuildDivisions();
                    element.AddRange(divisions);
                }
            }

            //var triangleTest = intermeshTriangles.FirstOrDefault(t => t.Id == 18);
            //if (triangleTest != null)
            //{
            //    var test = WireFrameMesh.Create();
            //    var more = triangleTest.ExportWithDivisionsSplit(test);
            //    WavefrontFile.Export(test, $"Wavefront/Intermesh-{triangleTest.Id}");
            //    WavefrontFile.Export(more, $"Wavefront/Intermesh-{triangleTest.Id}");
            //}

            ConsoleLog.WriteLine($"Build divisions. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
