using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using FundamentalMeshes;
using Operations.SetOperators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireFrameModels3._0;

namespace Projects.Projects
{
    public class FlushCubeTest : ProjectBase
    {
        protected override void RunProject()
        {
            var cube1 = Cuboid.Create(1, 1, 1, 1, 1, 1);
            var cube2 = Cuboid.Create(1, 1, 1, 1, 1, 1);
            cube2.Apply(Transform.Translation(new Vector3D(0.00, 0.27, 0.32)));
            //var sum = WireFrameMesh.CreateMesh();
            //sum.AddGrid(cube1);
            //sum.AddGrid(cube2);
            var sum = cube1.Difference(cube2);

            WavefrontFile.Export(sum, "Wavefront/FlushCubeTest");
        }
    }
}
