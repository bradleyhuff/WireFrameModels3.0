using BasicObjects.GeometricObjects;
using Collections.Buckets.Interfaces;
using Collections.Buckets;
using Collections.WireFrameMesh.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    public class IntermeshTriangle :IBox
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

        public List<IntermeshTriangle> Gathering { get; } = new List<IntermeshTriangle>();

        public Dictionary<int, IntermeshIntersection> IntersectionTable { get; } = new Dictionary<int, IntermeshIntersection>();
        public IEnumerable<IntermeshIntersection> Intersections
        {
            get { return IntersectionTable.Values.Where(i => i.Intersection is not null).DistinctBy(i => i.Id); }
        }

        public IEnumerable<IntermeshDivision> Divisions
        {
            get { return Intersections.Where(i => !i.Disabled).SelectMany(i => i.Divisions).DistinctBy(i => i.Id); }
        }

        public void ClearNullIntersections()
        {
            var nullKeys = IntersectionTable.Where(p => p.Value.Intersection is null).Select(p => p.Key).ToArray();
            foreach (var key in nullKeys)
            {
                IntersectionTable.Remove(key);
            }
        }

        public void ClearDisabledIntersections()
        {
            var nullKeys = IntersectionTable.Where(p => p.Value.Disabled).Select(p => p.Key).ToArray();
            foreach (var key in nullKeys)
            {
                IntersectionTable.Remove(key);
            }
        }
    }
}
