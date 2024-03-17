using BasicObjects.GeometricObjects;

namespace Operations.Intermesh.Elastics
{
    internal class ElasticVertexLink
    {
        private static int _id = 0;
        public ElasticVertexLink(ElasticVertexCore pointA, ElasticVertexCore pointB)
        {
            Id = _id++;
            PointA = pointA;
            PointB = pointB;
        }

        public int Id { get; }

        public ElasticVertexCore PointA { get; }
        public ElasticVertexCore PointB { get; }

        public LineSegment3D Segment
        {
            get { return new LineSegment3D(PointA.Point, PointB.Point); }
        }
    }
}
