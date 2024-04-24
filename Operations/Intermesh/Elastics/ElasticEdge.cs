using BasicObjects.GeometricObjects;

namespace Operations.Intermesh.Elastics
{
    internal class ElasticEdge
    {
        private static int _id = 0;
        public ElasticEdge(ElasticVertexAnchor anchorA, ElasticVertexAnchor anchorB)
        {
            Id = _id++;
            AnchorA = anchorA;
            AnchorB = anchorB;
        }

        public int Id { get; }

        public ElasticVertexAnchor AnchorA { get; }
        public ElasticVertexAnchor AnchorB { get; }

        internal void AddPerimeterPoints(IEnumerable<ElasticVertexCore> perimeterPoints, IEnumerable<ElasticSegment> perimeterSegments)
        {
            Segments = Segments.Concat(perimeterSegments).ToList();
            var allPoints = PerimeterPoints.Concat(perimeterPoints.Concat(perimeterSegments.SelectMany(p => p.VerticiesAB).Select(v => v.Vertex))).DistinctBy(v => v.Id);
            _perimeterPoints = allPoints.OrderBy(p => Point3D.Distance(p.Point, AnchorA.Point)).ToList();
        }

        internal void AddPerimeterPoints(IEnumerable<ElasticVertexCore> input)
        {
            if (!input.Any()) { return; }
            var allPoints = input.Concat(PerimeterPoints).DistinctBy(v => v.Id);
            _perimeterPoints = allPoints.OrderBy(p => Point3D.Distance(p.Point, AnchorA.Point)).ToList();
        }
        internal void RemovePerimeterPoint(ElasticVertexCore perimeterPoint)
        {
            _perimeterPoints = PerimeterPoints.Where(p => p.Id != perimeterPoint.Id).ToList();
        }

        internal void ReplacePerimeterPoint(ElasticVertexCore perimeterPoint, ElasticVertexCore newPoint)
        {
            var points = PerimeterPoints.ToList();
            var index = points.IndexOf(perimeterPoint);
            if (index == -1) { return; }
            points[index] = newPoint;
            _perimeterPoints = points;
        }
        private List<ElasticVertexCore> _perimeterPoints = new List<ElasticVertexCore>();
        public IReadOnlyList<ElasticVertexCore> PerimeterPoints { get { return _perimeterPoints; } }
        public IReadOnlyList<ElasticSegment> Segments { get; private set; } = new List<ElasticSegment>();

        public IEnumerable<ElasticVertexLink> GetPerimeterLinks()
        {
            ElasticVertexCore lastPoint = AnchorA;
            foreach (var point in PerimeterPoints)
            {
                yield return new ElasticVertexLink(
                    lastPoint,
                    point
                    );
                lastPoint = point;
            }

            yield return new ElasticVertexLink(lastPoint, AnchorB);
        }
    }
}
