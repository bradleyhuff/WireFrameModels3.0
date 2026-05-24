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
            this(pointA.Point, triangle.NormalFromProjectedPoint(pointA.Point),
            pointB.Point, triangle.NormalFromProjectedPoint(pointB.Point),
            pointC.Point, triangle.NormalFromProjectedPoint(pointC.Point))
        { }

        internal FillTriangle(Point3D pointA, Vector3D normalA, Point3D pointB, Vector3D normalB, Point3D pointC, Vector3D normalC)
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
        }

        public int Id { get; }

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
            PositionTriangle = mesh.AddTriangle(PointA, NormalA, PointB, NormalB, PointC, NormalC, String.Empty, 0);
        }
    }
}
