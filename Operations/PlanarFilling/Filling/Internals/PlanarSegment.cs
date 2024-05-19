using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.Buckets.Interfaces;

namespace Operations.PlanarFilling.Filling.Internals
{
    internal class PlanarSegment<T> : IBox
    {
        public PlanarSegment(Point3D a, Point3D b, PlanarLoop<T> parentLoop = null)
        {
            Segment = new LineSegment3D(a, b);
            var box = Rectangle3D.Containing(a, b);
            Box = new Rectangle3D(box.MinPoint, box.MaxPoint + new Vector3D(BoxBucket.MARGINS, BoxBucket.MARGINS, BoxBucket.MARGINS));
            ParentLoop = parentLoop;
        }
        public PlanarLoop<T> ParentLoop { get; }
        public LineSegment3D Segment { get; private set; }
        public Rectangle3D Box { get; private set; }

        public static IEnumerable<PlanarSegment<T>> GetLoop(PlanarLoop<T> parentLoop, Point3D[] points)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                yield return new PlanarSegment<T>(points[i], points[i + 1], parentLoop);
            }
            yield return new PlanarSegment<T>(points[points.Length - 1], points[0], parentLoop);
        }
    }
}
