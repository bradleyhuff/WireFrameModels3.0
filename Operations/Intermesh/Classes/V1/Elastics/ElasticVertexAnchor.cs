using BasicObjects.GeometricObjects;

namespace Operations.Intermesh.Classes.V1.Elastics
{
    internal class ElasticVertexAnchor : ElasticVertexCore
    {
        public ElasticVertexAnchor(Point3D point) : base(point) { }

        private List<ElasticTriangle> _capping = new List<ElasticTriangle>();

        internal void AddCapping(ElasticTriangle triangle)
        {
            _capping.Add(triangle);
        }

        public IReadOnlyList<ElasticTriangle> Capping { get { return _capping; } }
    }
}
