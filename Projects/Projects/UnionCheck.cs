using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using FundamentalMeshes;
using Operations.Basics;
using Operations.Groupings.Basics;
using Operations.Groupings.FileExportImport;
using Operations.ParallelSurfaces;
using Operations.PositionRemovals;
using Operations.SetOperators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireFrameModels3._0;

namespace Projects.Projects
{
    internal class UnionCheck : ProjectBase
    {
        protected override void RunProject()
        {
            //var part1 = Cuboid.Create(1, 2, 1, 2, 1, 2);
            //var part2 = Cuboid.Create(1, 2, 1, 2, 1, 2);
            //part2.Apply(Transform.Translation(new Vector3D(0.4, 0.4, 0)));

            //var output = part1.Difference(part2);

            var import = PntFile.Import(WireFrameMesh.Create, "Pnt/SphereDifference8B 64");
            var clusters = GroupingCollection.ExtractClusters(import.Triangles).ToArray();
            //var output = clusters[180].Create();
            //var output = clusters[137].Create();
            //var output = clusters[51].Create();//chaining error
            var output = clusters[7].Create();//chaining error
            //var output = clusters[38].Create();//banana with error
            //var output = clusters[99].Create();

            //output.Apply(Transform.Rotation(Vector3D.BasisX, 1e-1));

            WavefrontFile.Export(output, "Wavefront/Input");
            //var output = WireFrameMesh.Create();
            //output.AddGrid(part1);
            //output.AddGrid(part2);

            double offset = -0.0025;// 7
            //double offset = -0.0750;
            var facePlates = output.BuildFacePlates(offset).ToArray();
            WavefrontFile.Export(facePlates, "Wavefront/FacePlates");

            //WavefrontFile.Export(facePlates.Select(f => { var g = WireFrameMesh.Create(); g.AddRangeTriangles(f.Triangles.Where(t => t.Trace[0] == 'S'), "", 0); return g; }), "Wavefront/FacePlates-Surface");
            //WavefrontFile.Export(facePlates.Select(f => { var g = WireFrameMesh.Create(); g.AddRangeTriangles(f.Triangles.Where(t => t.Trace[0] == 'B'), "", 0); return g; }), "Wavefront/FacePlates-Base");
            //WavefrontFile.Export(facePlates.Select(f => { var g = WireFrameMesh.Create(); g.AddRangeTriangles(f.Triangles.Where(t => t.Trace[0] == 'F'), "", 0); return g; }), "Wavefront/FacePlates-Side");
            //facePlates = facePlates.Reverse().ToArray();
            foreach (var facePlate in facePlates.Select((f, i) => new { Grid = f, Index = i }))
            {
                //if (facePlate.Index > 4) { break; }
                Console.WriteLine("Face plate");
                //RemoveTags(facePlate);
                facePlate.Grid.ShowVitals(facePlate.Index);
                output = output.Difference(facePlate.Grid);
                //RemoveTags(output);
            }

            //output = output.Difference(facePlates[6]);


            //output.Test(facePlates[5]);

            //output = output.Difference(facePlates[5]);


            //while (output.Triangles.Any(t => t.OpenEdges.Any())) { output.RemoveAllTriangles(output.Triangles.Where(t => t.OpenEdges.Any())); }
            //output.RemoveShortSegments(1e-4);
            //output.RemoveCollinearEdgePoints();
            //output.RemoveCoplanarSurfacePoints();

            Console.WriteLine("Output");
            output.ShowVitals(99);
            WavefrontFile.Export(output, "Wavefront/Output");
            WavefrontFileGroups.ExportBySurfaces(output, "Wavefront/Output");

            //var union = Sets.Union(facePlates);
            //union.BaseStrip();
            //union.ShowVitals();

            //WavefrontFile.Export(output, "Wavefront/UnionCheck");
            //WavefrontFile.Export(facePlates, "Wavefront/FacePlates");
            //WavefrontFile.Export(union, "Wavefront/Union");
            WavefrontFile.Export(NormalOverlay(output, 0.05), "Wavefront/UnionNormals");
        }

        private static void RemoveTags(IWireFrameMesh output)
        {
            var tags = output.Triangles.Where(t => t.AdjacentAnyCount < 3);
            while (tags.Any())
            {
                output.RemoveAllTriangles(tags);
                tags = output.Triangles.Where(t => t.AdjacentAnyCount < 3);
            }
        }

        private IWireFrameMesh NormalOverlay(IWireFrameMesh input, double radius)
        {
            var output = WireFrameMesh.Create();

            foreach (var positionNormal in input.Positions.SelectMany(p => p.PositionNormals))
            {
                output.AddTriangle(positionNormal.Position, Vector3D.Zero, positionNormal.Position + 0.5 * radius * positionNormal.Normal.Direction, Vector3D.Zero, positionNormal.Position + radius * positionNormal.Normal.Direction, Vector3D.Zero, "", 0);
            }

            return output;
        }
    }
}
