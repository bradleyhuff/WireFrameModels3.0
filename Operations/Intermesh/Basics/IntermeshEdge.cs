using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshEdge
    {
        private static int _id = 0;
        internal IntermeshEdge(PositionNormal a, PositionNormal b, IEnumerable<IntermeshTriangle> adjacents)
        {
            A = a;
            B = b;
            _adjacents = adjacents;
            Id = _id++;
        }

        private IEnumerable<IntermeshTriangle> _adjacents;

        public int Id { get; }
        public PositionNormal A { get; private set; }
        public PositionNormal B { get; private set; }

        public bool IsDegenerate
        {
            get
            {
                return A.Id == B.Id;
            }
        }

        public IEnumerable<PositionNormal> Positions
        {
            get
            {
                yield return A;
                yield return B;
            }
        }

        private LineSegment3D _segment = null;
        public LineSegment3D Segment
        {
            get
            {
                if (_segment is null)
                {
                    _segment = new LineSegment3D(
                        A.Position,
                        B.Position);
                }
                return _segment;
            }
        }

        private SurfaceLineSegment _surfaceLineSegment = null;
        public SurfaceLineSegment SurfaceLineSegment
        {
            get
            {
                if (_surfaceLineSegment is null)
                {
                    _surfaceLineSegment = new SurfaceLineSegment(
                        new Ray3D(A.Position, A.Normal),
                        new Ray3D(B.Position, B.Normal));
                }
                return _surfaceLineSegment;
            }
        }

        public IEnumerable<Point3D> Points
        {
            get
            {
                if (A.Position is not null) yield return A.Position;
                if (B.Position is not null) yield return B.Position;
            }
        }

        public bool HasPosition(Position position)
        {
            return A.PositionObject?.Id == position.Id || B.PositionObject?.Id == position.Id;
        }

        internal IEnumerable<IntersectionVertexContainer> GetPerimeterPoints()
        {
            var linkedPerimeterPoints = GetLinkedPerimeterPointsIterate().ToArray();
            foreach (var point in linkedPerimeterPoints.DistinctBy(p => p.Vertex.Id)) { yield return point; }
        }

        private IEnumerable<IntersectionVertexContainer> GetLinkedPerimeterPointsIterate()
        {
            if (_adjacents.Count() < 2) { yield break; }
            var points = _adjacents.SelectMany(t => t.GetIntersectionPoints()).DistinctBy(v => v.Id).ToArray();

            foreach (var point in points.Where(p => p.Vertex is not null))
            {
                var intersections = point.Vertex.IntersectionContainers.Select(c => c.Intersection);

                var allAdjacents = intersections.SelectMany(i => i.Intersectors).DistinctBy(t => t.Id);

                var qualifyingAdjacents = _adjacents.Intersect(allAdjacents).DistinctBy(t => t.Id);
                if (qualifyingAdjacents.Count() > 1) { yield return point; }
            }
        }
    }
}
