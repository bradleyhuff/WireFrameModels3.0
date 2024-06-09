
using Collections.WireFrameMesh.Basics;

namespace Operations.PositionRemovals.Interfaces
{
    internal interface IFillConditionals
    {
        public void SetPrimaryMatchingPoints(Position[] positions);
        public void SetSecondaryMatchingPoints(Position[] positions);
        public bool AllowFill(PositionNormal a, PositionNormal b, PositionNormal c);
    }
}
