using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Operations.Filling.Abstracts;
using Operations.Filling.Interfaces;

namespace Operations.Filling.Prefilled
{
    internal class PrefilledLoopSet : AbstractLoopSet
    {
        public PrefilledLoopSet(PositionTriangle[] region, IReadOnlyList<Ray3D> referenceArray, int[] perimeterIndexLoop, int triangleID) : base(referenceArray, perimeterIndexLoop, triangleID)
        {
            Region = region;
        }

        protected override IFillingLoop CreateFillingLoop(IReadOnlyList<Ray3D> referenceArray, int[] indexLoop, int triangleID)
        {
            return new PrefilledLoop(Region, referenceArray, indexLoop, triangleID);
        }

        public PositionTriangle[] Region { get; }
    }
}
