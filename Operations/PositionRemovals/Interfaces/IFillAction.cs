using Operations.PlanarFilling.Filling.Internals;

namespace Operations.PositionRemovals.Interfaces
{
    internal interface IFillAction<T>
    {
        public IFillConditionals FillConditions { get; }
        public List<IndexSurfaceTriangle> Run(PlanarLoop<T> planarFilling);
    }
}
