using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using Operations.Intermesh.Basics;
using Operations.SurfaceSegmentChaining.Basics;
using System.Xml.Linq;

namespace Operations.Intermesh.Elastics
{
    internal class ElasticTriangle
    {
        private static int _id = 0;
        public ElasticTriangle(ElasticVertexAnchor anchorA, Vector3D normalA, ElasticVertexAnchor anchorB, Vector3D normalB, ElasticVertexAnchor anchorC, Vector3D normalC,
            ElasticEdge perimeterEdgeAB, ElasticEdge perimeterEdgeBC, ElasticEdge perimeterEdgeCA)
        {
            Id = _id++;

            AnchorA = anchorA;
            AnchorB = anchorB;
            AnchorC = anchorC;

            NormalA = normalA;
            NormalB = normalB;
            NormalC = normalC;

            PerimeterEdgeAB = perimeterEdgeAB;
            PerimeterEdgeBC = perimeterEdgeBC;
            PerimeterEdgeCA = perimeterEdgeCA;

            anchorA.AddCapping(this);
            anchorB.AddCapping(this);
            anchorC.AddCapping(this);

            SurfaceTriangle = new SurfaceTriangle(new Ray3D(anchorA.Point, normalA), new Ray3D(anchorB.Point, normalB), new Ray3D(anchorC.Point, normalC));
        }

        public int Id { get; }

        public SurfaceTriangle SurfaceTriangle { get; }

        public Ray3D RayFromProjectedPoint(Point3D point)
        {
            var projection = SurfaceTriangle.Triangle.Plane.Projection(point);

            var c = SurfaceTriangle.Triangle.GetBarycentricCoordinate(projection);

            return new Ray3D(projection, (c.λ1 * NormalA + c.λ2 * NormalB + c.λ3 * NormalC).Direction);
        }

        public Vector3D NormalFromProjectedPoint(Point3D point)
        {
            var projection = SurfaceTriangle.Triangle.Plane.Projection(point);

            var c = SurfaceTriangle.Triangle.GetBarycentricCoordinate(projection);

            return (c.λ1 * NormalA + c.λ2 * NormalB + c.λ3 * NormalC).Direction;
        }

        public ElasticVertexAnchor AnchorA { get; }
        public ElasticVertexAnchor AnchorB { get; }
        public ElasticVertexAnchor AnchorC { get; }

        public Vector3D NormalA { get; }
        public Vector3D NormalB { get; }
        public Vector3D NormalC { get; }

        public ElasticEdge PerimeterEdgeAB { get; }
        public ElasticEdge PerimeterEdgeBC { get; }
        public ElasticEdge PerimeterEdgeCA { get; }
        public IEnumerable<ElasticEdge> PerimeterEdges
        {
            get
            {
                yield return PerimeterEdgeAB;
                yield return PerimeterEdgeBC;
                yield return PerimeterEdgeCA;
            }
        }

        private List<ElasticSegment> _segments = new List<ElasticSegment>();
        private ElasticVertexLink[] _perimeterSegments;
        private ElasticVertexLink[] _dividingSegments;

        public IReadOnlyList<ElasticSegment> Segments { get { return _segments; } }

        public void SetSegments(IEnumerable<ElasticSegment> segments)
        {
            _segments = new List<ElasticSegment>(segments);
            _perimeterSegments = null;
            _dividingSegments = null;
        }

        public int SegmentsCount
        {
            get { return Segments.Count; }
        }

        public int PerimeterPointsCount
        {
            get { return PerimeterEdgeAB.PerimeterPoints.Count + PerimeterEdgeBC.PerimeterPoints.Count + PerimeterEdgeCA.PerimeterPoints.Count; }
        }

        public int VertexPointsCount
        {
            get { return Segments.SelectMany(s => s.VerticiesAB).Count(v => v.Vertex.Id == AnchorA.Id || v.Vertex.Id == AnchorB.Id || v.Vertex.Id == AnchorC.Id); }
        }

        private IEnumerable<ElasticVertexLink> GetPerimeterLinks()
        {
            foreach (var segment in PerimeterEdgeAB.GetPerimeterLinks()) { yield return segment; }
            foreach (var segment in PerimeterEdgeBC.GetPerimeterLinks()) { yield return segment; }
            foreach (var segment in PerimeterEdgeCA.GetPerimeterLinks()) { yield return segment; }
        }

