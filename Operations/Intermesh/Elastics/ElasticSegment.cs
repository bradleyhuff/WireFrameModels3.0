using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;

namespace Operations.Intermesh.Elastics
{
    internal class ElasticSegment
    {
        private static int _id = 0;
        public ElasticSegment(ElasticVertexContainer vertexA, ElasticVertexContainer vertexB)
        {
            Id = _id++;
            VertexA = vertexA;
            VertexB = vertexB;
        }

        public int Id { get; }

        public ElasticVertexContainer VertexA { get; }
        public ElasticVertexContainer VertexB { get; }

        public Combination2 PositionKey { get { return new Combination2(VertexA.Vertex.Id, VertexB.Vertex.Id); } }
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
