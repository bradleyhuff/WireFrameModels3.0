using Collections.Buckets.Interfaces;
using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.WireFrameMesh.Interfaces;
using BasicObjects.MathExtensions;

namespace Collections.WireFrameMesh.Basics
{
    public class PositionTriangle : IBox
    {
        private static int _id = 0;
        internal PositionTriangle(PositionNormal a, PositionNormal b, PositionNormal c)
        {
            A = a;
            B = b;
            C = c;
            Key = new Combination3(a.PositionObject.Id, b.PositionObject.Id, c.PositionObject.Id);
            Id = _id++;
            if (a.Position == b.Position || a.Position == c.Position || b.Position == c.Position) { return; }
            if (A.Mesh._keys.ContainsKey(Key)) { return; }
            A._triangles.Add(this);
            B._triangles.Add(this);
            C._triangles.Add(this);
            A.Mesh._triangles.Add(this);
            A.Mesh._keys[Key] = true;
        }
        internal PositionTriangle(PositionNormal a, PositionNormal b, PositionNormal c, string trace) : this(a, b, c)
        {
            Trace = trace;
        }

        public int Id { get; }
        public string Trace { get; set; }

        public Combination3 Key { get; }

        public override int GetHashCode()
        {
            return Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not PositionTriangle) { return false; }
            PositionTriangle compare = (PositionTriangle)obj;
            return Id == compare.Id;
        }

        public PositionNormal A { get; private set; }
        public PositionNormal B { get; private set; }
        public PositionNormal C { get; private set; }

        public void LinkPositionNormals(PositionNormal a, PositionNormal b, PositionNormal c)
        {
            DelinkPositionNormals();

            A = a;
            B = b;
            C = c;
            A._triangles.Add(this);
            B._triangles.Add(this);
            C._triangles.Add(this);
            A.Mesh._triangles.Add(this);
        }

        public void DelinkPositionNormals()
        {
            A?._triangles.Remove(this);
            B?._triangles.Remove(this);
            C?._triangles.Remove(this);
            A?.Mesh._triangles.Remove(this);
            A = null;
            B = null;
            C = null;
            ClearStates();
        }

        private IReadOnlyList<PositionTriangle> _abAdjacents;
        private IReadOnlyList<PositionTriangle> _bcAdjacents;
        private IReadOnlyList<PositionTriangle> _caAdjacents;
        private IReadOnlyList<PositionTriangle> _aVerticies;
        private IReadOnlyList<PositionTriangle> _bVerticies;
        private IReadOnlyList<PositionTriangle> _cVerticies;
        private IReadOnlyList<PositionTriangle> _aExclusiveVerticies;
        private IReadOnlyList<PositionTriangle> _bExclusiveVerticies;
        private IReadOnlyList<PositionTriangle> _cExclusiveVerticies;
        private void ClearStates()
        {
            _abAdjacents = null;
            _bcAdjacents = null;
            _caAdjacents = null;
            _aVerticies = null;
            _bVerticies = null;
            _cVerticies = null;
            _aExclusiveVerticies = null;
            _bExclusiveVerticies = null;
            _cExclusiveVerticies = null;
        }

        public IReadOnlyList<PositionTriangle> ABadjacents
        {
            get
            {
                if(_abAdjacents is null)
                {
                    _abAdjacents = A.PositionObject.PositionNormals.SelectMany(p => p.Triangles).
                        Intersect(B.PositionObject.PositionNormals.SelectMany(p => p.Triangles)).Where(t => t.Id != Id).ToList();
                }
                return _abAdjacents;
            }
        }

        public IReadOnlyList<PositionTriangle> BCadjacents
        {
            get
            {
                if (_bcAdjacents is null)
                {
                    _bcAdjacents = B.PositionObject.PositionNormals.SelectMany(p => p.Triangles).
                        Intersect(C.PositionObject.PositionNormals.SelectMany(p => p.Triangles)).Where(t => t.Id != Id).ToList();
                }
                return _bcAdjacents;
            }
        }

        public IReadOnlyList<PositionTriangle> CAadjacents
        {
            get
            {
                if (_caAdjacents is null)
                {
                    _caAdjacents = C.PositionObject.PositionNormals.SelectMany(p => p.Triangles).
                        Intersect(A.PositionObject.PositionNormals.SelectMany(p => p.Triangles)).Where(t => t.Id != Id).ToList();
                }
                return _caAdjacents;
            }
        }

