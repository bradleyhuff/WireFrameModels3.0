using BasicObjects.GeometricObjects;
using Operations.PlanarFilling.Filling.Interfaces;
using Operations.PlanarFilling.Abstracts;

namespace Operations.PlanarFilling.Filling
{
    internal class InternalPlanarLoop : AbstractLoop
    {
        public InternalPlanarLoop(Plane plane, double testSegmentLength, IReadOnlyList<Ray3D> referenceArray, int[] indexLoop, int triangleID) : base(referenceArray, indexLoop, triangleID)
        {
            _testSegmentLength = testSegmentLength;
            Plane = plane;
        }

        double _testSegmentLength;
        IFillingRegions _shellOutLine;
        IFillingRegions _shell;

        public Plane Plane { get; }

        protected override IReadOnlyList<Point3D> GetProjectedLoopPoints()
        {
            return IndexLoop.Select(i => Plane.Projection(_referenceArray[i].Point)).ToArray();
        }
        protected override IFillingRegions ShellOutLines
        {
            get
            {
                if (_shellOutLine is null)
                {
                    _shellOutLine = new InternalPlanarRegions(Plane, _testSegmentLength);
                    _shellOutLine.Load(LoopSegments);
                }
                return _shellOutLine;
            }
        }

        protected override IFillingRegions Shell
        {
            get
            {
                if (_shell is null)
                {
                    _shell = new InternalPlanarRegions(Plane, _testSegmentLength);
                    _shell.Load(LoopSegments.Concat(InternalLoops.SelectMany(l => l.LoopSegments)));
                }
                return _shell;
            }
        }

        protected override void ResetWithInternalLoops()
        {
            _shell = null;
        }
        protected override Point3D GetNextPoint(Point3D leftPoint, Point3D currentPoint, Point3D startLoopPoint, Point3D leftLoopPoint, Point3D rightLoopPoint)
        {
            var direction = Plane.Normal.Direction;

            var angle = Vector3D.SignedAngle(direction, (leftPoint - currentPoint).Direction, (startLoopPoint - currentPoint).Direction);
            var rightOptionAngle = Vector3D.SignedAngle(direction, (currentPoint - startLoopPoint).Direction, (rightLoopPoint - startLoopPoint).Direction);

            if (System.Math.Sign(angle) == System.Math.Sign(rightOptionAngle)) { return rightLoopPoint; }
            return leftLoopPoint;
        }
    }
}
