using BasicObjects.GeometricObjects;
using Collections.Buckets.Interfaces;

namespace Operations.PlanarFilling.Filling.Interfaces
{
    internal interface IFillingLoop : IBox
    {
        List<int> IndexLoop { get; }
        public IReadOnlyList<Point3D> ProjectedLoopPoints { get; }
        public IReadOnlyCollection<InternalPlanarSegment> LoopSegments { get; }
        public List<IFillingLoop> InternalLoops { get; set; }
        public bool OutLineContainsLoop(IFillingLoop input);
        public IReadOnlyList<IndexSurfaceTriangle> FillTriangles { get; }
        bool EnclosedByTriangle(Triangle3D triangle);
    }
}
