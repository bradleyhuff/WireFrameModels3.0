using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Interfaces;

namespace Operations.SurfaceSegmentChaining.Chaining.Diagnostics
{
    internal class SpurLoopChainingException<G, T> : InvalidOperationException where G: class
    {
        public SpurLoopChainingException(string message, List<LinkedIndexSurfaceSegment<G, T>> spurConnectingSegments, ISurfaceSegmentChaining<G, T> chain) : base(message)
        {
            Chain = chain;
            SpurConnectingSegments = spurConnectingSegments;
        }

        public ISurfaceSegmentChaining<G, T> Chain { get; set; }
        public IReadOnlyList<LinkedIndexSurfaceSegment<G, T>> SpurConnectingSegments { get; }
    }
}
