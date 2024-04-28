using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using FundamentalMeshes;
using Operations.SetOperators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireFrameModels3._0;

namespace Projects.Projects
{
    public class CrossTest : ProjectBase
    {
        protected override void RunProject()
        {
            var part1 = Cuboid.Create(0.5, 1, 5, 10, 5, 10);
            var part2 = part1.Clone(Transform.Rotation(Vector3D.BasisY, Math.PI / 2));
            part2.Apply(Transform.Translation(new Vector3D(-1.5, 0.1, 2.5)));

            //var part3 = part1.Clone(Transform.Rotation(Vector3D.BasisY, -Math.PI / 3));
            //part3.Apply(Transform.Translation(new Vector3D(1.10, -0.1, 2.55)));

            //var part4 = Cuboid.Create(10, 1, 10, 1, 10, 1);
            //part4.Apply(Transform.Translation(new Vector3D(-4.45, 3.55, -2.55)));

            var cross = part1.Union(part2);//.Union(part3);
            //cross = cross.Difference(part4);
            //cross.AddGrid(part3);
            //var sum = WireFrameMesh.CreateMesh();
            //sum.AddGrid(part1);
            //sum.AddGrid(part2);

            WavefrontFile.Export(cross, "Wavefront/CrossTest");
        }
    }
}
