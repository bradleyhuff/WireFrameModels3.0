
using Operations.SurfaceSegmentChaining.Basics;

namespace Operations.SurfaceSegmentChaining.Interfaces
{
    internal interface ISurfaceSegmentChaining<G, T>
    {
        IReadOnlyList<SurfaceRayContainer<T>> ReferenceArray { get; }
        ProtectedIndexedLoops ProtectedIndexedLoops { get; }
        List<int> PerimeterLoopGroupKeys { get; }
        IReadOnlyList<int> LoopGroupKeys { get; }
        IReadOnlyList<int> SpurredLoopGroupKeys { get; }
        IReadOnlyList<int> SpurGroupKeys { get; }
        List<G> PerimeterLoopGroupObjects { get; }
        IReadOnlyList<G> LoopGroupObjects { get; }
        IReadOnlyList<G> SpurredLoopGroupObjects { get; }
        IReadOnlyList<G> SpurGroupObjects { get; }
        IReadOnlyList<SurfaceRayContainer<T>[]> PerimeterLoops { get; }
        IReadOnlyList<SurfaceRayContainer<T>[]> Loops { get; }
        IReadOnlyList<SurfaceRayContainer<T>[]> SpurredLoops { get; }
        IReadOnlyList<SurfaceRayContainer<T>[]> Spurs { get; }
    }
}
