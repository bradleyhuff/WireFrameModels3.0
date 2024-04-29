using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using FundamentalMeshes;
using Operations;
using Operations.SetOperators;
using WireFrameModels3._0;

namespace Projects.Projects
{
    public class CrossTest : ProjectBase
    {
        protected override void RunProject()
        {
            var part1 = Cuboid.Create(0.5, 1, 5, 10, 5, 10);
            //var part2 = part1.Clone(Transform.Rotation(Vector3D.BasisY, Math.PI / 2));
            var part2 = Cuboid.Create(0.5, 1, 4, 8, 4, 8);
            part2.Apply(Transform.Rotation(Vector3D.BasisY, Math.PI / 2));
            part2.Apply(Transform.Translation(new Vector3D(-1.50000, 0.6, 2.5)));
            //part2.Apply(Transform.Translation(new Vector3D(-1.50000, 0.1, 2.5)));

            //var part3 = part1.Clone(Transform.Rotation(Vector3D.BasisY, -Math.PI / 3));
            //part3.Apply(Transform.Translation(new Vector3D(1.10, -0.1, 2.55)));

            var part4 = Cuboid.Create(10, 1, 10, 1, 10, 1);
            part4.Apply(Transform.Translation(new Vector3D(-4.4, 3.51, -2.5)));

            var cross = part1.Union(part2);//.Union(part3);
            //var cross = WireFrameMesh.CreateMesh();
            //cross.AddGrid(part1);
            //cross.AddGrid(part2);
            //IntermeshOperation.Run(cross);
            //int count = cross.RemoveAllTriangles(cross.Triangles.Where(t => t.Id == 1176));
            //Console.WriteLine($"Triangles removed {count}");
            cross = cross.Difference(part4);
            //cross.AddGrid(part3);
            //var sum = WireFrameMesh.CreateMesh();
            //sum.AddGrid(part1);
            //sum.AddGrid(part2);

            WavefrontFile.Export(cross, "Wavefront/CrossTest");
        }
    }
}
