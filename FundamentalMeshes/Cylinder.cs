using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundamentalMeshes
{
    public static class Cylinder
    {
        public static IWireFrameMesh Create(double radius, double height, int circumferenceSteps)
        {
            IWireFrameMesh cylinder = WireFrameMesh.Create();

            var delta = (Math.PI * 2.0) / circumferenceSteps;

            var upNormal = Vector3D.BasisY;
            var vertexPoint = new Point3D(0, height, 0);
            var rimPoint = new Point3D(radius, height, 0);
            var basePoint = new Point3D(radius, 0, 0);
            var normal = Vector3D.BasisX;
            var downNormal = -Vector3D.BasisY;

            for (int i = 0; i <= circumferenceSteps; i++)
            {
                var theta = i * delta;
                var transform = Transform.Rotation(Vector3D.BasisY, theta);

                cylinder.AddPoint(vertexPoint, upNormal);
                cylinder.AddPoint(rimPoint, upNormal, transform);
                cylinder.AddPoint(rimPoint, normal, transform);
                cylinder.AddPoint(basePoint, normal, transform);
                cylinder.AddPoint(basePoint, downNormal, transform);
                cylinder.AddPoint(Point3D.Zero, downNormal);
                cylinder.EndRow();
            }
            cylinder.EndGrid();

            return cylinder;
        }
    }
}
