using BasicObjects.GeometricObjects;
using Collections.Buckets.Interfaces;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshTriangle : IBox
    {
        private static int _id = 0;
        private PositionTriangle _triangle;
        private Dictionary<PositionTriangle, IntermeshTriangle> _lookup;
        internal IntermeshTriangle(PositionTriangle triangle, Dictionary<PositionTriangle, IntermeshTriangle> lookup)
        {
            _triangle = triangle;
            _lookup = lookup;
            A = triangle.A;
            B = triangle.B;
            C = triangle.C;
            Id = _id++;
        }

        public int Id { get; }
        public PositionTriangle PositionTriangle { get { return _triangle; } }

        public string Trace { get { return _triangle.Trace; } }

        public override int GetHashCode()
        {
            return Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not IntermeshTriangle) { return false; }
            IntermeshTriangle compare = (IntermeshTriangle)obj;
            return Id == compare.Id;
        }

        public Rectangle3D Box
        {
            get
            {
                return _triangle.Box;
            }
        }

        public PositionNormal A { get; private set; }
        public PositionNormal B { get; private set; }
        public PositionNormal C { get; private set; }

        public bool IsDegenerate
        {
            get
            {
                return A.Id == B.Id || B.Id == C.Id || C.Id == A.Id;
            }
        }

        public Triangle3D Triangle
        {
            get { return _triangle.Triangle; }
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

        private IReadOnlyList<IntermeshTriangle> _abAdjacents;
        private IReadOnlyList<IntermeshTriangle> _bcAdjacents;
        private IReadOnlyList<IntermeshTriangle> _caAdjacents;
        private IReadOnlyList<IntermeshTriangle> _aVerticies;
        private IReadOnlyList<IntermeshTriangle> _bVerticies;
        private IReadOnlyList<IntermeshTriangle> _cVerticies;
        private IReadOnlyList<IntermeshTriangle> _aExclusiveVerticies;
        private IReadOnlyList<IntermeshTriangle> _bExclusiveVerticies;
        private IReadOnlyList<IntermeshTriangle> _cExclusiveVerticies;
        private IReadOnlyList<IntermeshTriangle> _adjacents;
        private IReadOnlyList<IntermeshTriangle> _exclusives;

        public IReadOnlyList<IntermeshTriangle> ABadjacents
        {
            get
            {
                if (_abAdjacents is null)
                {
                    _abAdjacents = _triangle.ABadjacents.Select(t => _lookup[t]).ToList();
                }
                return _abAdjacents;
            }
        }

        public IReadOnlyList<IntermeshTriangle> BCadjacents
        {
            get
            {
                if (_bcAdjacents is null)
                {
                    _bcAdjacents = _triangle.BCadjacents.Select(t => _lookup[t]).ToList();
                }
                return _bcAdjacents;
            }
        }

        public IReadOnlyList<IntermeshTriangle> CAadjacents
        {
            get
            {
                if (_caAdjacents is null)
                {
                    _caAdjacents = _triangle.CAadjacents.Select(t => _lookup[t]).ToList();
                }
                return _caAdjacents;
            }
        }

        public IReadOnlyList<IntermeshTriangle> Averticies
        {
            get
            {
                if (_aVerticies is null)
                {
                    _aVerticies = _triangle.Averticies.Select(t => _lookup[t]).ToList();
                }
                return _aVerticies;
            }
        }

        public IReadOnlyList<IntermeshTriangle> Bverticies
        {
            get
            {
                if (_bVerticies is null)
                {
                    _bVerticies = _triangle.Bverticies.Select(t => _lookup[t]).ToList();
                }
                return _bVerticies;
            }
        }

        public IReadOnlyList<IntermeshTriangle> Cverticies
        {
            get
            {
                if (_cVerticies is null)
                {
                    _cVerticies = _triangle.Cverticies.Select(t => _lookup[t]).ToList();
                }
                return _cVerticies;
            }
        }

        public IReadOnlyList<IntermeshTriangle> AexclusiveVerticies
        {
            get
            {
                if (_aExclusiveVerticies is null)
                {
                    _aExclusiveVerticies = _triangle.AexclusiveVerticies.Select(t => _lookup[t]).ToList();

                }
                return _aExclusiveVerticies;
            }
        }

        public IReadOnlyList<IntermeshTriangle> BexclusiveVerticies
        {
            get
            {
                if (_bExclusiveVerticies is null)
                {
                    _bExclusiveVerticies = _triangle.BexclusiveVerticies.Select(t => _lookup[t]).ToList();
                }
                return _bExclusiveVerticies;
            }
        }

        public IReadOnlyList<IntermeshTriangle> CexclusiveVerticies
        {
            get
            {
                if (_cExclusiveVerticies is null)
                {
                    _cExclusiveVerticies = _triangle.CexclusiveVerticies.Select(t => _lookup[t]).ToList();
                }
                return _cExclusiveVerticies;
            }
        }



        public IReadOnlyList<IntermeshTriangle> AdjacentTriangles
        {
            get
            {
                if (_adjacents is null)
                {
                    _adjacents = ABadjacents.Concat(BCadjacents).Concat(CAadjacents).DistinctBy(t => t.Id).ToList();
                }
                return _adjacents;
            }
        }

        public IReadOnlyList<IntermeshTriangle> ExclusiveTriangles
        {
            get
            {
                if (_exclusives is null)
                {
                    _exclusives = AexclusiveVerticies.Concat(BexclusiveVerticies).Concat(CexclusiveVerticies).DistinctBy(t => t.Id).ToList();
                }
                return _exclusives;
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

        private IntermeshEdge _edgeAB;
        private IntermeshEdge _edgeBC;
        private IntermeshEdge _edgeCA;

        public IntermeshEdge EdgeAB
        {
            get
            {
                if (_edgeAB is null)
                {
                    var existingEdge = AdjacentTriangles.SelectMany(t => t.ExistingEdges).
                        FirstOrDefault(e => e.HasPosition(A.PositionObject) && e.HasPosition(B.PositionObject));
                    var allAdjacents = ABadjacents.ToList();
                    allAdjacents.Add(this);
                    if (existingEdge is not null) { _edgeAB = existingEdge; } else { _edgeAB = new IntermeshEdge(A, B, allAdjacents); }
                }
                return _edgeAB;
            }
        }
        public IntermeshEdge EdgeBC
        {
            get
            {
                if (_edgeBC is null)
                {
                    var existingEdge = AdjacentTriangles.SelectMany(t => t.ExistingEdges).
                        FirstOrDefault(e => e.HasPosition(B.PositionObject) && e.HasPosition(C.PositionObject));
                    var allAdjacents = BCadjacents.ToList();
                    allAdjacents.Add(this);
                    if (existingEdge is not null) { _edgeBC = existingEdge; } else { _edgeBC = new IntermeshEdge(B, C, allAdjacents); }
                }
                return _edgeBC;
            }
        }
        public IntermeshEdge EdgeCA {
            get
            {
                if (_edgeCA is null)
                {
                    var existingEdge = AdjacentTriangles.SelectMany(t => t.ExistingEdges).
                        FirstOrDefault(e => e.HasPosition(C.PositionObject) && e.HasPosition(A.PositionObject));
                    var allAdjacents = CAadjacents.ToList();
                    allAdjacents.Add(this);
                    if (existingEdge is not null) { _edgeCA = existingEdge; } else { _edgeCA = new IntermeshEdge(C, A, allAdjacents); }
                }
                return _edgeCA;
            }
        }

        private IEnumerable<IntermeshEdge> ExistingEdges
        {
            get
            {
                if (_edgeAB is not null) yield return _edgeAB;
                if (_edgeBC is not null) yield return _edgeBC;
                if (_edgeCA is not null) yield return _edgeCA;
            }
        }

        public IEnumerable<IntermeshEdge> Edges
        {
            get
            {
                yield return EdgeAB;
                yield return EdgeBC;
                yield return EdgeCA;
            }
        }

        public List<IntermeshTriangle> Gathering { get; } = new List<IntermeshTriangle>();

        public Dictionary<int, IntermeshIntersectionSet> IntersectionTable { get; } = new Dictionary<int, IntermeshIntersectionSet>();
        public IEnumerable<IntermeshIntersection> Intersections
        {
            get { return IntersectionTable.Values.Where(i => i.Intersections is not null).SelectMany(s => s.Intersections).DistinctBy(i => i.Id); }
        }

        public IEnumerable<IntermeshDivision> Divisions
        {
            get { return Intersections.Where(i => !i.Disabled).SelectMany(i => i.Divisions).DistinctBy(i => i.Id); }
        }

        public IEnumerable<IntersectionVertexContainer> GetIntersectionPoints()
        {
            return Intersections.SelectMany(i => i.VerticiesAB).DistinctBy(v => v.Id);
        }

        public void ClearNullIntersections()
        {
            var nullKeys = IntersectionTable.Where(p => p.Value.Intersections is null).Select(p => p.Key).ToArray();
            foreach (var key in nullKeys)
            {
                IntersectionTable.Remove(key);
            }
        }

        public void ClearDisabledIntersections()
        {
            foreach (var pair in IntersectionTable) { pair.Value.Intersections = pair.Value.Intersections.Where(s => !s.Disabled).ToArray(); }

            var nullKeys = IntersectionTable.Where(p => p.Value.Intersections.Length == 0).Select(p => p.Key).ToArray();
            foreach (var key in nullKeys)
            {
                IntersectionTable.Remove(key);
            }
        }

        public void AddWireFrameTriangle(IWireFrameMesh mesh)
        {
            mesh.AddTriangle(A.Position, A.Normal, B.Position, B.Normal, C.Position, C.Normal, Trace);
        }

        public void ExportWithDivisions(IWireFrameMesh mesh)
        {
            mesh.AddTriangle(A.Position, A.Normal, B.Position, B.Normal, C.Position, C.Normal);

            foreach (var division in Divisions)
            {
                var mid = (division.VertexA.Point + division.VertexB.Point) / 2;

                mesh.AddTriangle(division.VertexA.Point, Triangle.Normal, division.VertexB.Point, Triangle.Normal, mid, Triangle.Normal);
            }
        }

        public void ExportWithIntersections(IWireFrameMesh mesh)
        {
            mesh.AddTriangle(A.Position, A.Normal, B.Position, B.Normal, C.Position, C.Normal);

            foreach (var intersection in Intersections)
            {
                var mid = (intersection.VertexA.Point + intersection.VertexB.Point) / 2;

                mesh.AddTriangle(intersection.VertexA.Point, Triangle.Normal, intersection.VertexB.Point, Triangle.Normal, mid, Triangle.Normal);
            }
        }
    }
}
