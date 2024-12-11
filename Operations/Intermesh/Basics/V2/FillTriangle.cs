using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Interfaces;

namespace Operations.Intermesh.Basics.V2
{
    internal class FillTriangle
    {
        private static int _id = 0;
        public FillTriangle(IntermeshTriangle triangle, IntermeshPoint pointA, IntermeshPoint pointB, IntermeshPoint pointC) :
            this(pointA, triangle.NormalFromProjectedPoint(pointA.Point),
                pointB, triangle.NormalFromProjectedPoint(pointB.Point),
                pointC, triangle.NormalFromProjectedPoint(pointC.Point), triangle.PositionTriangle.Trace, triangle.PositionTriangle.Tag)
        { }
        public FillTriangle(IntermeshTriangle node) :
            this(node.A, node.PositionTriangle.A.Normal,
                node.B, node.PositionTriangle.B.Normal,
                node.C, node.PositionTriangle.C.Normal, node.PositionTriangle.Trace, node.PositionTriangle.Tag)
        { }
        public FillTriangle(IntermeshPoint pointA, Vector3D normalA, IntermeshPoint pointB, Vector3D normalB, IntermeshPoint pointC, Vector3D normalC, string trace, int tag)
        {
            Id = _id++;
            PointA = pointA;
            PointB = pointB;
            PointC = pointC;
            NormalA = normalA;
            NormalB = normalB;
            NormalC = normalC;
            Trace = trace;
            Tag = tag;
        }

        public int Id { get; }
        public string Trace { get; }
        public int Tag { get; }
        public bool Disabled { get; set; }

        public IntermeshPoint PointA { get; }
        public Vector3D NormalA { get; }
        public IntermeshPoint PointB { get; }
        public Vector3D NormalB { get; }
        public IntermeshPoint PointC { get; }
        public Vector3D NormalC { get; }
        public Triangle3D Triangle
        {
            get { return new Triangle3D(PointA.Point, PointB.Point, PointC.Point); }
        }

        public void AddWireFrameTriangle(IWireFrameMesh mesh)
        {
            if (Disabled) return;
            mesh.AddTriangle(PointA.Point, NormalA, PointB.Point, NormalB, PointC.Point, NormalC, Trace, Tag);
        }
    }
}
