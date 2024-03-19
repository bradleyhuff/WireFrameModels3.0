using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.Buckets.Interfaces;
using Operations.PlanarFilling.Basics;

namespace Operations.PlanarFilling.Filling
{
    internal partial class PlanarFilling<G, T> where G : PlanarFillingGroup
    {
        private class InternalPlanarSegment : IBox
        {
            public InternalPlanarSegment(Point3D a, Point3D b, InternalPlanarLoop parentLoop = null)
            {
                Segment = new LineSegment3D(a, b);
                var box = Rectangle3D.Containing(a, b);
                Box = new Rectangle3D(box.MinPoint, box.MaxPoint + new Vector3D(BoxBucket.MARGINS, BoxBucket.MARGINS, BoxBucket.MARGINS));
                ParentLoop = parentLoop;
            }
            public InternalPlanarLoop ParentLoop { get; }
            public LineSegment3D Segment { get; private set; }
            public Rectangle3D Box { get; private set; }

            public static IEnumerable<InternalPlanarSegment> GetLoop(InternalPlanarLoop parentLoop, Point3D[] points)
            {
                for (int i = 0; i < points.Length - 1; i++)
                {
                    yield return new InternalPlanarSegment(points[i], points[i + 1], parentLoop);
                }
                yield return new InternalPlanarSegment(points[points.Length - 1], points[0], parentLoop);
            }
        }
    }
}
