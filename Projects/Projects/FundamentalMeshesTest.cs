using BaseObjects.Transformations;
using BasicObjects;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
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
            //SphereTest();
            TorusTest();
        }

        private void ConeTest()
        {
            var cone = Cone.Create(0.5, 3, 128);
            //cone.Transform(Transform.Scale(0.10, 1, 1));
            cone.Apply(Transform.ShearXZ(2, 2));

            TableDisplays.ShowCountSpread("Position normal triangle counts", cone.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            TableDisplays.ShowCountSpread("Position normal counts", cone.Positions, p => p.PositionNormals.Count);

            PntFile.Export(cone, "Pnt/Cone");
            WavefrontFile.Export(cone, "Wavefront/Cone");

            //cone.Transform(Transform.Reflection(Vector3D.BasisY));

            //TableDisplays.ShowCountSpread("Position normal triangle counts", cone.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            //TableDisplays.ShowCountSpread("Position normal counts", cone.Positions, p => p.PositionNormals.Count);

            //PntFile.Export(cone, "Pnt/ConeReflection");
            //WavefrontFile.Export(cone, "Wavefront/ConeReflection");
        }

        private void CylinderTest()
        {
            var cylinder = Cylinder.Create(0.5, 3, 64);
            cylinder.Apply(Transform.Scale(0.25, 1, 1));

            TableDisplays.ShowCountSpread("Position normal triangle counts", cylinder.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            TableDisplays.ShowCountSpread("Position normal counts", cylinder.Positions, p => p.PositionNormals.Count);

            PntFile.Export(cylinder, "Pnt/Cylinder");
            WavefrontFile.Export(cylinder, "Wavefront/Cylinder");
        }

        private void CuboidTest()
        {
            var cuboid = Cuboid.Create(1.0, 3, 2.0, 5, 3.0, 7);

            cuboid.Apply(Transform.ShearXY(4.5, 1.5));

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
            var shape = Torus.Create(4, 2, 8, 128);
            //var shape = Cuboid.Create(1, 1, 1, 1, 1, 1);
            //var squash = Transform.Scale(1, 0.25, 2);
            //var squash = Transform.ShearXZ(2, 1);
            //torus.Transform(Transform.Scale(0.5, 1, 2));
            //torus.Transform(Transform.Scale(1, 0.2, 1));
            //shape.Transform(Transform.Scale(2.05, 2.45, 1) * Transform.Rotation(Vector3D.BasisZ, -0.375) * Transform.Scale(1, 0.20, 1) * Transform.Rotation(Vector3D.BasisZ, 1.1));
            //shape.Transform(Transform.ShearXZ(2, 2));
            //shape.Transform(Transform.ShearXY(2, 2));
            //shape.Transform(Transform.ShearYZ(2, 2));

            TableDisplays.ShowCountSpread("Position normal triangle counts", shape.Positions, p => p.PositionNormals.Sum(n => n.Triangles.Count));
            TableDisplays.ShowCountSpread("Position normal counts", shape.Positions, p => p.PositionNormals.Count);

            PntFile.Export(shape, "Pnt/Shape");
            WavefrontFile.Export(shape, "Wavefront/Shape");
            WavefrontFile.Export(NormalOverlay(shape, 0.25), "Wavefront/ShapeNormals");
        }

        private IWireFrameMesh NormalOverlay(IWireFrameMesh input, double radius)
        {
            var output = WireFrameMesh.CreateMesh();

            foreach(var positionNormal in input.Positions.SelectMany(p => p.PositionNormals))
            {
                var pointA = output.AddPointNoRow(positionNormal.Position, Vector3D.Zero);
                var pointB = output.AddPointNoRow(positionNormal.Position + 0.5 * radius * positionNormal.Normal.Direction, Vector3D.Zero);
                var pointC = output.AddPointNoRow(positionNormal.Position + radius * positionNormal.Normal.Direction, Vector3D.Zero);
                new PositionTriangle(pointA, pointB, pointC);

            }

            return output;
        }
    }
}
