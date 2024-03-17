
using BasicObjects.GeometricObjects;

namespace Operations.RegionalFilling.Basics
{
    internal class PlanarFillingGroup
    {
        //public PlanarFillingGroup(/*PlanarGroupNode planar,*/Plane plane, double testSegmentLength)
        //{
        //    //Planar = planar;
        //    Plane = plane;
        //    TestSegmentLength = testSegmentLength;
        //}
        public PlanarFillingGroup(Plane plane, double testSegmentLength)
        {
            Plane = plane;
            TestSegmentLength = testSegmentLength;
        }
        protected PlanarFillingGroup() { }
        //public PlanarGroupNode Planar { get; protected set; }
        public Plane Plane { get; protected set; }
        public double TestSegmentLength { get; protected set; }
    }
}
