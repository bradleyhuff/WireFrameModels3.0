using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using FundamentalMeshes;
using Operations.Basics;
using Operations.Groupings.Basics;
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

            var import = PntFile.Import(WireFrameMesh.Create, "Pnt/SphereDifference8 64");
            var clusters = GroupingCollection.ExtractClusters(import.Triangles).ToArray();
            //var output = clusters[180].Create();
            //var output = clusters[137].Create();
            var output = clusters[68].Create();


            //WavefrontFile.Export(output, "Wavefront/Output");
            //var output = WireFrameMesh.Create();
            //output.AddGrid(part1);
            //output.AddGrid(part2);

            double offset = -0.0150;
            var facePlates = output.BuildFacePlates(offset).ToArray();
            WavefrontFile.Export(facePlates, "Wavefront/FacePlates");

            foreach (var facePlate in facePlates.Take(1))
            {
                output = output.Difference(facePlate);
            }
            //while (output.Triangles.Any(t => t.OpenEdges.Any())) { output.RemoveAllTriangles(output.Triangles.Where(t => t.OpenEdges.Any())); }
            //output.RemoveShortSegments(1e-4);
            //output.RemoveCollinearEdgePoints();
            //output.RemoveCoplanarSurfacePoints();

            output.ShowVitals();
            WavefrontFile.Export(output, "Wavefront/Output");

            //var union = Sets.Union(facePlates);
            //union.BaseStrip();
            //union.ShowVitals();

            //WavefrontFile.Export(output, "Wavefront/UnionCheck");
            //WavefrontFile.Export(facePlates, "Wavefront/FacePlates");
            //WavefrontFile.Export(union, "Wavefront/Union");
            //WavefrontFile.Export(NormalOverlay(union, 0.05), "Wavefront/UnionNormals");
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
