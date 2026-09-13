
using Collections.WireFrameMesh.Basics;

namespace Operations.SurfaceSegmentChaining.Basics
{
    internal class SurfaceSegmentSets<G, T> where T: IId
    {
        public int NodeId;
        public int GroupKey;
        public G GroupObject;
        public SurfaceSegmentContainer<T>[] IntersectionSegments = new SurfaceSegmentContainer<T>[0];
        public SurfaceSegmentContainer<T>[] PerimeterSegments = new SurfaceSegmentContainer<T>[0];

        public void Show()
        {
            Console.WriteLine($"NodeId {NodeId} GroupKey {GroupKey}");
            Console.WriteLine($"Intersection Segments {IntersectionSegments.Length}\n{string.Join("\n", IntersectionSegments.Select(d => $"[{d.Segment.A.Point} {d.Segment.B.Point} {d.Segment.Segment.Length}]"))}");
            Console.WriteLine($"Perimeter Segments {PerimeterSegments.Length}\n{string.Join("\n", PerimeterSegments.Select(d => $"[{d.Segment.A.Point} {d.Segment.B.Point} {d.Segment.Segment.Length}]"))}");

            Console.WriteLine($"Intersection Segments {IntersectionSegments.Length}\n{string.Join("\n", IntersectionSegments.Select(d => $"[{d.A.Index} {d.B.Index}]"))}");
            Console.WriteLine($"Perimeter Segments {PerimeterSegments.Length}\n{string.Join("\n", PerimeterSegments.Select(d => $"[{d.A.Index} {d.B.Index}]"))}");
        }
    }
}
