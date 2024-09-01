using Collections.WireFrameMesh.Basics;
using Operations.PlanarFilling.Basics;
using Operations.PositionRemovals.Interfaces;
using Operations.SurfaceSegmentChaining.Interfaces;

namespace Operations.PositionRemovals.Conditionals
{
    internal class MatchingConditionals : IFillConditionals
    {
        private Position[] _matchingPoints = null;
        private Position[] _primaryMatchingPoints = null;
        private Position[] _secondaryMatchingPoints = null;

        public void SetPrimaryMatchingPoints(Position[] positions)
        {
            _primaryMatchingPoints = positions;
        }

        public void SetSecondaryMatchingPoints(Position[] positions)
        {
            _secondaryMatchingPoints = positions;
        }

        public void MatchWithPrimaryMatchingPoints()
        {
            _matchingPoints = _primaryMatchingPoints;
        }

        public void MatchWithSecondaryMatchingPoints()
        {
            _matchingPoints = _secondaryMatchingPoints;
        }

        public bool AllowFill(PositionNormal a, PositionNormal b, PositionNormal c)
        {
            if(_matchingPoints is null) { return true; }
            if (_matchingPoints.Any(m => a.PositionObject.Id == m.Id)) { return true; }
            if (_matchingPoints.Any(m => b.PositionObject.Id == m.Id)) { return true; }
            if (_matchingPoints.Any(m => c.PositionObject.Id == m.Id)) { return true; }
            return false;
        }

        public bool AllowFill(Position a, Position b, Position c)
        {
            if (_matchingPoints is null) { return true; }
            if (_matchingPoints.Any(m => a.Id == m.Id)) { return true; }
            if (_matchingPoints.Any(m => b.Id == m.Id)) { return true; }
            if (_matchingPoints.Any(m => c.Id == m.Id)) { return true; }
            return false;
        }
    }
}
