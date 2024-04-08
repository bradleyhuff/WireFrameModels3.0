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

        internal void SetPerimeterPoints(IEnumerable<ElasticVertexCore> perimeterPoints, IEnumerable<ElasticSegment> perimeterSegments)
        {
            Segments = perimeterSegments.ToList();
            var allPoints = perimeterPoints.Concat(perimeterSegments.SelectMany(p => p.VerticiesAB).Select(v => v.Vertex)).DistinctBy(v => v.Id);
            PerimeterPoints = allPoints.OrderBy(p => Point3D.Distance(p.Point, AnchorA.Point)).ToList();
        }
        internal void AddPerimeterPoints(IEnumerable<ElasticVertexCore> input)
        {
            var allPoints = input.Concat(PerimeterPoints).DistinctBy(v => v.Id);
            PerimeterPoints = allPoints.OrderBy(p => Point3D.Distance(p.Point, AnchorA.Point)).ToList();
        }
        internal void RemovePerimeterPoint(ElasticVertexCore perimeterPoint)
        {
            PerimeterPoints = PerimeterPoints.Where(p => p.Id != perimeterPoint.Id).ToList();
        }

        internal void ReplacePerimeterPoint(ElasticVertexCore perimeterPoint, ElasticVertexCore newPoint)
        {
            var points = PerimeterPoints.ToList();
            var index = points.IndexOf(perimeterPoint);
            if (index == -1) { return; }
            points[index] = newPoint;
            PerimeterPoints = points;
        }

        public IReadOnlyList<ElasticVertexCore> PerimeterPoints { get; private set; } = new List<ElasticVertexCore>();
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
