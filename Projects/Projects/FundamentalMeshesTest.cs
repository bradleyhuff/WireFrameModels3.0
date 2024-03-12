using BaseObjects.Transformations;
using BasicObjects;
using BasicObjects.GeometricObjects;
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
    public class FundamentalMeshesTest : ProjectBase
    {
        protected override void RunProject()
        {
            //CylinderTest();
            //ConeTest();
            //CuboidTest();
            SphereTest();
            //TorusTest();
        }

        private void ConeTest()
        {
            var cone = Cone.Create(0.5, 3, 64);
            cone.Transform(Transform.Scale(0.25, 1, 1));

            TableDisplays.ShowCountSpread("Position normal triangle counts", cone.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            TableDisplays.ShowCountSpread("Position normal counts", cone.Positions, p => p.PositionNormals.Count);

            PntFile.Export(cone, "Pnt/Cone");
            WavefrontFile.Export(cone, "Wavefront/Cone");

            cone.Transform(Transform.Reflection(Vector3D.BasisY));

            TableDisplays.ShowCountSpread("Position normal triangle counts", cone.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            TableDisplays.ShowCountSpread("Position normal counts", cone.Positions, p => p.PositionNormals.Count);

            PntFile.Export(cone, "Pnt/ConeReflection");
            WavefrontFile.Export(cone, "Wavefront/ConeReflection");
        }

        private void CylinderTest()
        {
            var cylinder = Cylinder.Create(0.5, 3, 64);
            cylinder.Transform(Transform.Scale(0.25, 1, 1));

            TableDisplays.ShowCountSpread("Position normal triangle counts", cylinder.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            TableDisplays.ShowCountSpread("Position normal counts", cylinder.Positions, p => p.PositionNormals.Count);

            PntFile.Export(cylinder, "Pnt/Cylinder");
            WavefrontFile.Export(cylinder, "Wavefront/Cylinder");
        }

        private void CuboidTest()
        {
            var cuboid = Cuboid.Create(1.0, 3, 2.0, 5, 3.0, 7);

            cuboid.Transform(Transform.ShearXY(4.5, 1.5));

            TableDisplays.ShowCountSpread("Position normal triangle counts", cuboid.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            TableDisplays.ShowCountSpread("Position normal counts", cuboid.Positions, p => p.PositionNormals.Count);

            PntFile.Export(cuboid, "Pnt/Cuboid");
            WavefrontFile.Export(cuboid, "Wavefront/Cuboid");
        }

        private void SphereTest()
        {
            var sphere = Sphere.Create(1, 32);
            //var shear = Transform.ShearXY(0.2, 0.3);
            //sphere.Transform(Transform.Scale(0.25, 1, 1));

            TableDisplays.ShowCountSpread("Position normal triangle counts", sphere.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            TableDisplays.ShowCountSpread("Position normal counts", sphere.Positions, p => p.PositionNormals.Count);

            PntFile.Export(sphere, "Pnt/Sphere");
            WavefrontFile.Export(sphere, "Wavefront/Sphere");
        }

        private void TorusTest()
        {
            var torus = Torus.Create(4, 2, 64, 32);
            //var squash = Transform.Scale(1, 0.25, 2);
            //var squash = Transform.ShearXZ(2, 1);
            //torus.Transform(Transform.Scale(0.1, 0.25, 2));
            torus.Transform(Transform.ShearXZ(10, 10));

            TableDisplays.ShowCountSpread("Position normal triangle counts", torus.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            TableDisplays.ShowCountSpread("Position normal counts", torus.Positions, p => p.PositionNormals.Count);

            PntFile.Export(torus, "Pnt/Torus");
            WavefrontFile.Export(torus, "Wavefront/Torus");
        }
    }
}
