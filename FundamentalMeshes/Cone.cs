using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using Operations.PositionRemovals;
using Operations.PositionRemovals.Interfaces;

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

            //cone.RemovePosition(baseCenter.PositionObject);

            var convergePoint = new ConvergePoint() { ConvergeAt = firstPoint.PositionObject };
            cone.RemovePosition(baseCenter.PositionObject, convergePoint);

            return cone;
        }

        internal class ConvergePoint : ISharedFillConditionals
        {
            public Position ConvergeAt { get; set; }
            public bool AllowFill(PositionNormal a, PositionNormal b, PositionNormal c)
            {
                return a.PositionObject.Id == ConvergeAt.Id || b.PositionObject.Id == ConvergeAt.Id || c.PositionObject.Id == ConvergeAt.Id;
            }
        }
    }
}
