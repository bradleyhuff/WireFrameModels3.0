using BasicObjects.GeometricObjects;
using Operations.PlanarFilling.Abstracts;
using Operations.PlanarFilling.Filling.Interfaces;

namespace Operations.PlanarFilling.Filling
{
    internal class InternalPlanarLoopSet : AbstractLoopSet
    {
        public InternalPlanarLoopSet(Plane plane, double testSegmentLength, IReadOnlyList<Ray3D> referenceArray, int[] perimeterIndexLoop, int triangleID): 
            base(referenceArray, perimeterIndexLoop, triangleID)
        {
            Plane = plane;
            TestSegmentLength = testSegmentLength;
        }

        protected override IFillingLoop CreateFillingLoop(IReadOnlyList<Ray3D> referenceArray, int[] indexLoop, int triangleID)
        {
            return new InternalPlanarLoop(Plane, TestSegmentLength, referenceArray, indexLoop, triangleID);
        }
        public Plane Plane { get; }
        public double TestSegmentLength { get; }
    }
}
