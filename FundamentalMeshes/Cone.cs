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
    public static class Cone
    {
        public static IWireFrameMesh Create(double radius, double height, int circumferenceSteps)
        {
            IWireFrameMesh cone = WireFrameMesh.CreateMesh();

            var delta = (Math.PI * 2.0) / circumferenceSteps;

            var vertexPoint = new Point3D(0, height, 0);
            var basePoint = new Point3D(radius, 0, 0);
            var normal = new Vector3D(1, radius / height, 0);
            var downNormal = -Vector3D.BasisY;

            for (int i = 0; i <= circumferenceSteps; i++)
            {
                var theta = i * delta;
                var transform = Transform.Rotation(Vector3D.BasisY, theta);

                cone.AddPoint(vertexPoint, normal, transform);
                cone.AddPoint(basePoint, normal, transform);
                cone.AddPoint(basePoint, downNormal, transform);
                cone.AddPoint(Point3D.Zero, downNormal);
                cone.EndRow();
            }
            cone.EndGrid();

            return cone;
        }
    }
}
