using BaseObjects;
using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using FundamentalMeshes;
using Operations.Basics;
using Operations.Groupings.FileExportImport;
using Operations.SetOperators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireFrameModels3._0;

namespace Projects.Projects
{
    public class DiceMeshTest : ProjectBase
    {
        protected override void RunProject()
        {
            var cube = Cuboid.Create(1, 2, 1, 2, 1, 2);
            var sphere = Sphere.Create(1, 16);
            sphere.Apply(Transform.Scale(0.0250));

            var spheres = sphere.Clones(9).ToArray();

            spheres[0].Apply(Transform.Translation(new Vector3D(0.50000111, 0.50000111, 0)));
            spheres[1].Apply(Transform.Translation(new Vector3D(0.52000001, 0.52000001, 0)));
            //Transformations.Translation(spheres[0], new Vector3D(0.50000111, 0.50000111, 0));
            //Transformations.Translation(spheres[1], new Vector3D(0.52000001, 0.52000001, 0));

            spheres[2].Apply(Transform.Translation(new Vector3D(0.93, 0.75, 0)));
            spheres[7].Apply(Transform.Translation(new Vector3D(0.95, 0.73, 0)));
            spheres[8].Apply(Transform.Translation(new Vector3D(0.928, 0.73, 0)));
            spheres[6].Apply(Transform.Translation(new Vector3D(0.0178, 0.0178, 0.0178)));
            spheres[3].Apply(Transform.Translation(new Vector3D(0.90, 0.80, 0)));
            spheres[4].Apply(Transform.Translation(new Vector3D(0.7500001, 0.75, 0)));
            spheres[5].Apply(Transform.Translation(new Vector3D(0.7600001, 0.76, 0)));

            //Transformations.Translation(spheres[2], new Vector3D(0.93, 0.75, 0));
            //Transformations.Translation(spheres[7], new Vector3D(0.95, 0.73, 0));
            //Transformations.Translation(spheres[8], new Vector3D(0.928, 0.73, 0));
            //Transformations.Translation(spheres[6], new Vector3D(0.0178, 0.0178, 0.0178));
            //Transformations.Translation(spheres[3], new Vector3D(0.90, 0.80, 0));
            //Transformations.Translation(spheres[4], new Vector3D(0.7500001, 0.75, 0));
            //Transformations.Translation(spheres[5], new Vector3D(0.7600001, 0.76, 0));

            var allSpheres = WireFrameMesh.Create();
            allSpheres.AddGrid(spheres[0]);
            allSpheres.AddGrid(spheres[1]);
            allSpheres.AddGrid(spheres[2]);
            allSpheres.AddGrid(spheres[3]);
            allSpheres.AddGrid(spheres[4]);
            allSpheres.AddGrid(spheres[5]);
            allSpheres.AddGrid(spheres[6]);
            allSpheres.AddGrid(spheres[7]);
            allSpheres.AddGrid(spheres[8]);

            ConsoleLog.MaximumLevels = 8;
            var grid = cube.Difference(allSpheres);

            grid.ShowVitals();

            PntFile.Export(grid, "Pnt/Dice");
            WavefrontFile.Export(grid, "Wavefront/Dice");
            WavefrontFileGroups.ExportBySurfaces(grid, "Wavefront/Dice");
            //WavefrontFile.Export(NormalOverlay(grid, 0.003), "Wavefront/DiceNormals");
            {
                var test = WireFrameMesh.Create();
                var openEdges = grid.Triangles.SelectMany(t => t.OpenEdges);
                //Console.WriteLine($"Triangles {string.Join(",", output.Triangles.Select(t => t.Key))}");
                System.Console.WriteLine($"Open edges {string.Join(",", openEdges.Select(o => o.Segment))}");
                System.Console.WriteLine($"Open edges {string.Join(",", openEdges.Select(o => $"[{o.A.PositionObject.Point}<{o.A.PositionObject.Id}>, {o.B.PositionObject.Point}<{o.B.PositionObject.Id}>]"))}");
                System.Console.WriteLine($"Open edges {string.Join(",", openEdges.Select(o => $"[{o.A.Normal}<{o.A.PositionObject.Id}>, {o.B.Normal}<{o.B.PositionObject.Id}>]"))}");
                test.AddRangeTriangles(openEdges.Select(e => e.Plot), "", 0);
                WavefrontFile.Export(test, $"Wavefront/TagOpenEdges");
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
