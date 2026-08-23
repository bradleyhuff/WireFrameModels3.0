using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Basics;

namespace Operations.SurfaceSegmentChaining.Basics
{
    internal class SurfaceSegmentContainer<T> where T: IId
    {
        public SurfaceSegmentContainer(SurfaceRayContainer<T> a, SurfaceRayContainer<T> b)
        {
            A = a;
            B = b;
            Segment = new SurfaceLineSegment(a, b);
            Key = new Combination2(a.Reference.Id, b.Reference.Id);
        }

        public Combination2 Key { get; }

        public SurfaceLineSegment Segment { get; }

        public SurfaceRayContainer<T> A { get; }
        public SurfaceRayContainer<T> B { get; }

        public IEnumerable<SurfaceRayContainer<T>> Points
        {
            get
            {
                yield return A; yield return B;
            }
        }
    }
}
