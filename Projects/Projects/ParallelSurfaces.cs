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
            grid.ShowSegmentLengths();
            grid.ShowVitals();

            //var parallelSurfaces = grid.ParallelSurfaces(-0.01790);
            //var parallelSurfaces = grid.ParallelSurfaces(-0.001750);
            //var parallelSurfaces = grid.ParallelSurfaces(-0.001900);//check indeterminancies  search 'Indeterminant point' RESOLVED
            //var parallelSurfaces = grid.ParallelSurfaces(-0.01000);//extra surface RESOLVED
            //var parallelSurfaces = grid.ParallelSurfaces(-0.01700);//filling errors on point removal
            //var parallelSurfaces = grid.ParallelSurfaces(-0.02200);//multiple surfaces start showing RESOLVED
            var displacement = -0.0230;
            //var parallelSurfaces = grid.ParallelSurfaces(-0.001005);
            var parallelSurfaces = grid.ParallelSurfaces(displacement);

            parallelSurfaces.ShowVitals();
            //parallelSurfaces.ShowOpenEdges();
            WavefrontFile.Export(parallelSurfaces, $"Wavefront/ParallelSurfaces{displacement}");

            //var test = WireFrameMesh.Create();
            //{
            //    var segment = new LineSegment3D(new Point3D(0.5331362315381254, 0.8471644540143806, 0.4999999991064687), new Point3D(0.5347974559256385, 0.8422291341886806, 0.49999999909095205));
            //    test.AddTriangle(new Triangle3D(segment.Start, segment.Center, segment.End));
            //}
            //{
            //    var segment = new LineSegment3D(new Point3D(0.5347974559256385, 0.8422291341886806, 0.49999999909095205), new Point3D(0.5348667626638983, 0.8420232312374452, 0.49999999910628645));
            //    test.AddTriangle(new Triangle3D(segment.Start, segment.Center, segment.End));
            //}
            //{
            //    var segment = new LineSegment3D(new Point3D(0.5348667626638983, 0.8420232312374452, 0.49999999910628645), new Point3D(0.5348667626638983, 0.8420232312374452, 0.49999999910628645));
            //    test.AddTriangle(new Triangle3D(segment.Start, segment.Center, segment.End));
            //}
            //{
            //    var segment = new LineSegment3D(new Point3D(0.5348667626638983, 0.8420232312374452, 0.49999999910628645), new Point3D(0.5347974559256385, 0.8422291341886806, 0.49999999909095205));
            //    test.AddTriangle(new Triangle3D(segment.Start, segment.Center, segment.End));
            //}
            //{
            //    var segment = new LineSegment3D(new Point3D(0.5347974559256385, 0.8422291341886806, 0.49999999909095205), new Point3D(0.5303035828870624, 0.841192936479687, 0.5045115007596117));
            //    test.AddTriangle(new Triangle3D(segment.Start, segment.Center, segment.End));
            //}
            //{
            //    var segment = new LineSegment3D(new Point3D(0.5303035828870624, 0.841192936479687, 0.5045115007596117), new Point3D(0.5331362315381254, 0.8471644540143806, 0.4999999991064687));
            //    test.AddTriangle(new Triangle3D(segment.Start, segment.Center, segment.End));
            //}

            //WavefrontFile.Export(test, $"Wavefront/ErrorFill");

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
            grid.RemoveShortSegments(1e-4);
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
