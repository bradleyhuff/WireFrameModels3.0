using BaseObjects;
using BaseObjects.Transformations;
using BasicObjects;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using FundamentalMeshes;
using Operations.Basics;
using Operations.Groupings.Basics;
using Console = BaseObjects.Console;
using Operations.SetOperators;
using WireFrameModels3._0;
using Operations.PositionRemovals;

namespace Projects.Projects
{
    public class IntermeshDevelopment : ProjectBase
    {
        protected override void RunProject()
        {
            ConsoleLog.MaximumLevels = 1;
            CubeSphereTestOne(60);
            //CubeSphereTestOne(64);
            //CubeSphereTestTwo(179);
        }

        private void CubeSphereTestOne(int resolution)
        {
            var cube = Cuboid.Create(1, 2, 1, 2, 1, 2);
            var spheres = CreateTestSpheres(resolution);
            var output = cube.Difference(spheres);
            output.ShowVitals();
            var spheres2 = spheres.Clone();

            ////spheres2.Apply(Transform.Translation(new Point3D(1.0001, 0.0001, 0.0001)));
            ////spheres2.Apply(Transform.Translation(new Point3D(1.000001, 0.000001, 0.000001)));
            ////spheres2.Apply(Transform.Translation(new Point3D(1.00000001, 0.00000001, 0.00000001)));
            spheres2.Apply(Transform.Translation(new Vector3D(1, 0, 0)));
            //spheres2.Apply(Transform.Translation(new Point3D(1.05, 0.04, 0.06)));
            //Grid.RemovalFilter = true;
            output = output.Difference(spheres2);
            output.ShowVitals();
            //PntFile.Export(output, $"Pnt/SphereDifference2 {resolution}");
            //WavefrontFile.Export(output, $"Wavefront/SphereDifference2 {resolution}");
            //var removalTriangle = output.Triangles.First();
            //var isRemoved = output.RemoveTriangle(output.Triangles.First());
            //Console.WriteLine($"Triangle removed {isRemoved}");
            //Console.WriteLine($"Output triangles {output.Triangles.Count()}");
            //Console.WriteLine($"Triangles removed {output.RemoveAllTriangles(output.Triangles.Take(16384))}");

            var spheres3 = spheres.Clone();
            spheres3.Apply(Transform.Translation(new Vector3D(0, 1, 0)));
            output = output.Difference(spheres3);
            output.ShowVitals();

            var spheres4 = spheres.Clone();
            spheres4.Apply(Transform.Translation(new Vector3D(0, 0, 1)));
            output = output.Difference(spheres4);
            output.ShowVitals();

            var spheres5 = spheres.Clone();
            spheres5.Apply(Transform.Translation(new Vector3D(0, 1, 1)));
            output = output.Difference(spheres5);
            output.ShowVitals();

            var spheres6 = spheres.Clone();
            spheres6.Apply(Transform.Translation(new Vector3D(1, 0, 1)));
            output = output.Difference(spheres6);
            output.ShowVitals();

            var spheres7 = spheres.Clone();
            spheres7.Apply(Transform.Translation(new Vector3D(1, 1, 0)));
            output = output.Difference(spheres7);
            output.ShowVitals();

            var spheres8 = spheres.Clone();
            spheres8.Apply(Transform.Translation(new Vector3D(1, 1, 1)));
            output = output.Difference(spheres8);

            output.ShowSegmentLengths();
            output.ShowVitals();

            PntFile.Export(output, $"Pnt/SphereDifference8 {resolution}");
            WavefrontFile.Export(output, $"Wavefront/SphereDifference8 {resolution}");
            //WavefrontFile.Export(spheres3A, $"Wavefront/Sphere3 {resolution}");
            //WavefrontFileGroups.ExportByClusters(output, "Wavefront/Clusters");
            {
                var test = WireFrameMesh.Create();
                var openEdges = output.Triangles.SelectMany(t => t.OpenEdges);
                //Console.WriteLine($"Triangles {string.Join(",", output.Triangles.Select(t => t.Key))}");
                Console.WriteLine($"Open edges {string.Join(",", openEdges.Select(o => o.Segment))}");
                Console.WriteLine($"Open edges {string.Join(",", openEdges.Select(o => $"[{o.A.PositionObject.Point}<{o.A.PositionObject.Id}>, {o.B.PositionObject.Point}<{o.B.PositionObject.Id}>]"))}");
                Console.WriteLine($"Open edges {string.Join(",", openEdges.Select(o => $"[{o.A.Normal}<{o.A.PositionObject.Id}>, {o.B.Normal}<{o.B.PositionObject.Id}>]"))}");
                test.AddRangeTriangles(openEdges.Select(e => e.Plot), "", 0);
                WavefrontFile.Export(test, $"Wavefront/TagOpenEdges");
            }
        }

