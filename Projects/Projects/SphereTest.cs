using BaseObjects.Transformations;
using BasicObjects;
using FileExportImport;
using FundamentalMeshes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireFrameModels3._0;

namespace Projects.Projects
{
    public class SphereTest : ProjectBase
    {
        protected override void RunProject()
        {
            var sphere = Sphere.Create(0.5, 256);
            //var squash = Transform.Scale(0.5, 1, 2);
            //var shear = Transform.ShearXY(0.2, 0.3);
            //sphere.Transformation(shear.Apply);

            TableDisplays.ShowCountSpread("Position normal triangle counts", sphere.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            TableDisplays.ShowCountSpread("Position normal counts", sphere.Positions, p => p.PositionNormals.Count);

            PntFile.Export(sphere, "Pnt/Sphere");
            WavefrontFile.Export(sphere, "Wavefront/Sphere");
        }
    }
}
