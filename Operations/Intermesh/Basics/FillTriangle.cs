using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets.Interfaces;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    public class FillTriangle : IBox
    {
        private static int _id = 0;
        private static object lockObject = new object();

        internal FillTriangle(IntermeshTriangle triangle, IntermeshPoint pointA, IntermeshPoint pointB, IntermeshPoint pointC) :
            this(triangle, pointA, triangle.NormalFromProjectedPoint(pointA.Point),
            pointB, triangle.NormalFromProjectedPoint(pointB.Point),
            pointC, triangle.NormalFromProjectedPoint(pointC.Point))
        { Key = new Combination3(pointA.Id, pointB.Id, pointC.Id); }

        private FillTriangle(IntermeshTriangle triangle, IntermeshPoint pointA, Vector3D normalA, IntermeshPoint pointB, Vector3D normalB, IntermeshPoint pointC, Vector3D normalC)
        {
            lock (lockObject)
            {
                Id = _id++;
            }

            PointA = pointA;
            PointB = pointB;
            PointC = pointC;
            NormalA = normalA;
            NormalB = normalB;
            NormalC = normalC;
            _positionTriangle = triangle.PositionTriangle;
            _intermeshTriangle = triangle;
        }

        public Combination3 Key { get; }
        public Combination LoopKey { get; set; }
        public IntermeshPoint[] Loop { get; set; }

        public int Id { get; }

        private Triangle3D _triangle = null;
        private PositionTriangle _positionTriangle;
        private IntermeshTriangle _intermeshTriangle;

        public Rectangle3D Box
        {
            get
            {
                return Triangle.Box;
            }
        }
        public IntermeshTriangle Parent
        {
            get { return _intermeshTriangle; }
        }

        public IntermeshPoint PointA { get; }
        public Vector3D NormalA { get; }
        public IntermeshPoint PointB { get; }
        public Vector3D NormalB { get; }
        public IntermeshPoint PointC { get; }
        public Vector3D NormalC { get; }

        public bool IsDisabled { get; set; }
        public Triangle3D Triangle
        {
            get
            {
                if (_triangle is null)
                {
                    _triangle = new Triangle3D(PointA.Point, PointB.Point, PointC.Point);
                }
                return _triangle;
            }
        }

        public void AddWireFrameTriangle(IWireFrameMesh mesh)
        {
            var positionTriangle = mesh.AddTriangle(PointA.Point, NormalA, PointB.Point, NormalB, PointC.Point, NormalC, _positionTriangle.Trace, _positionTriangle.Tag);
        }
    }
}
