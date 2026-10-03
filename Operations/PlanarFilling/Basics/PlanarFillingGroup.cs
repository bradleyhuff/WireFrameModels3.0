
using BasicObjects.GeometricObjects;
using Operations.Intermesh.Basics;
using Operations.SurfaceSegmentChaining.Basics.Abstractions;

namespace Operations.PlanarFilling.Basics
{
    public class PlanarFillingGroup : LoopGroupObjects
    {
        public PlanarFillingGroup(Plane plane, double testSegmentLength) : base()
        {
            Plane = plane;
            TestSegmentLength = testSegmentLength;
        }

        public PlanarFillingGroup(int triangleId, Plane plane, double testSegmentLength) : base()
        {
            Plane = plane;
            TestSegmentLength = testSegmentLength;
            TriangleId = triangleId;
        }
        public Plane Plane { get; protected set; }
        public double TestSegmentLength { get; protected set; }

        public int TriangleId { get; }

        public override PlanarFillingGroup Clone()
        {
            return new PlanarFillingGroup(TriangleId, Plane, TestSegmentLength);
        }


    }
}