        private void CubeSphereTestTwo(int resolution)
        {

            var output = PntFile.Import(WireFrameMesh.Create, $"Pnt/SphereDifference7 {resolution}");
            output.ShowVitals();
            var spheres = CreateTestSpheres(resolution);

            //var spheres5 = spheres.Clone();
            //spheres5.Apply(Transform.Translation(new Vector3D(0, 1, 1)));
            //output = output.Difference(spheres5);
            //output.ShowVitals();
            //WavefrontFile.Export(spheres5, "Wavefront/Spheres5");

            //var spheres6 = spheres.Clone();
            //spheres6.Apply(Transform.Translation(new Vector3D(1, 0, 1)));
            //output = output.Difference(spheres6);
            //output.ShowVitals();

            ////WavefrontFile.Export(spheres6, "Wavefront/Spheres6");

            //var spheres7 = spheres.Clone();
            //spheres7.Apply(Transform.Translation(new Vector3D(1, 1, 0)));
            //output = output.Difference(spheres7);

            var spheres8 = spheres.Clone();
            spheres8.Apply(Transform.Translation(new Vector3D(1, 1, 1)));
            output = output.Difference(spheres8);


            //TableDisplays.ShowCountSpread("Position normal triangle counts", output.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            //TableDisplays.ShowCountSpread("Position normal counts", output.Positions, p => p.PositionNormals.Count);

            //Console.WriteLine($"Clusters {GroupingCollection.ExtractClusters(output.Triangles).Count()}");
            //var tagTriangles = output.Triangles.Where(t => t.AdjacentAnyCount <= 2).ToArray();
            //{
            //    var test = WireFrameMesh.Create();
            //    test.AddRangeTriangles(tagTriangles.Select(PositionTriangle.GetSurfaceTriangle));
            //    WavefrontFile.Export(test, $"Wavefront/TagTriangles");
            //    var normalGrid = NormalOverlay(test, 0.001);
            //    WavefrontFile.Export(normalGrid, $"Wavefront/TagNormals");
            //}
            //{
            //    var test = WireFrameMesh.Create();
            //    var openEdges = tagTriangles.SelectMany(t => t.OpenEdges);
            //    Console.WriteLine($"Triangles {string.Join(",",tagTriangles.Select(t => t.Key))}");
            //    Console.WriteLine($"Open edges {string.Join(",", openEdges.Select(o => o.Segment))}");
            //    Console.WriteLine($"Open edges {string.Join(",", openEdges.Select(o => $"[{o.A.PositionObject.Point}<{o.A.PositionObject.Id}>, {o.B.PositionObject.Point}<{o.B.PositionObject.Id}>]"))}");
            //    Console.WriteLine($"Open edges {string.Join(",", openEdges.Select(o => $"[{o.A.Normal}<{o.A.PositionObject.Id}>, {o.B.Normal}<{o.B.PositionObject.Id}>]"))}");
            //    test.AddRangeTriangles(openEdges.Select(e => e.Plot));
            //    WavefrontFile.Export(test, $"Wavefront/TagOpenEdges");
            //}

            //Console.WriteLine();
            //output.RemoveTagTriangles();
            //output.RemoveTagTriangles();
            //output.RemoveTagTriangles();
            //output.RemoveTagTriangles();
            //output.RemoveTagTriangles();
            //output.RemoveTagTriangles();

            //output.RemoveTagTriangles();
            //output.RemoveTagTriangles();
            //output.RemoveTagTriangles();
            //output.RemoveTagTriangles();
            //output.RemoveTagTriangles();
            //output.RemoveTagTriangles();

            output.ShowSegmentLengths();
            output.ShowVitals();

            //var preFragments = output.Triangles.Where(t => t.Id == 2728611 || t.Id == 2601493);
            //Console.WriteLine($"Prefragments {preFragments.Count()}");
            //{
            //    var test = WireFrameMesh.Create();
            //    test.AddRangeTriangles(preFragments.Select(f => f.Triangle));
            //    WavefrontFile.Export(test, "Wavefront/Prefragments");
            //}

            PntFile.Export(output, $"Pnt/SphereDifference8 {resolution}");
            WavefrontFile.Export(output, $"Wavefront/SphereDifference8 {resolution}");
            {
                var test = WireFrameMesh.Create();
                var openEdges = output.Triangles.SelectMany(t => t.OpenEdges);
                //Console.WriteLine($"Triangles {string.Join(",", output.Triangles.Select(t => t.Key))}");
                Console.WriteLine($"Open edges {string.Join(",", openEdges.Select(o => o.Segment))}");
                Console.WriteLine($"Open edges {string.Join(",", openEdges.Select(o => $"[{o.A.PositionObject.Point}<{o.A.PositionObject.Id}>, {o.B.PositionObject.Point}<{o.B.PositionObject.Id}>]"))}");
                Console.WriteLine($"Open edges {string.Join(",", openEdges.Select(o => $"[{o.A.Normal}<{o.A.PositionObject.Id}>, {o.B.Normal}<{o.B.PositionObject.Id}>]"))}");
                test.AddRangeTriangles(openEdges.Select(e => e.Plot), "", 0);
                WavefrontFile.Export(test, $"Wavefront/TagOpenEdges");
            }

            //PntFile.Export(output, $"Pnt/SphereDifference8 {resolution}");
            //WavefrontFile.Export(output, $"Wavefront/SphereDifference8 {resolution}");
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
            var output = WireFrameMesh.Create();

            foreach (var positionNormal in input.Positions.SelectMany(p => p.PositionNormals))
            {
                output.AddTriangle(positionNormal.Position, Vector3D.Zero, positionNormal.Position + 0.5 * radius * positionNormal.Normal.Direction, Vector3D.Zero, positionNormal.Position + radius * positionNormal.Normal.Direction, Vector3D.Zero, "", 0);
            }

            return output;
        }
    }
}
