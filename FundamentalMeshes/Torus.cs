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
    public static class Torus
    {
        public static IWireFrameMesh Create(double majorRadius, double minorRadius, int majorCircumferenceSteps, int minorCircumferenceSteps)
        {
            IWireFrameMesh torus = WireFrameMesh.CreateMesh();

            var majorDelta = (Math.PI * 2.0) / majorCircumferenceSteps;
            var minorDelta = (Math.PI * 2.0) / minorCircumferenceSteps;

            var pathPoints = new Point3D[minorCircumferenceSteps + 1];
            var pathNormals = new Vector3D[minorCircumferenceSteps + 1];

            var generatorPoint = new Point3D(minorRadius, 0, 0);
            var radiusTranslation = new Point3D(majorRadius, 0, 0);
            var translate = Transform.Translation(radiusTranslation);

            for (int i = 0; i <= minorCircumferenceSteps; i++)
            {
                var theta = i * minorDelta;
                var transform = Transform.Rotation(Vector3D.BasisZ, theta);
                var pathPoint = transform.Apply(generatorPoint);
                pathNormals[i] = transform.Apply(Point3D.Zero, Vector3D.BasisX);
                pathPoints[i] = translate.Apply(pathPoint);
            }

            for (int i = 0; i <= majorCircumferenceSteps; i++)
            {
                var theta = i * majorDelta;
                var transform = Transform.Rotation(Vector3D.BasisY, theta);

                for (int j = 0; j <= minorCircumferenceSteps; j++)
                {
                    torus.AddPoint(pathPoints[j], pathNormals[j], transform);
                }
                torus.EndRow();
            }
            torus.EndGrid();
            return torus;
        }
    }
}
