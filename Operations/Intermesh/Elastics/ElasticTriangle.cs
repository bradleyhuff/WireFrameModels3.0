using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Interfaces;
using Operations.Intermesh.Basics;
using Operations.PlanarFilling.Basics;
using Operations.SurfaceSegmentChaining.Basics;

namespace Operations.Intermesh.Elastics
{
    internal class ElasticTriangle
    {
        private static int _id = 0;
        public ElasticTriangle(IntermeshTriangle triangle, ElasticVertexAnchor anchorA, Vector3D normalA, ElasticVertexAnchor anchorB, Vector3D normalB, ElasticVertexAnchor anchorC, Vector3D normalC,
            ElasticEdge perimeterEdgeAB, ElasticEdge perimeterEdgeBC, ElasticEdge perimeterEdgeCA, string trace, int tag)
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
            Tag = tag;

            anchorA.AddCapping(this);
            anchorB.AddCapping(this);
            anchorC.AddCapping(this);

            SurfaceTriangle = new SurfaceTriangle(new Ray3D(anchorA.Point, normalA), new Ray3D(anchorB.Point, normalB), new Ray3D(anchorC.Point, normalC));
        }

        public int Id { get; }
        public string Trace { get; }
        public int Tag { get; }

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

        public bool IsDegenerate
        {
            get
            {
                return AnchorA.Id == AnchorB.Id || AnchorB.Id == AnchorC.Id || AnchorC.Id == AnchorA.Id;
            }
        }

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

        internal void RemoveSegments(IEnumerable<ElasticSegment> segments)
        {
            PerimeterEdgeAB.RemovePerimeterSegments(segments);
            PerimeterEdgeBC.RemovePerimeterSegments(segments);
            PerimeterEdgeCA.RemovePerimeterSegments(segments);
            foreach (var segment in segments)
            {
                _segments.Remove(segment);
            }

        }

        public void SetAdjacents(IEnumerable<ElasticTriangle> abAdjacents, IEnumerable<ElasticTriangle> bcAdjacents, IEnumerable<ElasticTriangle> caAdjacents)
        {
            ABadjacents = abAdjacents.ToList();
            BCadjacents = bcAdjacents.ToList();
            CAadjacents = caAdjacents.ToList();
        }

        public int SegmentsCount
        {
            get { return Segments.Count(); }
        }

        public int PerimeterPointsCount
        {
            get { return PerimeterEdgeAB.PerimeterPoints.Count() + PerimeterEdgeBC.PerimeterPoints.Count() + PerimeterEdgeCA.PerimeterPoints.Count(); }
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
            var groups = GetPerimeterLinks().GroupBy(x => x.Key, new Combination2Comparer());
            _perimeterLinks = groups.Where(g => g.Count() == 1).Select(g => g.First()).ToArray();// Remove pinched perimeter segments
        }

        private void SetDividingLinks()
        {
            var perimeterSegments = GetPerimeterSegments().ToArray();

            var dividingSegments = new List<ElasticVertexLink>();
            foreach (var segment in Segments.Where(s => !perimeterSegments.Any(p => p.Id == s.Id)))
            {
                var proximityA = GetNearestAnchorInProximity(segment.VertexA);
                var proximityB = GetNearestAnchorInProximity(segment.VertexB);

                if (proximityA is not null && proximityB is not null) { continue; }

                if (proximityA is null && proximityB is not null)
                {
                    dividingSegments.Add(new ElasticVertexLink(segment.VertexA.Vertex, proximityB));
                    continue;
                }

                if (proximityA is not null && proximityB is null)
                {
                    dividingSegments.Add(new ElasticVertexLink(proximityA, segment.VertexB.Vertex));
                    continue;
                }

                dividingSegments.Add(new ElasticVertexLink(segment.VertexA.Vertex, segment.VertexB.Vertex));
            }

            _dividingSegments = dividingSegments.ToArray();
        }

        private ElasticVertexAnchor GetNearestAnchorInProximity(ElasticVertexContainer container)
        {
            var distanceA = Point3D.Distance(container.Point, AnchorA.Point);
            var distanceB = Point3D.Distance(container.Point, AnchorB.Point);
            var distanceC = Point3D.Distance(container.Point, AnchorC.Point);

            if (distanceA < distanceB && distanceA < distanceC && distanceA < GapConstants.Proximity) { return AnchorA; }
            if (distanceB < distanceA && distanceB < distanceC && distanceB < GapConstants.Proximity) { return AnchorB; }
            if (distanceC < distanceA && distanceC < distanceB && distanceC < GapConstants.Proximity) { return AnchorC; }
            return null;
        }

