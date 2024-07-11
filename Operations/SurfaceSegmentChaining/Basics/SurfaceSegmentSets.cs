
namespace Operations.SurfaceSegmentChaining.Basics
{
    internal class SurfaceSegmentSets<G, T>
    {
        public int NodeId;
        public int GroupKey;
        public G GroupObject;
        public SurfaceSegmentContainer<T>[] DividingSegments = new SurfaceSegmentContainer<T>[0];
        public SurfaceSegmentContainer<T>[] PerimeterSegments = new SurfaceSegmentContainer<T>[0];

        public void Show()
        {
            Console.WriteLine($"NodeId {NodeId} GroupKey {GroupKey}");
            Console.WriteLine($"Dividing Segments {DividingSegments.Length}\n{string.Join("\n", DividingSegments.Select(d => $"[{d.Segment.A.Point} {d.Segment.B.Point} {d.Segment.Segment.Length}]"))}");
            Console.WriteLine($"Perimeter Segments {PerimeterSegments.Length}\n{string.Join("\n", PerimeterSegments.Select(d => $"[{d.Segment.A.Point} {d.Segment.B.Point} {d.Segment.Segment.Length}]"))}");

            Console.WriteLine($"Dividing Segments {DividingSegments.Length}\n{string.Join("\n", DividingSegments.Select(d => $"[{d.A.Index} {d.B.Index}]"))}");
            Console.WriteLine($"Perimeter Segments {PerimeterSegments.Length}\n{string.Join("\n", PerimeterSegments.Select(d => $"[{d.A.Index} {d.B.Index}]"))}");
        }
    }
}