        public IReadOnlyList<PositionTriangle> Averticies
        {
            get
            {
                if(_aVerticies is null)
                {
                    _aVerticies = A.PositionObject.PositionNormals.SelectMany(p => p.Triangles).Where(t => t.Id != Id).ToList();
                }
                return _aVerticies;
            }
        }

        public IReadOnlyList<PositionTriangle> Bverticies
        {
            get
            {
                if (_bVerticies is null)
                {
                    _bVerticies = B.PositionObject.PositionNormals.SelectMany(p => p.Triangles).Where(t => t.Id != Id).ToList();
                }
                return _bVerticies;
            }
        }

        public IReadOnlyList<PositionTriangle> Cverticies
        {
            get
            {
                if (_cVerticies is null)
                {
                    _cVerticies = C.PositionObject.PositionNormals.SelectMany(p => p.Triangles).Where(t => t.Id != Id).ToList();
                }
                return _cVerticies;
            }
        }

        public IReadOnlyList<PositionTriangle> AexclusiveVerticies
        {
            get
            {
                if (_aExclusiveVerticies is null)
                {
                    _aExclusiveVerticies = A.PositionObject.PositionNormals.SelectMany(p => p.Triangles).Where(t => t.Id != Id && 
                        !ABadjacents.Any(a => a.Id == t.Id) && !CAadjacents.Any(a => a.Id == t.Id)).ToList();

                }
                return _aExclusiveVerticies;
            }
        }

        public IReadOnlyList<PositionTriangle> BexclusiveVerticies
        {
            get
            {
                if (_bExclusiveVerticies is null)
                {
                    _bExclusiveVerticies = B.PositionObject.PositionNormals.SelectMany(p => p.Triangles).Where(t => t.Id != Id &&
                        !ABadjacents.Any(a => a.Id == t.Id) && !BCadjacents.Any(a => a.Id == t.Id)).ToList();

                }
                return _bExclusiveVerticies;
            }
        }

        public IReadOnlyList<PositionTriangle> CexclusiveVerticies
        {
            get
            {
                if (_cExclusiveVerticies is null)
                {
                    _cExclusiveVerticies = C.PositionObject.PositionNormals.SelectMany(p => p.Triangles).Where(t => t.Id != Id &&
                        !BCadjacents.Any(a => a.Id == t.Id) && !CAadjacents.Any(a => a.Id == t.Id)).ToList();

                }
                return _cExclusiveVerticies;
            }
        }

        private Rectangle3D _box = null;
        public Rectangle3D Box
        {
            get
            {
                if (_box is null && Triangle is not null)
                {
                    _box = Triangle.Box.Margin(BoxBucket.MARGINS);
                }
                return _box;
            }
        }

        public IEnumerable<PositionNormal> Positions
        {
            get
            {
                yield return A;
                yield return B;
                yield return C;
            }
        }

        public IEnumerable<PositionNormal[]> Edges
        {
            get
            {
                yield return [A, B];
                yield return [B, C];
                yield return [C, A];
            }
        }

        private Triangle3D _triangle = null;
        public Triangle3D Triangle
        {
            get
            {
                if (_triangle is null)
                {
                    _triangle = new Triangle3D(
                        A.Position,
                        B.Position,
                        C.Position);
                }
                return _triangle;
            }
        }

        public static SurfaceTriangle GetSurfaceTriangle(PositionTriangle triangle)
        {
            return new SurfaceTriangle(
                PositionNormal.GetRay(triangle.A),
                PositionNormal.GetRay(triangle.B),
                PositionNormal.GetRay(triangle.C));
        }

        public IEnumerable<Point3D> Points
        {
            get
            {
                yield return A.Position;
                yield return B.Position;
                yield return C.Position;
            }
        }


        public void ExportWithCenterNormal(IWireFrameMesh mesh)
        {
            mesh.AddTriangle(A.Position, A.Normal, B.Position, B.Normal, C.Position, C.Normal);
            mesh.AddTriangle(Triangle.Center, Vector3D.Zero, Triangle.Center + 0.005 * Triangle.Normal.Direction, Vector3D.Zero, Triangle.Center + 0.01 * Triangle.Normal.Direction, Vector3D.Zero);
        }
    }
}
