using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.ParallelSurfaces.Internals
{
    internal class Quadrangle
    {
        private static int _id = 0;
        private static object lockObject = new object();

        private BasePoint _a, _b;
        private Vector3D _normalA;
        private Vector3D _normalB;

        private void GetNormals()
        {
            double maxDotProduct = -1;
            foreach (var normalA in _a.EdgeNormals)
            {
                foreach (var normalB in _b.EdgeNormals)
                {
                    var dotProduct = Vector3D.Dot(normalA, normalB);
                    if (dotProduct > maxDotProduct)
                    {
                        maxDotProduct = dotProduct;
                        _normalA = normalA;
                        _normalB = normalB;
                    }
                }
            }
        }
        public Quadrangle(BasePoint a, BasePoint b)
        {
            lock (lockObject)
            {
                Id = _id++;
            }
            _a = a;
            _b = b;
        }
        public int Id { get; }
        public Quadrangle Last { get; set; }
        public Quadrangle Next { get; set; }

        public Point3D BaseA { get { return _a.Position; } }
        public Point3D BaseB { get { return _b.Position; } }
        public Point3D SurfaceA { get; set; }
        public Point3D SurfaceB { get; set; }

        public Vector3D NormalA
        {
            get
            {
                if (_normalA is null)
                {
                    GetNormals();
                }
                return _normalA;
            }
        }
        public Vector3D NormalB
        {
            get
            {
                if (_normalB is null)
                {
                    GetNormals();
                }
                return _normalB;
            }
        }
    }
}
