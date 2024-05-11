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
            ParentGrid = A.Mesh.Id;
            if (a.Position == b.Position || a.Position == c.Position || b.Position == c.Position) { return; }
            if (!A.Mesh.AddNewTriangle(this)) { return; }
            A._triangles.Add(this);
            B._triangles.Add(this);
            C._triangles.Add(this);
        }
        internal PositionTriangle(PositionNormal a, PositionNormal b, PositionNormal c, string trace) : this(a, b, c)
        {
            Trace = trace;
        }

        public int Id { get; }
        public string Trace { get; set; }

        public int ParentGrid { get; private set; }

        public void GridClearMarks()
        {
            A.Mesh.IncrementMark();
        }

        private int _mark = 0;
        public void Mark()
        {
            _mark = A.Mesh.Mark;
        }

        public void ClearMark()
        {
            _mark--;
        }
        public bool IsMarked { get { return _mark == A.Mesh.Mark; } }

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

        public IEnumerable<PositionEdge> Edges
        {
            get
            {
                yield return new PositionEdge(A, B);
                yield return new PositionEdge(B, C);
                yield return new PositionEdge(C, A);
            }
        }

        public bool Disabled { get; private set; }
        public void Disable()
        {
            Disabled = true;
        }

        internal void DelinkPositionNormals()
        {
            A._triangles.Remove(this);
            B._triangles.Remove(this);
            C._triangles.Remove(this);
            Disable();
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

        public IReadOnlyList<PositionTriangle> ABadjacents
        {
            get
            {
                return A.PositionObject.PositionNormals.SelectMany(p => p.Triangles).
                    Intersect(B.PositionObject.PositionNormals.SelectMany(p => p.Triangles)).Where(t => t.Id != Id).ToList();
            }
        }

        public IReadOnlyList<PositionTriangle> BCadjacents
        {
            get
            {
                return B.PositionObject.PositionNormals.SelectMany(p => p.Triangles).
                    Intersect(C.PositionObject.PositionNormals.SelectMany(p => p.Triangles)).Where(t => t.Id != Id).ToList();
            }
        }

        public IReadOnlyList<PositionTriangle> CAadjacents
        {
            get
            {
                return C.PositionObject.PositionNormals.SelectMany(p => p.Triangles).
                    Intersect(A.PositionObject.PositionNormals.SelectMany(p => p.Triangles)).Where(t => t.Id != Id).ToList();
            }
        }

        public IReadOnlyList<PositionTriangle> Averticies
        {
            get
            {
                return A.PositionObject.PositionNormals.SelectMany(p => p.Triangles).Where(t => t.Id != Id).ToList();
            }
        }

        public IReadOnlyList<PositionTriangle> Bverticies
        {
            get
            {
                return B.PositionObject.PositionNormals.SelectMany(p => p.Triangles).Where(t => t.Id != Id).ToList();
            }
        }

        public IReadOnlyList<PositionTriangle> Cverticies
        {
            get
            {
                return C.PositionObject.PositionNormals.SelectMany(p => p.Triangles).Where(t => t.Id != Id).ToList();
            }
        }

        public IReadOnlyList<PositionTriangle> AexclusiveVerticies
        {
            get
            {
                return A.PositionObject.PositionNormals.SelectMany(p => p.Triangles).Where(t => t.Id != Id &&
                    !ABadjacents.Any(a => a.Id == t.Id) && !CAadjacents.Any(a => a.Id == t.Id)).ToList();
            }
        }

        public IReadOnlyList<PositionTriangle> BexclusiveVerticies
        {
            get
            {
                return B.PositionObject.PositionNormals.SelectMany(p => p.Triangles).Where(t => t.Id != Id &&
                    !ABadjacents.Any(a => a.Id == t.Id) && !BCadjacents.Any(a => a.Id == t.Id)).ToList();
            }
        }

        public IReadOnlyList<PositionTriangle> CexclusiveVerticies
        {
            get
            {
                return C.PositionObject.PositionNormals.SelectMany(p => p.Triangles).Where(t => t.Id != Id &&
                    !BCadjacents.Any(a => a.Id == t.Id) && !CAadjacents.Any(a => a.Id == t.Id)).ToList();
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
