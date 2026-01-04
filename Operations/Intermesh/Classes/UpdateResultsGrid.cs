using BaseObjects;
using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using Operations.Basics;
using Operations.Intermesh.Basics;
using System;

namespace Operations.Intermesh.Classes
{
    internal class UpdateResultsGrid
    {
        internal static void Action(IWireFrameMesh mesh, IEnumerable<IntermeshTriangle> processTriangles)
        {
            var start = DateTime.Now;

            var processPositionTriangles = processTriangles.Select(p => p.PositionTriangle).ToArray();
            var removalCount = mesh.RemoveAllTriangles(processPositionTriangles);
            var fillings = processTriangles.SelectMany(t => t.Fillings).ToArray();
            foreach (var filling in fillings)
            {
                filling.AddWireFrameTriangle(mesh);
            }
            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Update result grid: Triangle removals {removalCount} Fills {fillings.Length} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
