using BasicObjects.GeometricObjects;
using Collections.Buckets.Interfaces;
using Collections.WireFrameMesh.Basics;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshEdge : IBox
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

        private Rectangle3D _box = null;

        public Rectangle3D? Box
        {
            get
            {
                if (_box is null)
                {
                    _box = Rectangle3D.Containing(A.Box, B.Box);
                }
                return _box;
            }
        }

        public bool HasPosition(Position position)
        {
            return A.PositionObject?.Id == position.Id || B.PositionObject?.Id == position.Id;
        }

        internal IEnumerable<IntersectionVertexContainer> GetPerimeterPoints()
        {
            foreach (var point in GetPerimeterPointsFreePoints().DistinctBy(p => p.Vertex.Id)) { yield return point; }
            foreach (var point in GetPerimeterPointsLinkedPoints().DistinctBy(p => p.Vertex.Id)) { yield return point; }
        }
        private IEnumerable<IntersectionVertexContainer> GetPerimeterPointsLinkedPoints()
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

        private IEnumerable<IntersectionVertexContainer> GetPerimeterPointsFreePoints()
        {
            foreach (var triangle in _adjacents)
            {
                var points = triangle.GetIntersectionPoints().DistinctBy(v => v.Id);
                foreach (var point in points.Where(p => p.Vertex is not null && p.Vertex.DivisionContainers.Count() < 2))
                {
                    var distance = Point3D.Distance(point.Point, Segment.LineExtension.Projection(point.Point));
                    if (Point3D.Distance(point.Point, Segment.Projection(point.Point)) < 5e-9) { yield return point; }
                }
            }
        }
    }
}