        private void SetPerimeterLinks()
        {
            if (_perimeterSegments is not null) { return; }
            _perimeterSegments = GetPerimeterLinks().ToArray();
        }

        private void SetDividingLinks()
        {
            if (_dividingSegments is not null) { return; }
            _dividingSegments = Segments.Select(s => new ElasticVertexLink(s.VertexA.Vertex, s.VertexB.Vertex)).ToArray();
        }

        public IEnumerable<SurfaceSegmentContainer<int>> GetDividingSurfaceSegments()
        {
            SetPerimeterLinks();
            SetDividingLinks();

            foreach (var segment in _dividingSegments)
            {
                yield return new SurfaceSegmentContainer<int>(
                    new SurfaceRayContainer<int>(RayFromProjectedPoint(segment.PointA.Point), segment.PointA.Id),
                    new SurfaceRayContainer<int>(RayFromProjectedPoint(segment.PointB.Point), segment.PointB.Id));
            }
        }

        public IEnumerable<SurfaceSegmentContainer<int>> GetPerimeterSurfaceSegments()
        {
            SetPerimeterLinks();

            foreach (var segment in _perimeterSegments)
            {
                yield return new SurfaceSegmentContainer<int>(
                    new SurfaceRayContainer<int>(RayFromProjectedPoint(segment.PointA.Point), segment.PointA.Id),
                    new SurfaceRayContainer<int>(RayFromProjectedPoint(segment.PointB.Point), segment.PointB.Id));
            }
        }

        public SurfaceSegmentSets<TriangleFillingGroup, int> CreateSurfaceSegmentSet()
        {
            return new SurfaceSegmentSets<TriangleFillingGroup, int>
            {
                NodeId = Id,
                GroupObject = new TriangleFillingGroup(SurfaceTriangle),
                DividingSegments = GetDividingSurfaceSegments().ToArray(),
                PerimeterSegments = GetPerimeterSurfaceSegments().ToArray()
            };
        }

        Dictionary<int, ElasticVertexCore> _vertexLookup;
        public Dictionary<int, ElasticVertexCore> VertexLookup
        {
            get
            {
                if (_vertexLookup is null)
                {
                    _vertexLookup = new Dictionary<int, ElasticVertexCore>();
                    _vertexLookup[AnchorA.Id] = AnchorA;
                    _vertexLookup[AnchorB.Id] = AnchorB;
                    _vertexLookup[AnchorC.Id] = AnchorC;
                    foreach (var point in PerimeterEdgeAB.PerimeterPoints)
                    {
                        _vertexLookup[point.Id] = point;
                    }
                    foreach (var point in PerimeterEdgeBC.PerimeterPoints)
                    {
                        _vertexLookup[point.Id] = point;
                    }
                    foreach (var point in PerimeterEdgeCA.PerimeterPoints)
                    {
                        _vertexLookup[point.Id] = point;
                    }
                    foreach (var segment in _segments)
                    {
                        _vertexLookup[segment.VertexA.Vertex.Id] = segment.VertexA.Vertex;
                        _vertexLookup[segment.VertexB.Vertex.Id] = segment.VertexB.Vertex;
                    }
                }
                return _vertexLookup;
            }
        }

        public void Export(IWireFrameMesh mesh)
        {
            var a = mesh.AddPointNoRow(SurfaceTriangle.A.Point, SurfaceTriangle.A.Normal);
            var b = mesh.AddPointNoRow(SurfaceTriangle.B.Point, SurfaceTriangle.B.Normal);
            var c = mesh.AddPointNoRow(SurfaceTriangle.C.Point, SurfaceTriangle.C.Normal);

            new PositionTriangle(a, b, c);

            foreach(var segment in Segments)
            {
                var aa = mesh.AddPointNoRow(segment.VertexA.Point, NormalFromProjectedPoint(segment.VertexA.Point));
                var bb = mesh.AddPointNoRow(segment.VertexB.Point, NormalFromProjectedPoint(segment.VertexB.Point));
                var mid = (segment.VertexA.Point + segment.VertexB.Point) / 2;
                var cc = mesh.AddPointNoRow(mid, NormalFromProjectedPoint(mid));
                new PositionTriangle(aa, bb, cc);
            }
        }
    }
}
