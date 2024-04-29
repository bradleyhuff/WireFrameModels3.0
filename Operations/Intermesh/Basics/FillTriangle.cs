using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Interfaces;
using Operations.Intermesh.Elastics;

namespace Operations.Intermesh.Basics
{
    internal class FillTriangle
    {
        private static int _id = 0;
        public FillTriangle(ElasticTriangle triangle, ElasticVertexCore pointA, ElasticVertexCore pointB, ElasticVertexCore pointC) :
            this(pointA, triangle.NormalFromProjectedPoint(pointA.Point), pointB, triangle.NormalFromProjectedPoint(pointB.Point), pointC, triangle.NormalFromProjectedPoint(pointC.Point), triangle.Trace)
        { }
        public FillTriangle(ElasticTriangle node) : this(node.AnchorA, node.NormalA, node.AnchorB, node.NormalB, node.AnchorC, node.NormalC, node.Trace) { }
        public FillTriangle(ElasticVertexCore pointA, Vector3D normalA, ElasticVertexCore pointB, Vector3D normalB, ElasticVertexCore pointC, Vector3D normalC, string trace)
        {
            Id = _id++;
            PointA = pointA;
            PointB = pointB;
            PointC = pointC;
            NormalA = normalA;
            NormalB = normalB;
            NormalC = normalC;
            Trace = trace;
        }

        public int Id { get; }
        public string Trace { get; }

        public ElasticVertexCore PointA { get; }
        public Vector3D NormalA { get; }
        public ElasticVertexCore PointB { get; }
        public Vector3D NormalB { get; }
        public ElasticVertexCore PointC { get; }
        public Vector3D NormalC { get; }
        public Triangle3D Triangle
        {
            get { return new Triangle3D(PointA.Point, PointB.Point, PointC.Point); }
        }

        public IEnumerable<ElasticVertexCore> Points
        {
            get
            {
                yield return PointA;
                yield return PointB;
                yield return PointC;
            }
        }

        public void AddWireFrameTriangle(IWireFrameMesh mesh)
        {
            mesh.AddTriangle(PointA.Point, NormalA, PointB.Point, NormalB, PointC.Point, NormalC, Trace);
        }
    }
}
