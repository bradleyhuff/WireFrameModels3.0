
using Collections.WireFrameMesh.Basics;

namespace Operations.PositionRemovals.Interfaces
{
    public interface IFillConditionals
    {
        public bool AllowFill(PositionNormal a, PositionNormal b, PositionNormal c);
        public bool AllowFill(Position a, Position b, Position c);
    }
}
