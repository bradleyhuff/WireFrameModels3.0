using BaseObjects.Transformations;
using BasicObjects;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using FundamentalMeshes;
using Operations.Groupings.Basics;
using Operations.Groupings.FileExportImport;
using Operations.Groupings.Types;
using Operations.SetOperators;
using WireFrameModels3._0;

namespace Projects.Projects
{
    public class IntermeshDevelopment : ProjectBase
    {
        protected override void RunProject()
        {
            CubeSphereTestOne(128);
            //CubeSphereTestTwo(64);
        }

        private void CubeSphereTestOne(int resolution)
        {
            var cube = Cuboid.Create(1, 2, 1, 2, 1, 2);
            var spheres = CreateTestSpheres(resolution);
            var output = cube.Difference(spheres);
            var spheres2 = spheres.Clone();

            //spheres2.Apply(Transform.Translation(new Point3D(1.0001, 0.0001, 0.0001)));
            //spheres2.Apply(Transform.Translation(new Point3D(1.000001, 0.000001, 0.000001)));
            //spheres2.Apply(Transform.Translation(new Point3D(1.00000001, 0.00000001, 0.00000001)));
            spheres2.Apply(Transform.Translation(new Vector3D(1, 0, 0)));
            //spheres2.Apply(Transform.Translation(new Point3D(1.05, 0.04, 0.06)));
            output = output.Difference(spheres2);

            var spheres3 = spheres.Clone();
            spheres3.Apply(Transform.Translation(new Vector3D(0e-10, 1 + 0e-10, 0e-10)));
            output = output.Difference(spheres3);
            //var spheres3A = CreateTestSpheres2(resolution);
            //spheres3A.Apply(Transform.Translation(new Vector3D(0e-10, 1 + 0e-10, 0e-10)));

            var spheres4 = spheres.Clone();
            spheres4.Apply(Transform.Translation(new Vector3D(0, 0, 1)));
            output = output.Difference(spheres4);

            var spheres5 = spheres.Clone();
            spheres5.Apply(Transform.Translation(new Vector3D(0, 1, 1)));
            output = output.Difference(spheres5);

            var spheres6 = spheres.Clone();
            spheres6.Apply(Transform.Translation(new Vector3D(1, 0, 1)));
            output = output.Difference(spheres6);

            var spheres7 = spheres.Clone();
            spheres7.Apply(Transform.Translation(new Vector3D(1, 1, 0)));
            output = output.Difference(spheres7);

            var spheres8 = spheres.Clone();
            spheres8.Apply(Transform.Translation(new Vector3D(1, 1, 1)));
            output = output.Difference(spheres8);

            //TableDisplays.ShowCountSpread("Position normal triangle counts", output.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            //TableDisplays.ShowCountSpread("Position normal counts", output.Positions, p => p.PositionNormals.Count);

            Console.WriteLine($"Clusters {GroupingCollection.ExtractClusters(output.Triangles).Count()}");
            Console.WriteLine();

            PntFile.Export(output, $"Pnt/SphereDifference8 {resolution}");
            WavefrontFile.Export(output, $"Wavefront/SphereDifference8 {resolution}");
            //WavefrontFile.Export(spheres3A, $"Wavefront/Sphere3 {resolution}");
            //WavefrontFileGroups.ExportByClusters(output, "Wavefront/Clusters");
            //WavefrontFileGroups.ExportByClusters(output, o => NormalOverlay(o, 0.003), "Wavefront/Normals");
        }

        private void CubeSphereTestTwo(int resolution)
        {

            var output = PntFile.Import(() => WireFrameMesh.CreateMesh(), $"Pnt/SphereDifference4 {resolution}");
            var spheres = CreateTestSpheres(resolution);

            var spheres5 = spheres.Clone();
            spheres5.Apply(Transform.Translation(new Vector3D(0, 1, 1)));
            output = output.Difference(spheres5);

            //var spheres6 = spheres.Clone();
            //spheres6.Apply(Transform.Translation(new Vector3D(1, 0, 1)));
            //output = output.Difference(spheres6);

            //var spheres7 = spheres.Clone();
            //spheres7.Apply(Transform.Translation(new Vector3D(1, 1, 0)));
            //output = output.Difference(spheres7);

            //var spheres8 = spheres.Clone();
            //spheres8.Apply(Transform.Translation(new Vector3D(1, 1, 1)));
            //output = output.Difference(spheres8);


            //TableDisplays.ShowCountSpread("Position normal triangle counts", output.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            //TableDisplays.ShowCountSpread("Position normal counts", output.Positions, p => p.PositionNormals.Count);

            Console.WriteLine($"Clusters {GroupingCollection.ExtractClusters(output.Triangles).Count()}");
            Console.WriteLine();

            PntFile.Export(output, $"Pnt/SphereDifference5 {resolution}");
            WavefrontFile.Export(output, $"Wavefront/SphereDifference5 {resolution}");
            //WavefrontFileGroups.ExportByClusters(output, "Wavefront/SphereDifference5");
            //WavefrontFileGroups.ExportByClusters(output, o => NormalOverlay(o, 0.003), "Wavefront/Normals");
        }

        private IWireFrameMesh CreateTestSpheres(int resolution)
        {
            var sphere = Sphere.Create(1, resolution);
            var sphere2 = sphere.Clone();
            var sphere3 = sphere.Clone();
            var sphere4 = sphere.Clone();
            var sphere5 = sphere.Clone();
            var sphere6 = sphere.Clone();

            sphere.Apply(Transform.Scale(0.70));
            sphere2.Apply(Transform.Scale(0.65));
            sphere3.Apply(Transform.Scale(0.60));
            sphere4.Apply(Transform.Scale(0.55));
            sphere5.Apply(Transform.Scale(0.75));
            sphere6.Apply(Transform.Scale(0.80));

            var spheres = sphere;
            spheres.AddGrid(sphere2);
            spheres.AddGrid(sphere3);
            spheres.AddGrid(sphere4);
            spheres.AddGrid(sphere5);
            spheres.AddGrid(sphere6);

            return spheres;
        }

        private IWireFrameMesh CreateTestSpheres2(int resolution)
        {
            var sphere = Sphere.Create(1, resolution);
            var sphere2 = sphere.Clone();
            var sphere3 = sphere.Clone();
            var sphere4 = sphere.Clone();
            var sphere5 = sphere.Clone();

            sphere.Apply(Transform.Scale(0.70));
            sphere2.Apply(Transform.Scale(0.65));
            sphere3.Apply(Transform.Scale(0.60));
            sphere4.Apply(Transform.Scale(0.55));
            sphere5.Apply(Transform.Scale(0.75));

            var spheres = sphere;
            spheres.AddGrid(sphere2);
            spheres.AddGrid(sphere3);
            spheres.AddGrid(sphere4);
            spheres.AddGrid(sphere5);

            return spheres;
        }

        private IWireFrameMesh NormalOverlay(IWireFrameMesh input, double radius)
        {
            var output = WireFrameMesh.CreateMesh();

            foreach (var positionNormal in input.Positions.SelectMany(p => p.PositionNormals))
            {
                output.AddTriangle(positionNormal.Position, Vector3D.Zero, positionNormal.Position + 0.5 * radius * positionNormal.Normal.Direction, Vector3D.Zero, positionNormal.Position + radius * positionNormal.Normal.Direction, Vector3D.Zero);
            }

            return output;
        }
    }
}
