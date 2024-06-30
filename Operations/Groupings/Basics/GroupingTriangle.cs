using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;

namespace Operations.Groupings.Basics
{
    internal class GroupingTriangle
    {
        private static int _id = 0;
        private PositionTriangle _triangle;
        private Dictionary<PositionTriangle, GroupingTriangle> _lookup;
        internal GroupingTriangle(PositionTriangle triangle, Dictionary<PositionTriangle, GroupingTriangle> lookup)
        {
            A = triangle.A;
            B = triangle.B;
            C = triangle.C;
            _triangle = triangle;
            _lookup = lookup;
            Trace = triangle.Trace;
            Id = _id++;
        }

        public int Id { get; }
        public string Trace { get; }

        public PositionNormal A { get; private set; }
        public PositionNormal B { get; private set; }
        public PositionNormal C { get; private set; }

        public PositionTriangle PositionTriangle
        {
            get { return _triangle; }
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not GroupingTriangle) { return false; }
            GroupingTriangle compare = (GroupingTriangle)obj;
            return Id == compare.Id;
        }

        private IReadOnlyList<GroupingTriangle> _abAdjacents;
        private IReadOnlyList<GroupingTriangle> _bcAdjacents;
        private IReadOnlyList<GroupingTriangle> _caAdjacents;

        public IReadOnlyList<GroupingTriangle> ABadjacents
        {
            get
            {
                if (_abAdjacents is null)
                {
                    _abAdjacents = _triangle.ABadjacents.Select(GetTriangle).ToList();
                }
                return _abAdjacents;
            }
        }

        public IReadOnlyList<GroupingTriangle> BCadjacents
        {
            get
            {
                if (_bcAdjacents is null)
                {
                    _bcAdjacents = _triangle.BCadjacents.Select(GetTriangle).ToList();
                }
                return _bcAdjacents;
            }
        }

        public IReadOnlyList<GroupingTriangle> CAadjacents
        {
            get
            {
                if (_caAdjacents is null)
                {
                    _caAdjacents = _triangle.CAadjacents.Select(GetTriangle).ToList();
                }
                return _caAdjacents;
            }
        }

        private GroupingTriangle GetTriangle(PositionTriangle triangle)
        {
            if (!_lookup.ContainsKey(triangle)) { _lookup[triangle] = new GroupingTriangle(triangle, _lookup); }
            return _lookup[triangle];
        }

        public Triangle3D Triangle
        {
            get { return _triangle.Triangle; }
        }

        public bool HasAnOpenEdge
        {
            get { return ABadjacents.Count == 0 || BCadjacents.Count == 0 || CAadjacents.Count == 0; }
        }
        public int GroupId { get; set; }
        public bool Spanned { get; set; }
        public int Seed { get; set; }//lower seed values seed first
        public IEnumerable<IReadOnlyList<GroupingTriangle>> Adjacents
        {
            get
            {
                yield return ABadjacents;
                yield return BCadjacents;
                yield return CAadjacents;
            }
        }

        public void AddWireFrameTriangle(IWireFrameMesh mesh)
        {
            mesh.AddTriangle(A.Position, A.Normal, B.Position, B.Normal, C.Position, C.Normal, Trace);
        }
    }
}
