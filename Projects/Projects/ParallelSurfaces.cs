using BasicObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
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
            //var grid = PntFile.Import(WireFrameMesh.Create, "Pnt/SphereDifference8-62");
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
            //var parallelSurfaces = grid.ParallelSurfaces(-0.001750);
            //var parallelSurfaces = grid.ParallelSurfaces(-0.001900);//check indeterminancies  search 'Indeterminant point' RESOLVED
            //var parallelSurfaces = grid.ParallelSurfaces(-0.01000);//extra surface RESOLVED
            //var parallelSurfaces = grid.ParallelSurfaces(-0.01700);//filling errors on point removal
            //var parallelSurfaces = grid.ParallelSurfaces(-0.02200);//multiple surfaces start showing
            var parallelSurfaces = grid.ParallelSurfaces(-0.01000);
            //WavefrontFileGroups.ExportByFaces(parallelSurfaces, "Wavefront/ParallelSurfaces");
            //parallelSurfaces.RemoveShortSegments(1e-4);
            parallelSurfaces.Trim();
            parallelSurfaces.RemoveShortSegments(1e-4);
            parallelSurfaces.RemoveCollinearEdgePoints();
            parallelSurfaces.RemoveCoplanarSurfacePoints();

            //var extra = parallelSurfaces.Triangles.SingleOrDefault(t => t.Id == 6240);
            //if (extra is not null)
            //{
            //    parallelSurfaces.RemoveTriangle(extra);
            //}

            //Console.WriteLine($"Clusters {GroupingCollection.ExtractClusters(parallelSurfaces.Triangles).Count()}");

            parallelSurfaces.ShowVitals();
            //parallelSurfaces.ShowOpenEdges();
            WavefrontFile.Export(parallelSurfaces, $"Wavefront/ParallelSurfaces");
            //PntFileGroups.ExportBySurfaces(parallelSurfaces, "Pnt/ParallelSurfacesDebug");
            //WavefrontFileGroups.ExportBySurfaces(parallelSurfaces, $"Wavefront/ParallelSurfacesDebug");
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

        private IWireFrameMesh NormalOverlay(IWireFrameMesh input, double radius)
        {
            var output = WireFrameMesh.Create();

            foreach (var positionNormal in input.Positions.SelectMany(p => p.PositionNormals))
            {
                output.AddTriangle(positionNormal.Position, Vector3D.Zero, positionNormal.Position + 0.5 * radius * positionNormal.Normal.Direction, Vector3D.Zero, positionNormal.Position + radius * positionNormal.Normal.Direction, Vector3D.Zero);
            }

            return output;
        }

        private void SurfaceExplore()
        {
            //var grid = PntFile.Import(WireFrameMesh.Create, "Pnt/ParallelSurfaces-186");
            var grid = PntFile.Import(WireFrameMesh.Create, "Pnt/SphereDifference8-62");
            grid.RemoveShortSegments(3e-4);
            grid.RemoveCollinearEdgePoints();
            grid.RemoveCoplanarSurfacePoints();
            //var surfaces = GroupingCollection.ExtractSurfaces(grid.Triangles);

            var parallelSurfaces = grid.ParallelSurfaces(-0.01000);
            //WavefrontFileGroups.ExportBySurfaces(parallelSurfaces, "Wavefront/SurfaceExplorePre-trim");


            parallelSurfaces.Trim();
            //parallelSurfaces.ShowOpenEdges();

            //grid.RemoveOpenFaces();
            //var extra = grid.Triangles.SingleOrDefault(t => t.Id == 5);
            //if (extra is not null)
            //{
            //    grid.RemoveTriangle(extra);
            //}

            parallelSurfaces.ShowVitals();
            parallelSurfaces.ShowOpenEdges();
            //Console.WriteLine($"Surfaces {string.Join(",", surfaces.Select(s => s.Triangles.Count()))}");
            //WavefrontFileGroups.ExportBySurfaces(parallelSurfaces, "Wavefront/SurfaceExplore");
            WavefrontFile.Export(parallelSurfaces, "Wavefront/SurfaceExplore");

            {
                var test = WireFrameMesh.Create();
                var segment = new LineSegment3D(new Point3D(0.49999999999999994, 0.1527586315275828, 0.6303201003419308), new Point3D(0.49999999999999994, 0.11764240251735608, 0.6180217443923588));
                test.AddTriangle(segment.Start, segment.Center, segment.End);
                WavefrontFile.Export(test,"Wavefront/SurfaceExploreXtestLine");
            }

            //foreach(var triangle in grid.Triangles)
            //{
            //    var test = WireFrameMesh.Create();
            //    test.AddTriangle(triangle.Triangle);
            //    WavefrontFile.Export(test, $"Wavefront/Surface-{triangle.Id}");
            //}

            //var normalGrid = NormalOverlay(grid, 0.001);
            //WavefrontFile.Export(normalGrid, "Wavefront/SurfaceNormals");
        }
    }
}
