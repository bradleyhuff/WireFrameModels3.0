using BasicObjects.GeometricObjects;
using Operations.Filling.Abstracts;
using Operations.Filling.Interfaces;

namespace Operations.Filling.Planar
{
    internal class PlanarLoop : AbstractLoop
    {
        public PlanarLoop(Plane plane, double testSegmentLength, IReadOnlyList<Ray3D> referenceArray, int[] indexLoop, int triangleID) : base(referenceArray, indexLoop, triangleID)
        {
            _testSegmentLength = testSegmentLength;
            Plane = plane;
        }

        double _testSegmentLength;
        IFillingRegions _regionOutLine;
        IFillingRegions _regionInternal;

        public Plane Plane { get; }

        protected override IReadOnlyList<Ray3D> GetAppliedLoopPoints()
        {
            return IndexLoop.Select(i => Plane.Projection(_referenceArray[i])).ToArray();
        }
        protected override IFillingRegions RegionOutLines
        {
            get
            {
                if (_regionOutLine is null)
                {
                    _regionOutLine = new PlanarRegions(Plane, _testSegmentLength);
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
                    _regionInternal = new PlanarRegions(Plane, _testSegmentLength);
                    _regionInternal.Load(LoopSegments.Concat(InternalLoops.SelectMany(l => l.LoopSegments)));
                }
                return _regionInternal;
            }
        }

        protected override void ResetWithInternalLoops()
        {
            _regionInternal = null;
        }

        protected override bool OutLineContainsPoint(Point3D testPoint)
        {
            return RegionOutLines.IsInInterior(testPoint);
        }
    }
}
