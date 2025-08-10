using BaseObjects;
using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using Operations.Basics;
using Operations.Intermesh.Basics;

namespace Operations.Intermesh.Classes
{
    internal class UpdateResultsGrid
    {
        internal static void Action(IWireFrameMesh mesh, IEnumerable<IntermeshTriangle> processTriangles)
        {
            var start = DateTime.Now;

            //var t3309 = processTriangles.SingleOrDefault(t => t.Id == 3309);
            //if (t3309 is not null)
            //{
            //    var grid = t3309.ExportWithMinimumHeightScale();
            //    var center = t3309.Triangle.A;

            //    grid.Apply(Transform.Translation(Point3D.Zero - center));
            //    grid.Apply(Transform.Scale(1e1));
            //    WavefrontFile.Export(grid, $"Wavefront/ProblemTriangle-{t3309.Id}B");
            //    foreach (var filler in t3309.Fillings)
            //    {
            //        var grid1 = t3309.ExportWithMinimumHeightScale(filler.Triangle);
            //        grid1.Apply(Transform.Translation(Point3D.Zero - center));
            //        grid1.Apply(Transform.Scale(1e1));
            //        WavefrontFile.Export(grid1, $"Wavefront/ProblemFillTriangle-{filler.Id}B");
            //    }
            //}

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
