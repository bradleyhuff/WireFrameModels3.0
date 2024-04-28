using BasicObjects.GeometricObjects;
using Operations.PlanarFilling.Abstracts;
using Operations.PlanarFilling.Basics;
using Operations.PlanarFilling.Filling.Interfaces;
using Operations.SurfaceSegmentChaining.Interfaces;

namespace Operations.PlanarFilling.Filling
{
    internal class PlanarFilling<G, T> : AbstractFilling<G, T> where G : PlanarFillingGroup
    {
        public PlanarFilling(ISurfaceSegmentChaining<G, T> chaining, int triangleID) : base(chaining, triangleID) { }

        protected override IFillingLoopSet CreateFillingLoopSet(object input, IReadOnlyList<Ray3D> referenceArray, int[] perimeterIndexLoop, int triangleID)
        {
            if (input is InternalPlanarLoopSet)
            {
                var loopSet = (InternalPlanarLoopSet)input;
                return new InternalPlanarLoopSet(loopSet.Plane, loopSet.TestSegmentLength, referenceArray, perimeterIndexLoop, triangleID);
            }
            if (input is G)
            {
                var loopSet = (G)input;
                return new InternalPlanarLoopSet(loopSet.Plane, loopSet.TestSegmentLength, referenceArray, perimeterIndexLoop, triangleID);
            }
            return null;
        }
    }
}
