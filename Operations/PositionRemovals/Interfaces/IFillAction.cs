using Collections.WireFrameMesh.Basics;
using Operations.PlanarFilling.Filling.Internals;
using Operations.SurfaceSegmentChaining.Basics;

namespace Operations.PositionRemovals.Interfaces
{
    internal interface IFillAction<T>
    {
        public IFillConditionals FillConditions { get; }
        public void PresetMatching(Position position, SurfaceRayContainer<PositionNormal>[] perimeterPoints);
        public List<IndexSurfaceTriangle> Run(PlanarLoop<T> planarFilling);
    }
}
