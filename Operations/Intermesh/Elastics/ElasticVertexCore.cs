using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.Buckets.Interfaces;

namespace Operations.Intermesh.Elastics
{
    internal class ElasticVertexCore : IBox
    {
        private static int _id = 0;
        protected ElasticVertexCore(Point3D point)
        {
            Id = _id++;
            Point = point;
        }

        public int Id { get; }

        public Point3D Point { get; internal set; }

        private List<ElasticVertexContainer> _segmentContainers = new List<ElasticVertexContainer>();
        public IReadOnlyList<ElasticVertexContainer> SegmentContainers { get { return _segmentContainers; } }

        private Rectangle3D _box;
        public Rectangle3D Box
        {
            get
            {
                if (_box is null)
                {
                    _box = new Rectangle3D(Point, BoxBucket.MARGINS);
                }
                return _box;
            }
        }

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

        public static ElasticVertexCore Create(Point3D point, IBoxBucket<ElasticVertexAnchor> anchors)
        {
            var fetch = anchors.Fetch(point.Margin(BoxBucket.MARGINS)).SingleOrDefault(p => p.Point == point);
            if (fetch is not null) { return fetch; }
            return new ElasticVertexCore(point);
        }
    }
}
