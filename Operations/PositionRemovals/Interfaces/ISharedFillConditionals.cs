
using Collections.WireFrameMesh.Basics;

namespace Operations.PositionRemovals.Interfaces
{
    public interface ISharedFillConditionals
    {
        public bool AllowFill(PositionNormal a, PositionNormal b, PositionNormal c);
    }
}
