using BasicObjects.GeometricObjects;

namespace Operations.Intermesh.Elastics
{
    internal class ElasticVertexCore
    {
        private static int _id = 0;
        public ElasticVertexCore(Point3D point)
        {
            Id = _id++;
            Point = point;
        }

        public int Id { get; }

        public Point3D Point { get; internal set; }

        private List<ElasticVertexContainer> _segmentContainers = new List<ElasticVertexContainer>();
        public IReadOnlyList<ElasticVertexContainer> SegmentContainers { get { return _segmentContainers; } }

        public void Link(ElasticVertexContainer container)
        {
            container.Vertex = this;
            if (_segmentContainers.Any(c => c.Id == container.Vertex.Id)) { return; }
            _segmentContainers.Add(container);
        }

        public void Delink(ElasticVertexContainer container)
        {
            _segmentContainers.RemoveAll(c => c.Vertex.Id == container.Vertex.Id);
            container.Vertex = null;
        }

        public void Link(ElasticVertexCore vertex)
        {
            var containers = vertex.SegmentContainers.ToArray();
            foreach (var container in containers) { vertex.Delink(container); }
            foreach (var container in containers) { Link(container); }
        }
    }
}
