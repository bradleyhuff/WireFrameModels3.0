using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Operations.Filling.Abstracts;
using Operations.Filling.Interfaces;

namespace Operations.Filling.Prefilled
{
    internal class PrefilledLoop : AbstractLoop
    {
        private PositionTriangle[] _region;
        public PrefilledLoop(PositionTriangle[] region, IReadOnlyList<Ray3D> referenceArray, int[] indexLoop, int triangleID) : base(referenceArray, indexLoop, triangleID)
        {
            _region = region;
        }

        IFillingRegions _regionOutLine;
        IFillingRegions _regionInternal;

        protected override IFillingRegions RegionOutLines
        {
            get
            {
                if (_regionOutLine is null)
                {
                    _regionOutLine = new PrefilledRegions(_region);
                    _regionOutLine.Load(LoopSegments);
                }
                return _regionOutLine;
            }
        }

        protected override IFillingRegions RegionInternal
        {
            get
            {
                if (_regionInternal is null)
                {
                    _regionInternal = new PrefilledRegions(_region);
                    _regionInternal.Load(LoopSegments.Concat(InternalLoops.SelectMany(l => l.LoopSegments)));
                }
                return _regionInternal;
            }
        }

        protected override IReadOnlyList<Ray3D> GetAppliedLoopPoints()
        {
            return IndexLoop.Select(i => _referenceArray[i]).ToArray();
        }

        protected override void ResetWithInternalLoops()
        {
            _regionInternal = null;
        }
    }
}
