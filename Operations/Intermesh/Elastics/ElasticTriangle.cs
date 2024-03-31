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
        public ElasticTriangle(IntermeshTriangle triangle, ElasticVertexAnchor anchorA, Vector3D normalA, ElasticVertexAnchor anchorB, Vector3D normalB, ElasticVertexAnchor anchorC, Vector3D normalC,
            ElasticEdge perimeterEdgeAB, ElasticEdge perimeterEdgeBC, ElasticEdge perimeterEdgeCA, string trace)
        {
            Id = _id++;

            Triangle = triangle;

            AnchorA = anchorA;
            AnchorB = anchorB;
            AnchorC = anchorC;

            NormalA = normalA;
            NormalB = normalB;
            NormalC = normalC;

            PerimeterEdgeAB = perimeterEdgeAB;
            PerimeterEdgeBC = perimeterEdgeBC;
            PerimeterEdgeCA = perimeterEdgeCA;

            Trace = trace;

            anchorA.AddCapping(this);
            anchorB.AddCapping(this);
            anchorC.AddCapping(this);

            SurfaceTriangle = new SurfaceTriangle(new Ray3D(anchorA.Point, normalA), new Ray3D(anchorB.Point, normalB), new Ray3D(anchorC.Point, normalC));
        }

        public int Id { get; }
        public string Trace { get; }

        public IntermeshTriangle Triangle { get; }

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

        public IReadOnlyList<ElasticTriangle> ABadjacents { get; private set; }
        public IReadOnlyList<ElasticTriangle> BCadjacents { get; private set; }
        public IReadOnlyList<ElasticTriangle> CAadjacents { get; private set; }

        private List<ElasticSegment> _segments = new List<ElasticSegment>();
        private ElasticVertexLink[] _perimeterLinks;
        private ElasticVertexLink[] _dividingSegments;

        public IReadOnlyList<ElasticSegment> Segments { get { return _segments; } }

        public void SetSegments(IEnumerable<ElasticSegment> segments)
        {
            _segments = new List<ElasticSegment>(segments);
            _perimeterLinks = null;
            _dividingSegments = null;
        }

        public void SetAdjacents(IEnumerable<ElasticTriangle> abAdjacents, IEnumerable<ElasticTriangle> bcAdjacents, IEnumerable<ElasticTriangle> caAdjacents)
        {
            ABadjacents = abAdjacents.ToList();
            BCadjacents = bcAdjacents.ToList();
            CAadjacents = caAdjacents.ToList();
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

        private IEnumerable<ElasticSegment> GetPerimeterSegments()
        {
            foreach (var segment in PerimeterEdgeAB.Segments) { yield return segment; }
            foreach (var segment in PerimeterEdgeBC.Segments) { yield return segment; }
            foreach (var segment in PerimeterEdgeCA.Segments) { yield return segment; }
        }

        private void SetPerimeterLinks()
        {
            _perimeterLinks = GetPerimeterLinks().ToArray();
        }

        private void SetDividingLinks()
        {
            var perimeterSegments = GetPerimeterSegments().ToArray();
            _dividingSegments = Segments.Where(s => !perimeterSegments.Any(p => p.Id == s.Id)).Select(s => new ElasticVertexLink(s.VertexA.Vertex, s.VertexB.Vertex)).ToArray();
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

            foreach (var segment in _perimeterLinks)
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

        public void ExportWithSegments(IWireFrameMesh mesh)
        {
            var a = mesh.AddPointNoRow(SurfaceTriangle.A.Point, SurfaceTriangle.A.Normal);
            var b = mesh.AddPointNoRow(SurfaceTriangle.B.Point, SurfaceTriangle.B.Normal);
            var c = mesh.AddPointNoRow(SurfaceTriangle.C.Point, SurfaceTriangle.C.Normal);

            new PositionTriangle(a, b, c);

            foreach(var segment in Segments)
            {
                var normalA = NormalFromProjectedPoint(segment.VertexA.Point);
                var normalB = NormalFromProjectedPoint(segment.VertexB.Point);
                var height = 2e-4;

                mesh.AddPoint(segment.VertexA.Point + -height * normalA, normalA);
                mesh.AddPoint(segment.VertexB.Point + -height * normalB, normalB);
                mesh.EndRow();

                mesh.AddPoint(segment.VertexA.Point, normalA);
                mesh.AddPoint(segment.VertexB.Point, normalB);
                mesh.EndRow();

                mesh.AddPoint(segment.VertexA.Point + height * normalA, normalA);
                mesh.AddPoint(segment.VertexB.Point + height * normalB, normalB);
                mesh.EndRow();
                mesh.EndGrid();
            }
        }
    }
}
