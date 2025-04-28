using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;

namespace Operations.Intermesh.Classes.V1.Elastics
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

        private Combination2 _key;
        private bool _keyIsAssigned = false;
        public Combination2 Key
        {
            get
            {
                if (!_keyIsAssigned)
                {
                    _key = new Combination2(PointA.Id, PointB.Id);
                    _keyIsAssigned = true;
                }
                return _key;
            }
        }

        public LineSegment3D Segment
        {
            get { return new LineSegment3D(PointA.Point, PointB.Point); }
        }
    }
}
