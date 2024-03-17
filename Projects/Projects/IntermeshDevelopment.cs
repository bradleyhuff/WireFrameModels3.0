using BaseObjects.Transformations;
using BasicObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using FundamentalMeshes;
using Operations.Intermesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireFrameModels3._0;

namespace Projects.Projects
{
    public class IntermeshDevelopment : ProjectBase
    {
        protected override void RunProject()
        {
            var sphere = Sphere.Create(1, 32);
            var sphere2 = sphere.Clone();
            var sphere3 = sphere.Clone();
            var sphere4 = sphere.Clone();
            var sphere5 = sphere.Clone();
            var sphere6 = sphere.Clone();

            sphere.Transform(Transform.Scale(0.70));
            sphere2.Transform(Transform.Scale(0.65));
            sphere3.Transform(Transform.Scale(0.60));
            sphere4.Transform(Transform.Scale(0.55));
            sphere5.Transform(Transform.Scale(0.75));
            sphere6.Transform(Transform.Scale(0.80));

            var cube = Cuboid.Create(1, 2, 1, 2, 1, 2);

            var spheres = sphere;
            spheres.AddGrid(sphere2);
            spheres.AddGrid(sphere3);
            spheres.AddGrid(sphere4);
            spheres.AddGrid(sphere5);
            spheres.AddGrid(sphere6);

            var intermesh = WireFrameMesh.CreateMesh();
            intermesh.AddGrid(cube);
            intermesh.AddGrid(spheres);

            TableDisplays.ShowCountSpread("Position normal triangle counts", intermesh.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            TableDisplays.ShowCountSpread("Position normal counts", intermesh.Positions, p => p.PositionNormals.Count);

            //
            var output = Operations.Intermesh.ElasticIntermeshOperations.Operations.Intermesh(intermesh);


            //
            PntFile.Export(output, "Pnt/Intermesh");
            WavefrontFile.Export(output, "Wavefront/Intermesh");
        }
    }
}
