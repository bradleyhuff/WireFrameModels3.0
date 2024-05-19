using BasicObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
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
            //var grid = PntFile.Import(WireFrameMesh.Create, "Pnt/SphereDifference8 32");
            //PntFileGroups.ExportByClusters(grid, "Pnt/Clusters");
            //var grid = PntFile.Import(WireFrameMesh.Create, "Pnt/Clusters-29");
            //TableDisplays.ShowCountSpread("Position cardinalities", grid.Positions, p => p.Cardinality);
            //WavefrontFile.Export(grid, $"Wavefront/Banana-Original");

            var grid = PntFile.Import(WireFrameMesh.Create, "Pnt/ParallelSurfaces-29");
            grid.Trim();
            WavefrontFile.Export(grid, "Wavefront/ParallelSurfaces-29 Trimmed");
            TableDisplays.ShowCountSpread("Position cardinalities", grid.Positions, p => p.Cardinality);

            var banana = PntFile.Import(WireFrameMesh.Create, "Pnt/Banana");
            TableDisplays.ShowCountSpread("Position cardinalities", banana.Positions, p => p.Cardinality);

            //var parallelSurfaces = grid.ParallelSurfaces(-0.0050);
            //parallelSurfaces.RemoveShortSegments(1e-4);
            //parallelSurfaces.Trim();

            //Console.WriteLine($"Clusters {GroupingCollection.ExtractClusters(parallelSurfaces.Triangles).Count()}");
            //PntFileGroups.ExportByClusters(parallelSurfaces, "Pnt/ParallelSurfaces");
            //WavefrontFileGroups.ExportByClusters(parallelSurfaces, "Wavefront/ParallelSurfaces");
            //Console.WriteLine();
            return;
            //var faces = GroupingCollection.ExtractFaces(grid.Triangles).ToArray();
            //var banana = WireFrameMesh.Create();
            //foreach (var triangle in faces[1].Triangles) { banana.AddTriangle(triangle.A.Position, triangle.A.Normal, triangle.B.Position, triangle.B.Normal, triangle.C.Position, triangle.C.Normal); }
            //foreach (var triangle in faces[3].Triangles) { banana.AddTriangle(triangle.A.Position, triangle.A.Normal, triangle.B.Position, triangle.B.Normal, triangle.C.Position, triangle.C.Normal); }
            //foreach (var triangle in faces[6].Triangles) { banana.AddTriangle(triangle.A.Position, triangle.A.Normal, triangle.B.Position, triangle.B.Normal, triangle.C.Position, triangle.C.Normal); }
            //foreach (var triangle in faces[7].Triangles) { banana.AddTriangle(triangle.A.Position, triangle.A.Normal, triangle.B.Position, triangle.B.Normal, triangle.C.Position, triangle.C.Normal); }
            //foreach (var triangle in faces[12].Triangles) { banana.AddTriangle(triangle.A.Position, triangle.A.Normal, triangle.B.Position, triangle.B.Normal, triangle.C.Position, triangle.C.Normal); }
            //foreach (var triangle in faces[14].Triangles) { banana.AddTriangle(triangle.A.Position, triangle.A.Normal, triangle.B.Position, triangle.B.Normal, triangle.C.Position, triangle.C.Normal); }
            //foreach (var triangle in faces[36].Triangles) { banana.AddTriangle(triangle.A.Position, triangle.A.Normal, triangle.B.Position, triangle.B.Normal, triangle.C.Position, triangle.C.Normal); }
            //foreach (var triangle in faces[47].Triangles) { banana.AddTriangle(triangle.A.Position, triangle.A.Normal, triangle.B.Position, triangle.B.Normal, triangle.C.Position, triangle.C.Normal); }
            //foreach (var triangle in faces[50].Triangles) { banana.AddTriangle(triangle.A.Position, triangle.A.Normal, triangle.B.Position, triangle.B.Normal, triangle.C.Position, triangle.C.Normal); }
            //foreach (var triangle in faces[69].Triangles) { banana.AddTriangle(triangle.A.Position, triangle.A.Normal, triangle.B.Position, triangle.B.Normal, triangle.C.Position, triangle.C.Normal); }

            ////WavefrontFile.Export(parallelSurfaces, $"Wavefront/ParallelSurfaces");
            ////WavefrontFileGroups.ExportByFaces(grid, "Wavefront/Banana");
            ////WavefrontFile.Export(grid, $"Wavefront/ParallelSurfaces");
            //PntFile.Export(banana, "Pnt/Banana");
            //WavefrontFile.Export(banana, $"Wavefront/Banana");

            //TableDisplays.ShowCountSpread("Position cardinalities", banana.Positions, p => p.Cardinality);
            //TableDisplays.ShowCountSpread("AB Adjacency counts",banana.Triangles.Select(t => t.ABadjacents), l => l.Count);
            //TableDisplays.ShowCountSpread("BC Adjacency counts", banana.Triangles.Select(t => t.BCadjacents), l => l.Count);
            //TableDisplays.ShowCountSpread("CA Adjacency counts", banana.Triangles.Select(t => t.CAadjacents), l => l.Count);

            //var corners = banana.Positions.Where(p => p.Cardinality == 3).ToArray();

            //banana.ShowSegmentLengths();
        }
    }
}
