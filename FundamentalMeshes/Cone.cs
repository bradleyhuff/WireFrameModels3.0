using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using Operations.PlanarFilling.Basics;
using Operations.PositionRemovals;
using Operations.PositionRemovals.FillActions;
using Operations.PositionRemovals.Interfaces;
using Operations.SurfaceSegmentChaining.Interfaces;

namespace FundamentalMeshes
{
    public static class Cone
    {
        public static IWireFrameMesh Create(double radius, double height, int circumferenceSteps)
        {
            IWireFrameMesh cone = WireFrameMesh.Create();

            var delta = (Math.PI * 2.0) / circumferenceSteps;

            var vertexPoint = new Point3D(0, height, 0);
            var basePoint = new Point3D(radius, 0, 0);
            var normal = new Vector3D(1, radius / height, 0);
            var downNormal = -Vector3D.BasisY;
            PositionNormal baseCenter = null;
            PositionNormal firstPoint = null;

            for (int i = 0; i <= circumferenceSteps; i++)
            {
                var theta = i * delta;
                var transform = Transform.Rotation(Vector3D.BasisY, theta);

                cone.AddPoint(vertexPoint, normal, transform);
                var circlePoint = cone.AddPoint(basePoint, normal, transform);
                if (i == 1) { firstPoint = circlePoint; }
                cone.AddPoint(basePoint, downNormal, transform);
                baseCenter = cone.AddPoint(Point3D.Zero, downNormal);
                cone.EndRow();
            }
            cone.EndGrid();

            cone.FanRemovePosition(baseCenter.PositionObject, firstPoint.PositionObject);

            return cone;
        }
    }
}
