using BasicObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using Operations.Basics;
using Operations.Groupings.Basics;
using Operations.Groupings.FileExportImport;
using Operations.ParallelSurfaces;
using Operations.PositionRemovals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireFrameModels3._0;

namespace Projects.Projects
{
    public class ParallelSurfaces : ProjectBase
    {
        protected override void RunProject()
        {
            ParallelSurfaceBuild();
            //SurfaceExplore();
        }

        private void ParallelSurfaceBuild()
        { 
            var grid = PntFile.Import(WireFrameMesh.Create, "Pnt/SphereDifference8 64");
            grid.RemoveShortSegments(3e-4);
            grid.RemoveCollinearEdgePoints();
            grid.RemoveCoplanarSurfacePoints();
            grid.ShowSegmentLengths();
            grid.ShowVitals();
            //WavefrontFileGroups.ExportByClusters(grid, "Wavefront/SphereDifference8");
            //PntFileGroups.ExportByClusters(grid, "Pnt/SphereDifference8");

            //Pnt/SphereDifference8-70.pnt
            //var grid = PntFile.Import(WireFrameMesh.Create, "Pnt/SphereDifference8-70");

            //var grid = PntFile.Import(WireFrameMesh.Create, "Pnt/Clusters-29");
            //TableDisplays.ShowCountSpread("Position cardinalities", grid.Positions, p => p.Cardinality);
            //WavefrontFile.Export(grid, $"Wavefront/Banana-Original");

            //var grid = PntFile.Import(WireFrameMesh.Create, "Pnt/ParallelSurfaces-29");
            //grid.Trim();
            //WavefrontFile.Export(grid, "Wavefront/ParallelSurfaces-29 Trimmed");
            //TableDisplays.ShowCountSpread("Position cardinalities", grid.Positions, p => p.Cardinality);

            //var banana = PntFile.Import(WireFrameMesh.Create, "Pnt/Banana");
            //TableDisplays.ShowCountSpread("Position cardinalities", banana.Positions, p => p.Cardinality);

            //var parallelSurfaces = grid.ParallelSurfaces(-0.01790);
            var parallelSurfaces = grid.ParallelSurfaces(-0.001750);
            //WavefrontFileGroups.ExportByFaces(parallelSurfaces, "Wavefront/ParallelSurfaces");
            //parallelSurfaces.RemoveShortSegments(1e-4);
            parallelSurfaces.Trim();
            parallelSurfaces.RemoveShortSegments(1e-4);
            parallelSurfaces.RemoveCollinearEdgePoints();
            parallelSurfaces.RemoveCoplanarSurfacePoints();
            //Console.WriteLine($"Clusters {GroupingCollection.ExtractClusters(parallelSurfaces.Triangles).Count()}");
            //PntFileGroups.ExportByClusters(parallelSurfaces, "Pnt/ParallelSurfaces");
            parallelSurfaces.ShowVitals();
            WavefrontFile.Export(parallelSurfaces, $"Wavefront/ParallelSurfaces");
            //WavefrontFileGroups.ExportByClusters(parallelSurfaces, o => {
            //    var surfaces = GroupingCollection.ExtractSurfaces(o.Triangles);
            //    Console.WriteLine($"Surfaces {string.Join(",", surfaces.Select(s => s.Triangles.Count()))}");
            //    return o; 
            //}, "Wavefront/ParallelSurfaces");
            //PntFileGroups.ExportByClusters(parallelSurfaces, "Pnt/ParallelSurfaces");
            //var surfaces = GroupingCollection.ExtractSurfaces(parallelSurfaces.Triangles).ToArray();

            //Console.WriteLine();
            //Console.WriteLine($"Surfaces {surfaces.Count()} [{string.Join(",", surfaces.Select(s => s.Triangles.Count()))}]", ConsoleColor.Cyan, ConsoleColor.DarkBlue);
            //Console.WriteLine();
        }

        private void SurfaceExplore()
        {
            var grid = PntFile.Import(WireFrameMesh.Create, "Pnt/SphereDifference8-136");
            grid.RemoveShortSegments(3e-4);
            grid.RemoveCollinearEdgePoints();
            grid.RemoveCoplanarSurfacePoints();
            var surfaces = GroupingCollection.ExtractSurfaces(grid.Triangles);

            var parallelSurfaces = grid.ParallelSurfaces(-0.00200);
            parallelSurfaces.Trim();
            parallelSurfaces.ShowOpenEdges();

            parallelSurfaces.ShowVitals();
            //Console.WriteLine($"Surfaces {string.Join(",", surfaces.Select(s => s.Triangles.Count()))}");
            //WavefrontFileGroups.ExportBySurfaces(parallelSurfaces, "Wavefront/SurfaceExplore");
            WavefrontFile.Export(parallelSurfaces, "Wavefront/SurfaceExplore");
        }
    }
}
