using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        //private ElasticVertexLink[] _perimeterSegments;
        //private ElasticVertexLink[] _dividingSegments;
        //private Combination2Dictionary<bool> _perimeterKeys;

        public IReadOnlyList<ElasticSegment> Segments { get { return _segments; } }

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
        public void SetSegments(IEnumerable<ElasticSegment> segments)
        {
            _segments = new List<ElasticSegment>(segments);
            //_perimeterSegments = null;
            //_dividingSegments = null;
            _vertexLookup = null;
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

        //private IEnumerable<ElasticVertexLink> GetPerimeterLinks()
        //{
        //    foreach (var segment in PerimeterEdgeAB.GetPerimeterLinks()) { yield return segment; }
        //    foreach (var segment in PerimeterEdgeBC.GetPerimeterLinks()) { yield return segment; }
        //    foreach (var segment in PerimeterEdgeCA.GetPerimeterLinks()) { yield return segment; }
        //}

        //private void SetPerimeterLinks()
        //{
        //    if (_perimeterSegments is not null) { return; }
        //    _perimeterSegments = GetPerimeterLinks().ToArray();
        //    _perimeterKeys = new Combination2Dictionary<bool>();
        //    foreach (var segment in _perimeterSegments)
        //    {
        //        var key = new Combination2(segment.PointA.Id, segment.PointB.Id);
        //        _perimeterKeys[key] = true;
        //    }
        //}

        //private void SetDividingLinks()
        //{
        //    if (_dividingSegments is not null) { return; }
        //    _dividingSegments = Segments.Where(s =>
        //        !_perimeterKeys.ContainsKey(new Combination2(s.VertexA.Vertex.Id, s.VertexB.Vertex.Id))
        //        ).Select(s => new ElasticVertexLink(s.VertexA.Vertex, s.VertexB.Vertex)).ToArray();
        //}

        //public IEnumerable<SurfaceSegmentNode<IndexTag>> GetDividingSurfaceSegments()
        //{
        //    SetPerimeterLinks();
        //    SetDividingLinks();

        //    foreach (var segment in _dividingSegments)
        //    {
        //        yield return new SurfaceSegmentNode<IndexTag>(
        //            new SurfaceRayNode<IndexTag>(RayFromProjectedPoint(segment.PointA.Point), new IndexTag(segment.PointA.Id)),
        //            new SurfaceRayNode<IndexTag>(RayFromProjectedPoint(segment.PointB.Point), new IndexTag(segment.PointB.Id)));
        //    }
        //}

        //public IEnumerable<SurfaceSegmentNode<IndexTag>> GetPerimeterSurfaceSegments()
        //{
        //    SetPerimeterLinks();

        //    foreach (var segment in _perimeterSegments)
        //    {
        //        yield return new SurfaceSegmentNode<IndexTag>(
        //            new SurfaceRayNode<IndexTag>(RayFromProjectedPoint(segment.PointA.Point), new IndexTag(segment.PointA.Id)),
        //            new SurfaceRayNode<IndexTag>(RayFromProjectedPoint(segment.PointB.Point), new IndexTag(segment.PointB.Id)));
        //    }
        //}
    }
}
