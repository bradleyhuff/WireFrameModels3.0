﻿using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using FundamentalMeshes;
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

            var allSpheres = WireFrameMesh.CreateMesh();
            allSpheres.AddGrid(spheres[0]);
            allSpheres.AddGrid(spheres[1]);
            allSpheres.AddGrid(spheres[2]);
            allSpheres.AddGrid(spheres[3]);
            allSpheres.AddGrid(spheres[4]);
            allSpheres.AddGrid(spheres[5]);
            allSpheres.AddGrid(spheres[6]);
            allSpheres.AddGrid(spheres[7]);
            allSpheres.AddGrid(spheres[8]);
            var grid = WireFrameMesh.CreateMesh();
            grid = cube.Difference(allSpheres);

            WavefrontFile.Export(grid, "Wavefront/Dice");
            //WavefrontFileGroups.ExportByFaces(grid, "Wavefront/Dice");
            WavefrontFile.Export(NormalOverlay(grid, 0.003), "Wavefront/DiceNormals");
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