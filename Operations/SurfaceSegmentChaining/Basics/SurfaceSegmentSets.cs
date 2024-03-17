
namespace Operations.SurfaceSegmentChaining.Basics
{
    internal class SurfaceSegmentSets<G, T>
    {
        public int NodeId;
        public int GroupKey;
        public G GroupObject;
        public SurfaceSegmentContainer<T>[] DividingSegments = new SurfaceSegmentContainer<T>[0];
        public SurfaceSegmentContainer<T>[] PerimeterSegments = new SurfaceSegmentContainer<T>[0];
    }
}
