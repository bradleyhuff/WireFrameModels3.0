using Collections.WireFrameMesh.Basics;
using Operations.PlanarFilling.Filling.Internals;
using Operations.PositionRemovals.Conditionals;
using Operations.PositionRemovals.Interfaces;
using Operations.SurfaceSegmentChaining.Basics;

namespace Operations.PositionRemovals.FillActions
{
    internal class CornerRemovalFill<T> : IFillAction<T>
    {
        private MatchingConditionals _conditionals = new MatchingConditionals();
        private PlanarLoop<T> _planarLoop;

        public IFillConditionals FillConditions { get { return _conditionals; } }

        public void PresetMatching(Position position, SurfaceRayContainer<PositionNormal>[] perimeterPoints)
        {
            _conditionals.SetPrimaryMatchingPoints([perimeterPoints.Single(p => p.Reference.PositionObject.Cardinality > 2).Reference.PositionObject]);
            _conditionals.SetSecondaryMatchingPoints(perimeterPoints.Select(p => p.Reference.PositionObject).ToArray());
        }

        public List<IndexSurfaceTriangle> Run(PlanarLoop<T> planarLoop)
        {
            _planarLoop = planarLoop;
            _conditionals.MatchWithPrimaryMatchingPoints();
            var result = EvaluateByMatchingPoints();
            if (result is not null) { return result; }
            _conditionals.MatchWithSecondaryMatchingPoints();
            result = EvaluateByMatchingPoints();
            if (result is null) { _planarLoop.ThrowError(); return _planarLoop.IndexedFillTriangles; }
            return result;
        }

        private List<IndexSurfaceTriangle> EvaluateByMatchingPoints()
        {
            if (_planarLoop.LoopForFillings(0, false)) return _planarLoop.IndexedFillTriangles;

            for (int i = 1; i <= _planarLoop.IndexLoop.Count / 2; i++)
            {
                if (_planarLoop.LoopForFillings(i, false)) return _planarLoop.IndexedFillTriangles;
                if (_planarLoop.LoopForFillings(-i, false)) return _planarLoop.IndexedFillTriangles;
            }
            return null;
        }
    }
}
