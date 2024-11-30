using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using FundamentalMeshes;
using Operations.Basics;
using Operations.ParallelSurfaces;
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
            var part1 = Cuboid.Create(1, 2, 1, 2, 1, 2);
            var part2 = Cuboid.Create(1, 2, 1, 2, 1, 2);
            part2.Apply(Transform.Translation(new Vector3D(0.4, 0.4, 0)));

            var output = part1;//.Difference(part2);
            //var output = WireFrameMesh.Create();
            //output.AddGrid(part1);
            //output.AddGrid(part2);
            var facePlates = output.BuildFacePlates(-0.1000).ToArray();
            //facePlates[0].Apply(Transform.Translation(new Vector3D(-4e-8, -5e-8, -6e-8)));
            //facePlates[1].Apply(Transform.Translation(new Vector3D(-1e-8, -2e-8, -3e-8)));
            //facePlates[2].Apply(Transform.Translation(new Vector3D(1e-8, 2e-8, 3e-8)));
            //facePlates[3].Apply(Transform.Translation(new Vector3D(4e-8, 5e-8, 6e-8)));
            //facePlates[4].Apply(Transform.Translation(new Vector3D(7e-8, 8e-8, 9e-8)));
            //facePlates[5].Apply(Transform.Translation(new Vector3D(10e-8, 11e-8, 12e-8)));
            var union = Sets.Union(facePlates);
            union.ShowVitals();

            WavefrontFile.Export(output, "Wavefront/UnionCheck");
            WavefrontFile.Export(facePlates, "Wavefront/FacePlates");
            WavefrontFile.Export(union, "Wavefront/Union");
            WavefrontFile.Export(NormalOverlay(union, 0.05), "Wavefront/UnionNormals");
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
