using BasicObjects.GeometricObjects;
using Collections.Buckets.Interfaces;

namespace Operations.SurfaceSegmentChaining.Basics.Abstractions
{
    public abstract class BaseSegment : IBox
    {
        private static int _id = 0;
        public BaseSegment(LineSegment3D segment, double margin = BasicObjects.Math.Double.ProximityError)
        {
            Segment = segment;

            var box = Rectangle3D.Containing(segment.Start, segment.End);
            Box = new Rectangle3D(box.MinPoint, box.MaxPoint + new Vector3D(margin, margin, margin));
            Id = _id++;
        }
        public int Id { get; }

        public IEnumerable<Point3D> Points
        {
            get
            {
                yield return Segment.Start;
                yield return Segment.End;
            }
        }

        public LineSegment3D Segment { get; private set; }
        public Rectangle3D Box { get; private set; }
    }
}
