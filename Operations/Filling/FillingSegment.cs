using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.Buckets.Interfaces;
using Operations.Filling.Interfaces;

namespace Operations.Filling
{
    internal class FillingSegment : IBox
    {
        public FillingSegment(Point3D a, Point3D b, IFillingLoop parentLoop = null)
        {
            Segment = new LineSegment3D(a, b);
            var box = Rectangle3D.Containing(a, b);
            Box = new Rectangle3D(box.MinPoint, box.MaxPoint + new Vector3D(BoxBucket.MARGINS, BoxBucket.MARGINS, BoxBucket.MARGINS));
            ParentLoop = parentLoop;
        }
        public IFillingLoop ParentLoop { get; }
        public LineSegment3D Segment { get; private set; }
        public Rectangle3D Box { get; private set; }

        public static IEnumerable<FillingSegment> GetLoop(IFillingLoop parentLoop, Point3D[] points)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                yield return new FillingSegment(points[i], points[i + 1], parentLoop);
            }
            yield return new FillingSegment(points[points.Length - 1], points[0], parentLoop);
        }
    }
}