        public IEnumerable<SurfaceSegmentContainer<ElasticVertexCore>> GetDividingSurfaceSegments()
        {
            SetPerimeterLinks();
            SetDividingLinks();

            foreach (var segment in _dividingSegments)
            {
                yield return new SurfaceSegmentContainer<ElasticVertexCore>(
                    new SurfaceRayContainer<ElasticVertexCore>(RayFromProjectedPoint(segment.PointA.Point), Triangle.Triangle.Normal, segment.PointA.Id, segment.PointA),
                    new SurfaceRayContainer<ElasticVertexCore>(RayFromProjectedPoint(segment.PointB.Point), Triangle.Triangle.Normal, segment.PointB.Id, segment.PointB));
            }
        }

        public IEnumerable<SurfaceSegmentContainer<ElasticVertexCore>> GetPerimeterSurfaceSegments()
        {
            SetPerimeterLinks();

            foreach (var segment in _perimeterLinks)
            {
                yield return new SurfaceSegmentContainer<ElasticVertexCore>(
                    new SurfaceRayContainer<ElasticVertexCore>(RayFromProjectedPoint(segment.PointA.Point), Triangle.Triangle.Normal, segment.PointA.Id, segment.PointA),
                    new SurfaceRayContainer<ElasticVertexCore>(RayFromProjectedPoint(segment.PointB.Point), Triangle.Triangle.Normal, segment.PointB.Id, segment.PointB));
            }
        }

        public SurfaceSegmentSets<PlanarFillingGroup, ElasticVertexCore> CreateSurfaceSegmentSet()
        {
            return new SurfaceSegmentSets<PlanarFillingGroup, ElasticVertexCore>
            {
                NodeId = Id,
                GroupObject = new PlanarFillingGroup(SurfaceTriangle.Triangle.Plane, SurfaceTriangle.Triangle.Box.Diagonal),
                DividingSegments = GetDividingSurfaceSegments().ToArray(),
                PerimeterSegments = GetPerimeterSurfaceSegments().ToArray()
            };
        }

        public void ExportWithPerimeters(IWireFrameMesh mesh, double height = 5e-5)
        {
            mesh.AddTriangle(SurfaceTriangle.A.Point, SurfaceTriangle.A.Normal, SurfaceTriangle.B.Point, SurfaceTriangle.B.Normal, SurfaceTriangle.C.Point, SurfaceTriangle.C.Normal, "", 0);

            foreach (var segment in GetPerimeterSurfaceSegments())
            {
                var normalA = SurfaceTriangle.Triangle.Normal;
                var normalB = SurfaceTriangle.Triangle.Normal;

                mesh.AddPoint(segment.A.Point + -height * normalA, normalA);
                mesh.AddPoint(segment.B.Point + -height * normalB, normalB);
                mesh.EndRow();

                mesh.AddPoint(segment.A.Point, normalA);
                mesh.AddPoint(segment.B.Point, normalB);
                mesh.EndRow();

                mesh.AddPoint(segment.A.Point + height * normalA, normalA);
                mesh.AddPoint(segment.B.Point + height * normalB, normalB);
                mesh.EndRow();
                mesh.EndGrid();
            }
        }

        public void ExportWithDivisions(IWireFrameMesh mesh, double height = 5e-5)
        {
            mesh.AddTriangle(SurfaceTriangle.A.Point, SurfaceTriangle.A.Normal, SurfaceTriangle.B.Point, SurfaceTriangle.B.Normal, SurfaceTriangle.C.Point, SurfaceTriangle.C.Normal, "", 0);

            var dividingSegments = GetDividingSurfaceSegments();
            foreach (var segment in dividingSegments)
            {
                var normalA = SurfaceTriangle.Triangle.Normal;
                var normalB = SurfaceTriangle.Triangle.Normal;

                mesh.AddPoint(segment.A.Point + -height * normalA, normalA);
                mesh.AddPoint(segment.B.Point + -height * normalB, normalB);
                mesh.EndRow();

                mesh.AddPoint(segment.A.Point, normalA);
                mesh.AddPoint(segment.B.Point, normalB);
                mesh.EndRow();

                mesh.AddPoint(segment.A.Point + height * normalA, normalA);
                mesh.AddPoint(segment.B.Point + height * normalB, normalB);
                mesh.EndRow();
                mesh.EndGrid();
            }
        }
    }
}
