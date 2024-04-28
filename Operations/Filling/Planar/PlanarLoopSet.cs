using BasicObjects.GeometricObjects;
using Operations.Filling.Abstracts;
using Operations.Filling.Interfaces;

namespace Operations.Filling.Planar
{
    internal class PlanarLoopSet : AbstractLoopSet
    {
        public PlanarLoopSet(Plane plane, double testSegmentLength, IReadOnlyList<Ray3D> referenceArray, int[] perimeterIndexLoop, int triangleID) :
            base(referenceArray, perimeterIndexLoop, triangleID)
        {
            Plane = plane;
            TestSegmentLength = testSegmentLength;
        }

        protected override IFillingLoop CreateFillingLoop(IReadOnlyList<Ray3D> referenceArray, int[] indexLoop, int triangleID)
        {
            return new PlanarLoop(Plane, TestSegmentLength, referenceArray, indexLoop, triangleID);
        }
        public Plane Plane { get; }
        public double TestSegmentLength { get; }
    }
}
