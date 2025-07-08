using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;

namespace Operations.ParallelSurfaces.Internals
{
    internal interface IPerimeterChainLink
    {
        public int Id { get; }
        public Point3D A { get; }
        public Vector3D NormalA { get; }
        public Vector3D BinormalA { get; }

        public Point3D B { get; }
        public Vector3D NormalB { get; }
        public Vector3D BinormalB { get; }

        public PerimeterChainLink Next { get; set; }
        public PerimeterChainLink Last { get; set; }
    }

    internal class PerimeterChainLinkDirectSet : IPerimeterChainLink
    {
        public PerimeterChainLinkDirectSet(Point3D a, Vector3D normalA, Vector3D binormalA, Point3D b, Vector3D normalB, Vector3D binormalB)
        {
            A = a;
            NormalA = normalA;
            BinormalA = binormalA;
            B = b;
            NormalB = normalB;
            BinormalB = binormalB;
            Id = PerimeterChainLink.GetId();
        }
        public int Id { get; }

        public Point3D A { get; }
        public Vector3D NormalA { get; }
        public Vector3D BinormalA { get; }

        public Point3D B { get; }
        public Vector3D NormalB { get; }
        public Vector3D BinormalB { get; }

        public PerimeterChainLink Next { get; set; }
        public PerimeterChainLink Last { get; set; }
    }

    internal class PerimeterChainLink : IPerimeterChainLink
    {
        private static int _id = 0;
        private static object lockObject = new object();

        private PositionNormal _a;
        private PositionNormal _b;

        public PerimeterChainLink(PositionNormal a, PositionNormal b)
        {
            _a = a;
            _b = b;
            A = a.Position;
            NormalA = a.Normal;
            B = b.Position;
            NormalB = b.Normal;
            Id = GetId();
        }

        internal static int GetId()
        {
            lock (lockObject)
            {
                return _id++;
            }
        }

        public int Id { get; }
        public Point3D A { get; }
        public Vector3D NormalA { get; }
        public Vector3D BinormalA { 
            get
            {
                if(_BinormalA is null)
                {
                    GetBinormals();
                }
                return _BinormalA;
            }
        }
        public Point3D B { get; }
        public Vector3D NormalB { get; }
        public Vector3D BinormalB
        {
            get
            {
                if (_BinormalB is null)
                {
                    GetBinormals();
                }
                return _BinormalB;
            }
        }
        public PerimeterChainLink Next { get; set; }
        public PerimeterChainLink Last { get; set; }

        private Vector3D _BinormalA;
        private Vector3D _BinormalB;
        private void GetBinormals()
        {
            var pair = Vector3D.GetNearestParallelPair(GetBinormals(_a), GetBinormals(_b));
            _BinormalA = pair.VectorA;
            _BinormalB = pair.VectorB;
        }

        private IEnumerable<Vector3D> GetBinormals(PositionNormal pn)
        {
            var surfacePlane = new Plane(Point3D.Zero, pn.Normal);
            return pn.PositionObject.PositionNormals.Where(pn2 => pn2.Id != pn.Id).Select(pn3 => pn3.Normal).Select(pn4 => surfacePlane.Projection(pn4)).ToList();
        }
    }
}
