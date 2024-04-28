
using BasicObjects.GeometricObjects;

namespace Operations.Filling.Basics
{
    internal class PlanarFillingGroup
    {
        public PlanarFillingGroup(Plane plane, double testSegmentLength)
        {
            Plane = plane;
            TestSegmentLength = testSegmentLength;
        }
        protected PlanarFillingGroup() { }
        public Plane Plane { get; protected set; }
        public double TestSegmentLength { get; protected set; }
    }
}
