﻿using Collections.WireFrameMesh.Basics;
using Operations.PlanarFilling.Filling.Internals;
using Operations.PositionRemovals.Conditionals;
using Operations.PositionRemovals.Interfaces;
using Operations.SurfaceSegmentChaining.Basics;

namespace Operations.PositionRemovals.FillActions
{
    internal class CornerRemovalFill<T> : IFillAction<T>
    {
        private MatchingConditionals _matching = new MatchingConditionals();
        private AngleConditionals _angleCheck = new AngleConditionals();
        private IFillConditionals _conditionals;
        private PlanarLoop<T> _planarLoop;

        public IFillConditionals FillConditions { get { return _conditionals; } }

        public void PresetMatching(Position position, SurfaceRayContainer<PositionNormal>[] perimeterPoints)
        {
            _matching.SetPrimaryMatchingPoints([perimeterPoints.Single(p => p.Reference.PositionObject.Cardinality > 2).Reference.PositionObject]);
        }

        public List<IndexSurfaceTriangle> Run(PlanarLoop<T> planarLoop)
        {
            _conditionals = _matching;
            _planarLoop = planarLoop;
            _matching.MatchWithPrimaryMatchingPoints();
            var result = EvaluateByMatchingPoints();
            if (result is not null) { return result; }
            _conditionals = _angleCheck;

            _angleCheck.MaxAngle = 2.5;
            result = EvaluateByMatchingPoints();
            if (result is not null) { return result; }
            _angleCheck.MaxAngle = 3.0;
            result = EvaluateByMatchingPoints();
            if (result is not null) { return result; }
            _angleCheck.MaxAngle = 3.1;
            result = EvaluateByMatchingPoints();
            if (result is not null) { return result; }
            _angleCheck.Unconditional = true;
            result = EvaluateByMatchingPoints();

            if (result is null) { _planarLoop.ThrowError(); return _planarLoop.IndexedFillTriangles; }
            return result;
        }

        private List<IndexSurfaceTriangle> EvaluateByMatchingPoints()
        {
            if (_planarLoop.LoopForFillings(0, false, false)) return _planarLoop.IndexedFillTriangles;

            for (int i = 1; i <= _planarLoop.IndexLoop.Count / 2; i++)
            {
                if (_planarLoop.LoopForFillings(i, false, false)) return _planarLoop.IndexedFillTriangles;
                if (_planarLoop.LoopForFillings(-i, false, false)) return _planarLoop.IndexedFillTriangles;
            }
            return null;
        }
    }
}
