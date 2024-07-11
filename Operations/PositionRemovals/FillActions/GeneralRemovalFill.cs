using Collections.WireFrameMesh.Basics;
using Operations.PlanarFilling.Filling.Internals;
using Operations.PositionRemovals.Conditionals;
using Operations.PositionRemovals.Interfaces;
using Operations.PositionRemovals.Internals;
using Operations.SurfaceSegmentChaining.Basics;

namespace Operations.PositionRemovals.FillActions
{
    internal class GeneralRemovalFill<T> : IFillAction<T>
    {
        private MatchingConditionals _matching = new MatchingConditionals();
        private AngleConditionals _angleCheck = new AngleConditionals();
        private IFillConditionals _conditionals;
        private PlanarLoop<T> _planarLoop;
        

        public IFillConditionals FillConditions { get { return _conditionals; } }

        public void PresetMatching(Position position, SurfaceRayContainer<PositionNormal>[] perimeterPoints)
        {
            var primaryMatchingPoints = GetPrimaryMatchingPoints(position, perimeterPoints);
            _matching.SetPrimaryMatchingPoints(primaryMatchingPoints.ToArray());
            var secondaryMatchingPoints = primaryMatchingPoints.Concat(GetSecondaryMatchingPoints(position, perimeterPoints));
            _matching.SetSecondaryMatchingPoints(secondaryMatchingPoints.ToArray());
        }

        public List<IndexSurfaceTriangle> Run(PlanarLoop<T> planarLoop)
        {
            _conditionals = _matching;
            _planarLoop = planarLoop;
            _matching.MatchWithPrimaryMatchingPoints();
            var result = EvaluateByMatchingPoints();
            if (result is not null) { return result; }
            _matching.MatchWithSecondaryMatchingPoints();
            result = EvaluateByMatchingPoints();
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


        private List<Position> GetPrimaryMatchingPoints(Position position, SurfaceRayContainer<PositionNormal>[] perimeterPoints)
        {
            EdgeCluster adjacentPoints = null;
            if (position.Cardinality == 2) { adjacentPoints = new EdgeCluster(position); }

            var matchingPoints = new List<Position>();
            if (perimeterPoints is not null)
            {
                matchingPoints.AddRange(perimeterPoints.Select(p => p.Reference.PositionObject).Where(p => p.Cardinality < 2));
            }
            if (adjacentPoints is not null)
            {
                matchingPoints.AddRange(adjacentPoints.Cluster.Where(c => c.Id != position.Id && c.Cardinality < 3));
            }
            return matchingPoints;
        }

        private List<Position> GetSecondaryMatchingPoints(Position position, SurfaceRayContainer<PositionNormal>[] perimeterPoints)
        {
            if (perimeterPoints is null) { return new List<Position>(); }
            var matchingPoints = new List<Position>();
            EdgeCluster adjacentPoints = null;
            if (position.Cardinality == 2) { adjacentPoints = new EdgeCluster(position); }
            if (adjacentPoints is null) { return new List<Position>(); }
            var cornerPoints = adjacentPoints.Cluster.Where(c => c.Id != position.Id && c.Cardinality > 2);
            if (!cornerPoints.Any()) { return new List<Position>(); }
            var cornerPositions = cornerPoints.SelectMany(t => t.Triangles.
                SelectMany(t => t.Positions)).Select(p => p.PositionObject).DistinctBy(p => p.Id).Where(p => p.Id != position.Id);
            if (!cornerPositions.Any()) { return new List<Position>(); }

            return cornerPositions.IntersectBy(perimeterPoints.Select(p => p.Reference.PositionObject.Id), c => c.Id).ToList();
        }
    }
}
