using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.Buckets.Interfaces;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Operations.Intermesh.Classes;
using Operations.ParallelSurfaces.Internals;

namespace Operations.Intermesh.Basics
{
    public class FillTriangle : IBox
    {
        private static int _id = 0;
        private static object lockObject = new object();
        internal FillTriangle(IntermeshTriangle triangle, IntermeshPoint pointA, IntermeshPoint pointB, IntermeshPoint pointC) :
            this(triangle, pointA.Point, triangle.NormalFromProjectedPoint(pointA.Point),
                pointB.Point, triangle.NormalFromProjectedPoint(pointB.Point),
                pointC.Point, triangle.NormalFromProjectedPoint(pointC.Point), -1, triangle.PositionTriangle.Trace, triangle.PositionTriangle.Tag)
        { }
        internal FillTriangle(IntermeshTriangle node) :
            this(node, node.A.Point, node.PositionTriangle.A.Normal,
                node.B.Point, node.PositionTriangle.B.Normal,
                node.C.Point, node.PositionTriangle.C.Normal, -1, node.PositionTriangle.Trace, node.PositionTriangle.Tag)
        { }

        public FillTriangle(Point3D pointA, Vector3D normalA, Point3D pointB, Vector3D normalB, Point3D pointC, Vector3D normalC, string trace, int tag) : this(null, pointA, normalA, pointB, normalB, pointC, normalC, 0, trace, tag) { }
        internal FillTriangle(IntermeshTriangle node, Point3D pointA, Vector3D normalA, Point3D pointB, Vector3D normalB, Point3D pointC, Vector3D normalC, int fillId, string trace, int tag)
        {
            lock (lockObject)
            {
                Id = _id++;
            }
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

        public int[] NodeIndices { get; set; } = new int[0];
        public string Trace { get; }
        public int Tag { get; }
        public bool Disabled { get; set; }

        internal IntermeshTriangle ParentIntermesh { get; }

        private Triangle3D _triangle = null;

        public Rectangle3D Box
        {
            get
            {
                return Triangle.Box;
            }
        }

        public Point3D PointA { get; }
        public Vector3D NormalA { get; }
        public Point3D PointB { get; }
        public Vector3D NormalB { get; }
        public Point3D PointC { get; }
        public Vector3D NormalC { get; }
        public Triangle3D Triangle
        {
            get
            {
                if (_triangle is null)
                {
                    _triangle = new Triangle3D(PointA, PointB, PointC);
                }
                return _triangle;
            }
        }

        public PositionTriangle PositionTriangle { get; private set; }

        public void AddWireFrameTriangle(IWireFrameMesh mesh)
        {
            if (Disabled) return;
            PositionTriangle = mesh.AddTriangle(PointA, NormalA, PointB, NormalB, PointC, NormalC, Trace, Tag);
        }

        public void ExportTriangle(IWireFrameMesh mesh)
        {
            mesh.AddTriangle(PointA, Triangle.Normal, PointB, Triangle.Normal, PointC, Triangle.Normal, "", 0);
        }
    }
}
