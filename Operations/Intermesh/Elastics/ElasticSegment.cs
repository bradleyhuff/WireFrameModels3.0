using BasicObjects.GeometricObjects;

namespace Operations.Intermesh.Elastics
{
    internal class ElasticSegment
    {
        private static int _id = 0;
        public ElasticSegment(Point3D pointA, Point3D pointB)
        {
            Id = _id++;
            _pointA = pointA;
            _pointB = pointB;
        }

        public int Id { get; }

        private Point3D _pointA;
        private Point3D _pointB;
        private ElasticVertexContainer _vertexA;
        private ElasticVertexContainer _vertexB;

        public ElasticVertexContainer VertexA
        {
            get
            {
                if (_vertexA is null) { _vertexA = new ElasticVertexContainer(this, _pointA, 'a'); }
                return _vertexA;
            }
        }
        public ElasticVertexContainer VertexB
        {
            get
            {
                if (_vertexB is null) { _vertexB = new ElasticVertexContainer(this, _pointB, 'b'); }
                return _vertexB;
            }
        }
        internal IEnumerable<ElasticVertexContainer> VerticiesAB
        {
            get
            {
                yield return VertexA;
                yield return VertexB;
            }
        }

        public LineSegment3D Segment
        {
            get
            {
                return new LineSegment3D(VertexA.Point, VertexB.Point);
            }
        }

        public double Length
        {
            get { return Point3D.Distance(VertexA.Point, VertexB.Point); }
        }
    }
}
