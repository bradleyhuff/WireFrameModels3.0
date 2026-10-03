
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Basics.Abstractions;

namespace Operations.SurfaceSegmentChaining.Interfaces
{
    public interface ISurfaceSegmentChaining<G, T> where G: LoopGroupObjects
    {
        IReadOnlyList<SurfaceRayContainer<T>> ReferenceArray { get; }
        IReadOnlyDictionary<int, int> BackReference { get; }
        ProtectedIndexedLoops ProtectedIndexedLoops { get; }
        IReadOnlyList<int> PerimeterLoopGroupKeys { get; }
        IReadOnlyList<int> DividingLoopGroupKeys { get; }
        IReadOnlyList<int> SpurredLoopGroupKeys { get; }
        IReadOnlyList<int> SpurGroupKeys { get; }
        IReadOnlyList<G> PerimeterLoopGroupObjects { get; }
        IReadOnlyList<G> DividingLoopGroupObjects { get; }
        IReadOnlyList<G> SpurredLoopGroupObjects { get; }
        IReadOnlyList<G> SpurGroupObjects { get; }
        IReadOnlyList<SurfaceRayContainer<T>[]> PerimeterLoops { get; }
        IReadOnlyList<SurfaceRayContainer<T>[]> DividingLoops { get; }
        IReadOnlyList<SurfaceRayContainer<T>[]> SpurredLoops { get; }
        IReadOnlyList<SurfaceRayContainer<T>[]> Spurs { get; }
    }
}
