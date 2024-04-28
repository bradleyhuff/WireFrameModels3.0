using BasicObjects.GeometricObjects;
using Operations.Filling.Abstracts;
using Operations.Filling.Basics;
using Operations.Filling.Interfaces;
using Operations.SurfaceSegmentChaining.Interfaces;

namespace Operations.Filling.Planar
{
    internal class PlanarFilling<G, T> : AbstractFilling<G, T> where G : PlanarFillingGroup
    {
        public PlanarFilling(ISurfaceSegmentChaining<G, T> chaining, int triangleID) : base(chaining, triangleID) { }

        protected override IFillingLoopSet CreateFillingLoopSet(object input, IReadOnlyList<Ray3D> referenceArray, int[] perimeterIndexLoop, int triangleID)
        {
            if (input is PlanarLoopSet)
            {
                var loopSet = (PlanarLoopSet)input;
                return new PlanarLoopSet(loopSet.Plane, loopSet.TestSegmentLength, referenceArray, perimeterIndexLoop, triangleID);
            }
            if (input is G)
            {
                var loopSet = (G)input;
                return new PlanarLoopSet(loopSet.Plane, loopSet.TestSegmentLength, referenceArray, perimeterIndexLoop, triangleID);
            }
            return null;
        }
    }
}
