using Collections.Buckets.Interfaces;
using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.WireFrameMesh.Interfaces;
using BasicObjects.MathExtensions;

namespace Collections.WireFrameMesh.Basics
{
    public class PositionTriangle : IBox, ITriangle
    {
        private static int _id = 0;
        private static object lockObject = new object();
        internal PositionTriangle(PositionNormal a, PositionNormal b, PositionNormal c)
        {
            A = a;
            B = b;
            C = c;
            Key = new Combination3(a.PositionObject.Id, b.PositionObject.Id, c.PositionObject.Id);
            SurfaceKey = new Combination3(a.Id, b.Id, c.Id);
            lock (lockObject)
            {
                Id = _id++;
            }
            ParentGrid = A.Mesh.Id;
            if (a.Position == b.Position || a.Position == c.Position || b.Position == c.Position) { return; }
            if (!A.Mesh.AddNewTriangle(this)) { return; }
            A._triangles.Add(this);
            B._triangles.Add(this);
            C._triangles.Add(this);
        }
        internal PositionTriangle(PositionNormal a, PositionNormal b, PositionNormal c, string trace, int tag) : this(a, b, c)
        {
            Trace = trace;
            Tag = tag;
        }

        public int Id { get; }
        public string Trace { get; set; }
        public int Tag { get; set; }

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
        public Combination3 SurfaceKey { get; }

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

        public bool IsFolded
        {
            get
            {
                if (Vector3D.Dot(A.Normal, B.Normal) < 0) return true;
                if (Vector3D.Dot(A.Normal, C.Normal) < 0) return true;
                if (Vector3D.Dot(B.Normal, C.Normal) < 0) return true;

                return false;
            }
        }

        public IEnumerable<PositionEdge> Edges
        {
            get
            {
                yield return new PositionEdge(A, B, this);
                yield return new PositionEdge(B, C, this);
                yield return new PositionEdge(C, A, this);
            }
        }

        public IEnumerable<PositionEdge> OpenEdges
        {
            get
            {
                if (!ABadjacents.Any()) yield return new PositionEdge(A, B, this);
                if (!BCadjacents.Any()) yield return new PositionEdge(B, C, this);
                if (!CAadjacents.Any()) yield return new PositionEdge(C, A, this);
            }
        }

        public IEnumerable<PositionEdge> ClosedEdges
        {
            get
            {
                if (ABadjacents.Any()) yield return new PositionEdge(A, B, this);
                if (BCadjacents.Any()) yield return new PositionEdge(B, C, this);
                if (CAadjacents.Any()) yield return new PositionEdge(C, A, this);
            }
        }

        public IEnumerable<PositionNormal> ClosedPoints
        {
            get
            {
                if (CAadjacents.Any() && ABadjacents.Any()) yield return A;
                if (ABadjacents.Any() && BCadjacents.Any()) yield return B;
                if (BCadjacents.Any() && CAadjacents.Any()) yield return C;
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

        public IReadOnlyList<PositionTriangle> ABadjacents
        {
            get
            {
                return A.PositionObject.Triangles.Intersect(B.PositionObject.Triangles).Where(t => t.Id != Id).ToList();
            }
        }

        public IReadOnlyList<PositionTriangle> BCadjacents
        {
            get
            {
                return B.PositionObject.Triangles.Intersect(C.PositionObject.Triangles).Where(t => t.Id != Id).ToList();
            }
        }

        public IReadOnlyList<PositionTriangle> CAadjacents
        {
            get
            {
                return C.PositionObject.Triangles.Intersect(A.PositionObject.Triangles).Where(t => t.Id != Id).ToList();
            }
        }

        public IEnumerable<PositionTriangle> AllAdjacents
        {
            get
            {
                foreach (var adjacent in ABadjacents) { yield return adjacent; }
                foreach (var adjacent in BCadjacents) { yield return adjacent; }
                foreach (var adjacent in CAadjacents) { yield return adjacent; }
            }
        }

        public IEnumerable<PositionTriangle> SingleAdjacents
        {
            get
            {
                if (ABadjacents.Count() == 1) { yield return ABadjacents[0]; }
                if (BCadjacents.Count() == 1) { yield return BCadjacents[0]; }
                if (CAadjacents.Count() == 1) { yield return CAadjacents[0]; }
            }
        }

        public IEnumerable<PositionTriangle> SingleAdjacentsWithCondition(Func<PositionTriangle, PositionTriangle, bool> condition)
        {
            if (ABadjacents.Count(a => condition(this, a)) == 1) { yield return ABadjacents[0]; }
            if (BCadjacents.Count(a => condition(this, a)) == 1) { yield return BCadjacents[0]; }
            if (CAadjacents.Count(a => condition(this, a)) == 1) { yield return CAadjacents[0]; }
        }

        public int AdjacentAnyCount
        {
            get { return (ABadjacents.Any() ? 1 : 0) + (BCadjacents.Any() ? 1 : 0) + (CAadjacents.Any() ? 1 : 0); }
        }

        public int AdjacentAnyWithCondition(Func<PositionTriangle, PositionTriangle, bool> condition)
        {
            return (ABadjacents.Any(a => condition(this, a)) ? 1 : 0) + (BCadjacents.Any(a => condition(this, a)) ? 1 : 0) + (CAadjacents.Any(a => condition(this, a)) ? 1 : 0);
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

        public Tuple<int, int, int> PositionCardinalities
        {
            get { return new Tuple<int, int, int>(A.PositionObject.Cardinality, B.PositionObject.Cardinality, C.PositionObject.Cardinality); }
        }

        public void ExportWithCenterNormal(IWireFrameMesh mesh)
        {
            mesh.AddTriangle(A.Position, A.Normal, B.Position, B.Normal, C.Position, C.Normal, "", 0);
            mesh.AddTriangle(Triangle.Center, Vector3D.Zero, Triangle.Center + 0.005 * Triangle.Normal.Direction, Vector3D.Zero, Triangle.Center + 0.01 * Triangle.Normal.Direction, Vector3D.Zero, "", 0);
        }
    }
}
