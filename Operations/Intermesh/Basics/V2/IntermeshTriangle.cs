using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets.Interfaces;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using Operations.Intermesh.Elastics;
using Operations.PlanarFilling.Basics;
using Operations.SurfaceSegmentChaining.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics.V2
{
    internal class IntermeshTriangle : IBox
    {
        private static int _id = 0;
        private PositionTriangle _triangle;
        internal IntermeshTriangle(PositionTriangle triangle)
        {
            _triangle = triangle;
            Id = _id++;
        }

        public int Id { get; }
        public PositionTriangle PositionTriangle { get { return _triangle; } }

        public IntermeshPoint A { get; set; }
        public IntermeshPoint B { get; set; }
        public IntermeshPoint C { get; set; }

        public IntermeshSegment AB
        {
            get
            {
                var key = new Combination2(A.Id, B.Id);
                return _segments.SingleOrDefault(s => s.Key == key);
            }
        }

        public IntermeshSegment BC
        {
            get
            {
                var key = new Combination2(B.Id, C.Id);
                return _segments.SingleOrDefault(s => s.Key == key);
            }
        }

        public IntermeshSegment CA
        {
            get
            {
                var key = new Combination2(C.Id, A.Id);
                return _segments.SingleOrDefault(s => s.Key == key);
            }
        }

        public IEnumerable<IntermeshSegment> PerimeterSegments
        {
            get
            {
                if (AB is not null) { yield return AB; }
                if (BC is not null) { yield return BC; }
                if (CA is not null) { yield return CA; }
            }
        }

        public IEnumerable<IntermeshSegment> InternalSegments
        {
            get
            {
                return Segments.Where(s => !PerimeterSegments.Any(p => p.Id == s.Id));
            }
        }

        public bool HasDivisions
        {
            get
            {
                if (InternalDivisions.Any()) { return true; }

                return PerimeterSegments.Any(p => p.HasDivisionPoints);
            }
        }

        public Triangle3D Triangle
        {
            get { return _triangle.Triangle; }
        }

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

        public List<IntermeshTriangle> Gathering { get; } = new List<IntermeshTriangle>();

        public Dictionary<int, IntermeshIntersection> GatheringSets { get; } = new Dictionary<int, IntermeshIntersection>();

        private List<IntermeshSegment> _segments = new List<IntermeshSegment>();
        public IReadOnlyList<IntermeshSegment> Segments
        {
            get { return _segments; }
        }

        public List<FillTriangle> Fillings { get; set; } = new List<FillTriangle>();
        public bool Add(IntermeshSegment segment)
        {
            if (_segments.Any(t => t.Key == segment.Key)) { return false; }
            _segments.Add(segment);
            return true;
        }

        private List<IntermeshDivision> _divisions = new List<IntermeshDivision>();
        private Combination2Dictionary<bool> _divisionTable = new Combination2Dictionary<bool>();
        public void AddRange(IEnumerable<IntermeshDivision> divisions)
        {
            foreach (var division in divisions)
            {
                if (_divisionTable.ContainsKey(division.Key)) { continue; }
                _divisions.Add(division);

                _abInternalPoints = null;
                _bcInternalPoints = null;
                _caInternalPoints = null;

                _divisionTable[division.Key] = true;
            }
        }

        public IReadOnlyList<IntermeshDivision> Divisions
        {
            get { return _divisions; }
        }
        public IEnumerable<IntermeshDivision> InternalDivisions
        {
            get
            {
                var perimeter = PerimeterDivisions.Select(p => p.Id).ToArray();
                return _divisions.Where(d => !perimeter.Contains(d.Id));
            }
        }
        public IEnumerable<IntermeshDivision> ABDivisions
        {
            get { return _divisions.Where(d => PointIsOnAB(d.A) && PointIsOnAB(d.B)); }
        }
        public IEnumerable<IntermeshDivision> BCDivisions
        {
            get { return _divisions.Where(d => PointIsOnBC(d.A) && PointIsOnBC(d.B)); }
        }
        public IEnumerable<IntermeshDivision> CADivisions
        {
            get { return _divisions.Where(d => PointIsOnCA(d.A) && PointIsOnCA(d.B)); }
        }
        public IEnumerable<IntermeshDivision> PerimeterDivisions
        {
            get { return ABDivisions.Union(BCDivisions).Union(CADivisions); }
        }

        private IntermeshPoint[] _abInternalPoints;
        private IntermeshPoint[] _bcInternalPoints;
        private IntermeshPoint[] _caInternalPoints;

        public IntermeshPoint[] ABInternalPoints
        {
            get 
            { 
                if (_abInternalPoints == null)
                {
                    _abInternalPoints = ABDivisions.SelectMany(d => d.Points).DistinctBy(p => p.Id).Where(p => p.Id != A.Id && p.Id != B.Id).
                        OrderBy(p => Point3D.Distance(p.Point, A.Point)).ToArray();
                }
                return _abInternalPoints;
            }
        }
        public IntermeshPoint[] BCInternalPoints
        {
            get
            {
                if (_bcInternalPoints == null)
                {
                    _bcInternalPoints = BCDivisions.SelectMany(d => d.Points).DistinctBy(p => p.Id).Where(p => p.Id != B.Id && p.Id != C.Id).
                        OrderBy(p => Point3D.Distance(p.Point, B.Point)).ToArray();
                }
                return _bcInternalPoints;
            }
        }
        public IntermeshPoint[] CAInternalPoints
        {
            get
            {
                if (_caInternalPoints == null)
                {
                    _caInternalPoints = CADivisions.SelectMany(d => d.Points).DistinctBy(p => p.Id).Where(p => p.Id != C.Id && p.Id != A.Id).
                        OrderBy(p => Point3D.Distance(p.Point, C.Point)).ToArray();
                }
                return _caInternalPoints;
            }
        }

        private bool PointIsOnAB(IntermeshPoint point)
        {
            return point.Segments.Any(s => s.Id == AB.Id);
        }
        private bool PointIsOnBC(IntermeshPoint point)
        {
            return point.Segments.Any(s => s.Id == BC.Id);
        }
        private bool PointIsOnCA(IntermeshPoint point)
        {
            return point.Segments.Any(s => s.Id == CA.Id);
        }

        public Ray3D RayFromProjectedPoint(Point3D point)
        {
            var projection = Triangle.Plane.Projection(point);

            var c = Triangle.GetBarycentricCoordinate(projection);
            var normal = (c.λ1 * PositionTriangle.A.Normal.Direction + c.λ2 * PositionTriangle.B.Normal.Direction + c.λ3 * PositionTriangle.C.Normal.Direction).Direction;

            return new Ray3D(projection, normal);
        }

        public Vector3D NormalFromProjectedPoint(Point3D point)
        {
            var projection = Triangle.Plane.Projection(point);

            var c = Triangle.GetBarycentricCoordinate(projection);

            return (c.λ1 * PositionTriangle.A.Normal.Direction + c.λ2 * PositionTriangle.B.Normal.Direction + c.λ3 * PositionTriangle.C.Normal.Direction).Direction;
        }

        public IEnumerable<SurfaceSegmentContainer<IntermeshPoint>> GetPerimeterSurfaceSegments()
        {
            foreach (var segment in PerimeterDivisions)
            {
                yield return new SurfaceSegmentContainer<IntermeshPoint>(
                    new SurfaceRayContainer<IntermeshPoint>(RayFromProjectedPoint(segment.A.Point), Triangle.Normal, segment.A.Id, segment.A),
                    new SurfaceRayContainer<IntermeshPoint>(RayFromProjectedPoint(segment.B.Point), Triangle.Normal, segment.B.Id, segment.B));
            }
        }

        public IEnumerable<SurfaceSegmentContainer<IntermeshPoint>> GetDividingSurfaceSegments()
        {
            foreach (var segment in InternalDivisions)
            {
                yield return new SurfaceSegmentContainer<IntermeshPoint>(
                    new SurfaceRayContainer<IntermeshPoint>(RayFromProjectedPoint(segment.A.Point), Triangle.Normal, segment.A.Id, segment.A),
                    new SurfaceRayContainer<IntermeshPoint>(RayFromProjectedPoint(segment.B.Point), Triangle.Normal, segment.B.Id, segment.B));
            }
        }

        public SurfaceSegmentSets<PlanarFillingGroup, IntermeshPoint> CreateSurfaceSegmentSet()
        {
            return new SurfaceSegmentSets<PlanarFillingGroup, IntermeshPoint>
            {
                NodeId = Id,
                GroupObject = new PlanarFillingGroup(Triangle.Plane, Triangle.Box.Diagonal),
                DividingSegments = GetDividingSurfaceSegments().ToArray(),
                PerimeterSegments = GetPerimeterSurfaceSegments().ToArray()
            };
        }

        public void ExportTriangle(IWireFrameMesh mesh)
        {
            mesh.AddTriangle(A.Point, Triangle.Normal, B.Point, Triangle.Normal, C.Point, Triangle.Normal, "", 0);
        }

        public IEnumerable<IWireFrameMesh> ExportWithDivisionsSplit(IWireFrameMesh mesh)
        {
            foreach (var division in PerimeterDivisions)
            {
                var mid = (division.A.Point + division.B.Point) / 2;
                var newMesh = WireFrameMesh.Create();
                newMesh.AddTriangle(division.A.Point, Triangle.Normal, division.B.Point, Triangle.Normal, mid, Triangle.Normal, "", 0);
                yield return newMesh;
            }
        }

        public IEnumerable<IWireFrameMesh> ExportWithInternalDivisionsSplit(IWireFrameMesh mesh)
        {
            foreach (var division in InternalDivisions)
            {
                var mid = (division.A.Point + division.B.Point) / 2;
                var newMesh = WireFrameMesh.Create();
                newMesh.AddTriangle(division.A.Point, Triangle.Normal, division.B.Point, Triangle.Normal, mid, Triangle.Normal, "", 0);
                yield return newMesh;
            }
        }

        public IEnumerable<IWireFrameMesh> ExportWithGatheringSplit(IWireFrameMesh mesh)
        {
            foreach (var gathering in Gathering)
            {
                var newMesh = WireFrameMesh.Create();
                newMesh.AddTriangle(gathering.A.Point, gathering.Triangle.Normal, gathering.B.Point, gathering.Triangle.Normal, gathering.C.Point, gathering.Triangle.Normal, "", 0);
                yield return newMesh;
            }
        }

    }
}
