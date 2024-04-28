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

        protected override IReadOnlyList<Point3D> GetProjectedLoopPoints()
        {
            return IndexLoop.Select(i => Plane.Projection(_referenceArray[i].Point)).ToArray();
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
        protected override Point3D GetNextPoint(Point3D leftPoint, Point3D currentPoint, Point3D startLoopPoint, Point3D leftLoopPoint, Point3D rightLoopPoint)
        {
            var direction = Plane.Normal.Direction;

            var angle = Vector3D.SignedAngle(direction, (leftPoint - currentPoint).Direction, (startLoopPoint - currentPoint).Direction);
            var rightOptionAngle = Vector3D.SignedAngle(direction, (currentPoint - startLoopPoint).Direction, (rightLoopPoint - startLoopPoint).Direction);

            if (Math.Sign(angle) == Math.Sign(rightOptionAngle)) { return rightLoopPoint; }
            return leftLoopPoint;
        }
    }
}
