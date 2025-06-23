using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;

namespace Operations.ParallelSurfaces.Internals
{
    internal interface IPerimeterChainLink
    {
        public int Id { get; }
        public Point3D A { get; }
        public Vector3D NormalA { get; }
        public Vector3D BitangentA { get; }

        public Point3D B { get; }
        public Vector3D NormalB { get; }
        public Vector3D BitangentB { get; }

        public PerimeterChainLink Next { get; set; }
        public PerimeterChainLink Last { get; set; }
    }

    internal class PerimeterChainLinkDirectSet: IPerimeterChainLink
    {
        public PerimeterChainLinkDirectSet(Point3D a, Vector3D normalA, Vector3D bitangentA, Point3D b, Vector3D normalB, Vector3D bitangentB)
        {
            A = a;
            NormalA = normalA;
            BitangentA = bitangentA;
            B = b;
            NormalB = normalB;
            BitangentB = bitangentB;
            Id = PerimeterChainLink.GetId();
        }
        public int Id { get; }

        public Point3D A { get; }
        public Vector3D NormalA { get; }
        public Vector3D BitangentA { get; }

        public Point3D B { get; }
        public Vector3D NormalB { get; }
        public Vector3D BitangentB { get; }

        public PerimeterChainLink Next { get; set; }
        public PerimeterChainLink Last { get; set; }
    }

    internal class PerimeterChainLink: IPerimeterChainLink
    {
        private PositionNormal _a;
        private PositionNormal _b;
        private static int _id = 0;
        private static object lockObject = new object();
        public PerimeterChainLink(PositionNormal a, PositionNormal b)
        {
            A = a.Position;
            NormalA = a.Normal;
            _a = a;
            B = b.Position;
            NormalB = b.Normal;
            _b = b;
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
        public Vector3D BitangentA { 
            get
            {
                if(_bitangentA is null)
                {
                    GetBitangents();
                }
                return _bitangentA;
            }
        }
        public Point3D B { get; }
        public Vector3D NormalB { get; }
        public Vector3D BitangentB
        {
            get
            {
                if (_bitangentB is null)
                {
                    GetBitangents();
                }
                return _bitangentB;
            }
        }
        public PerimeterChainLink Next { get; set; }
        public PerimeterChainLink Last { get; set; }

        private Vector3D _bitangentA;
        private Vector3D _bitangentB;
        private void GetBitangents()
        {
            var pair = Vector3D.GetNearestParallelPair(GetBitangents(_a), GetBitangents(_b));
            _bitangentA = pair.VectorA;
            _bitangentB = pair.VectorB;
        }

        private IEnumerable<Vector3D> GetBitangents(PositionNormal pn)
        {
            var surfacePlane = new Plane(Point3D.Zero, pn.Normal);
            return pn.PositionObject.PositionNormals.Where(pn2 => pn2.Id != pn.Id).Select(pn3 => pn3.Normal).Select(pn4 => surfacePlane.Projection(pn4)).ToList();
        }
    }
}
