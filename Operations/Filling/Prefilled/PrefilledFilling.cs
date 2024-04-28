using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Operations.Filling.Abstracts;
using Operations.Filling.Interfaces;
using Operations.SurfaceSegmentChaining.Interfaces;

namespace Operations.Filling.Prefilled
{
    internal class PrefilledFilling<G, T> : AbstractFilling<G, T>
    {
        PositionTriangle[] _region;
        public PrefilledFilling(PositionTriangle[] region, ISurfaceSegmentChaining<G, T> chaining, int triangleID) : base(chaining, triangleID)
        {
            _region = region;
        }

        protected override IFillingLoopSet CreateFillingLoopSet(object input, IReadOnlyList<Ray3D> referenceArray, int[] perimeterIndexLoop, int triangleID)
        {
            return new PrefilledLoopSet(_region, referenceArray, perimeterIndexLoop, triangleID);
        }
    }
}
