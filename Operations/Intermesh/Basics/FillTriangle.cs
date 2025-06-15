using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;

namespace Operations.Intermesh.Basics
{
    internal class FillTriangle
    {
        private static int _id = 0;
        public FillTriangle(IntermeshTriangle triangle, IntermeshPoint pointA, IntermeshPoint pointB, IntermeshPoint pointC) :
            this(triangle, pointA, triangle.NormalFromProjectedPoint(pointA.Point),
                pointB, triangle.NormalFromProjectedPoint(pointB.Point),
                pointC, triangle.NormalFromProjectedPoint(pointC.Point), -1, triangle.PositionTriangle.Trace, triangle.PositionTriangle.Tag)
        { }
        public FillTriangle(IntermeshTriangle node) :
            this(node, node.A, node.PositionTriangle.A.Normal,
                node.B, node.PositionTriangle.B.Normal,
                node.C, node.PositionTriangle.C.Normal, -1, node.PositionTriangle.Trace, node.PositionTriangle.Tag)
        { }
        public FillTriangle(IntermeshTriangle node, IntermeshPoint pointA, Vector3D normalA, IntermeshPoint pointB, Vector3D normalB, IntermeshPoint pointC, Vector3D normalC, int fillId, string trace, int tag)
        {
            Id = _id++;
            ParentIntermesh = node;
            PointA = pointA;
            PointB = pointB;
            PointC = pointC;
            NormalA = normalA;
            NormalB = normalB;
            NormalC = normalC;
            FillId = fillId;
            Trace = trace;
            Tag = tag;
        }

        public int Id { get; }
        public int FillId { get; }
        public string Trace { get; }
        public int Tag { get; }
        public bool Disabled { get; set; }

        public IntermeshTriangle ParentIntermesh { get; }

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

        public PositionTriangle PositionTriangle { get; private set; }

        public void AddWireFrameTriangle(IWireFrameMesh mesh)
        {
            if (Disabled) return;
            PositionTriangle = mesh.AddTriangle(PointA.Point, NormalA, PointB.Point, NormalB, PointC.Point, NormalC, Trace, Tag);
        }

        public void ExportTriangle(IWireFrameMesh mesh)
        {
            mesh.AddTriangle(PointA.Point, Triangle.Normal, PointB.Point, Triangle.Normal, PointC.Point, Triangle.Normal, "", 0);
        }
    }
}
