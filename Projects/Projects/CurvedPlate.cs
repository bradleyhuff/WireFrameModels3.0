using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using FundamentalMeshes;
using Operations.Basics;
using Operations.Groupings.Basics;
using Operations.Groupings.FileExportImport;
using Operations.ParallelSurfaces;
using Operations.SetOperators;
using WireFrameModels3._0;

namespace Projects.Projects
{
    internal class CurvedPlate : ProjectBase
    {
        protected override void RunProject()
        {
            Part1();
            //Part2();
        }

        private void Part1() { 
            var curvedFace = WireFrameMesh.Create();
            //curvedFace.AddGrid(Cylinder.Create(0.1, 1, 40));
            double displacement = 0.15;

            //AddPath(curvedFace, 0);
            //AddPath(curvedFace, 1);

            var cube = Cuboid.Create(1, 2, 1, 2, 1, 2);
            cube.Apply(Transform.Translation(new Vector3D(-0.6, 0.060, -0.6)));

            curvedFace.AddGrid(cube);
            //curvedFace.AddGrid(PntFile.Import(WireFrameMesh.Create, "Pnt/RoundedCube"));
            curvedFace = curvedFace.Difference(PntFile.Import(WireFrameMesh.Create, "Pnt/RoundedCube"));
            WavefrontFile.Export(curvedFace, "Wavefront/CurvedFace");
            //WavefrontFile.Export(NormalOverlay(curvedFace, 0.05), "Wavefront/CurvedFaceNormals");
            //WavefrontFileGroups.ExportByFaceFolds(curvedFace, "Wavefront/FaceFolds");
            //WavefrontFileGroups.ExportByFaces(curvedFace, "Wavefront/Faces");
            //curvedFace = curvedFace.Difference(Cylinder.Create(0.1, 1, 40));

            var facePlates = curvedFace.SetFacePlates(0.11);

            //var cube2 = Cuboid.Create(1, 1, 1, 1, 1, 1);
            //cube2.Apply(Transform.Translation(new Vector3D(-0.900001, 0, -1)));
            ////curvedFace.AddGrid(cube2);
            //curvedFace = curvedFace.Difference(cube2);
            ////curvedFace = curvedFace.Difference(cube2);

            //var cube3 = Cuboid.Create(1, 1, 1, 1, 1, 1);
            //cube3.Apply(Transform.Translation(new Vector3D(-1, 0, -0.900001)));
            //curvedFace = curvedFace.Difference(cube3);
            ////curvedFace.AddGrid(cube3);

            //var facePlate = curvedFace.ParallelFacePlate(displacement);
            //curvedFace.RemoveAllTriangles(curvedFace.Triangles.Where(t => t.Id == 1984 || t.Id == 1985));
            //curvedFace.AddTriangle(
            //    new Point3D(0.015643446504023068, 1, 0.09876883405951378), new Vector3D(-0.156434, -0.000000, -0.987688), 
            //    new Point3D(-1.270620473691705E-05, 1, 0.09999899999999995), new Vector3D(0.000000, 0.000000, -1.000000), 
            //    new Point3D(-1.270620473691705E-05, 0, 0.09999899999999995), new Vector3D(0.000000, -0.000000, -1.000000));
            //curvedFace.AddTriangle(
            //    new Point3D(0.015643446504023068, 1, 0.09876883405951378), new Vector3D(-0.156434, -0.000000, -0.987688), 
            //    new Point3D(-1.270620473691705E-05, 0, 0.09999899999999995), new Vector3D(0.000000, -0.000000, -1.000000), 
            //    new Point3D(0.015643446504023068, 0, 0.09876883405951378), new Vector3D(-0.156434, -0.000000, -0.987688));
            //A: [ X: 0.015643446504023068 Y: 1 Z: 0.09876883405951378 ][ X: -0.156434 Y: -0.000000 Z: -0.987688 ] B: [ X: -1.270620473691705E-05 Y: 1 Z: 0.09999899999999995 ][ X: 0.000127 Y: 0.000000 Z: -1.000000 ] C: [ X: -1.270620473691705E-05 Y: 0 Z: 0.09999899999999995 ][ X: 0.000127 Y: -0.000000 Z: -1.000000 ]
            //A: [ X: 0.015643446504023068 Y: 1 Z: 0.09876883405951378 ][ X: -0.156434 Y: -0.000000 Z: -0.987688 ] B: [ X: -1.270620473691705E-05 Y: 0 Z: 0.09999899999999995 ][ X: 0.000127 Y: -0.000000 Z: -1.000000 ] C: [ X: 0.015643446504023068 Y: 0 Z: 0.09876883405951378 ][ X: -0.156434 Y: -0.000000 Z: -0.987688 ]

            //curvedFace.ShowVitals();
            facePlates.ShowVitals();

            //Console.WriteLine($"{string.Join("\n", curvedFace.Positions.Where(p => p.Cardinality == 2).Select(q => $" {q.Id} [{string.Join(",", q.PositionNormals.Select(n => $"{n.Id}: {n.Normal}"))}]"))}");

            //Console.WriteLine($"{string.Join("\n", curvedFace.Triangles.Where(t => t.A.Id == 1219 || t.A.Id == 1220 || t.B.Id == 1219 || t.B.Id == 1220 || t.C.Id == 1219 || t.C.Id == 1220).Select(t => $"{t.Id} {PositionTriangle.GetSurfaceTriangle(t)}"))}");
            //curvedFace.RemoveAllTriangles(curvedFace.Triangles.Where(t => t.Id == 1984 || t.Id == 1985));

            //WavefrontFile.Export(facePlate, "Wavefront/curvedPlate");
            //WavefrontFile.Export(curvedFace, "Wavefront/CurvedPlate");
            //PntFile.Export(curvedFace, "Pnt/CurvedPlate");
            //WavefrontFileGroups.ExportByTraces(facePlates, "Wavefront/FacePlates");
            //PntFileGroups.ExportByFaces(facePlates, "Pnt/FacePlates");
            //WavefrontFileGroups.ExportByFolds(facePlates, "Wavefront/Folds");
            WavefrontFile.Export(facePlates, "Wavefront/FacePlates");
            //WavefrontFile.Export(parallelSurface, "Wavefront/ParallelSurface");
            WavefrontFile.Export(NormalOverlay(facePlates, 0.05), "Wavefront/FacePlatesNormals");

            //var faceTriangles = facePlates.Triangles.Where(t => t.Trace == "F0");
            //var faceTriangles = WireFrameMesh.Create();
            //faceTriangles.AddRangeTriangles(facePlates.Triangles.Where(t => t.Trace == "E6"));

            //WavefrontFileGroups.ExportByFaces(faceTriangles,"Wavefront/TraceE6");
            //var edgeTriangles = WireFrameMesh.Create();
            //edgeTriangles.AddRangeTriangles(facePlates.Triangles.Where(t => t.Trace[0] == 'E'));
            //WavefrontFile.Export(edgeTriangles, "Wavefront/EdgePlates");

            //WavefrontFile.Export(NormalOverlay(parallelSurface, 0.02), "Wavefront/curvedPlateNormals");
            //WavefrontFileGroups.ExportBySurfaces(parallelSurface, "Wavefront/ParallelSurface");
        }

        private void Part2()
        {
            var facePlate = PntFile.Import(WireFrameMesh.Create, "Pnt/FacePlates-0");

            var folds = GroupingCollection.ExtractFolds(facePlate.Triangles).ToArray();
            WavefrontFileGroups.ExportByFolds(facePlate, "Wavefront/Folds");
            Console.WriteLine($"Folds {folds.Length}");
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
    }
}
