using BaseObjects.Transformations.Interfaces;
using E = BasicObjects.Math;

namespace BasicObjects.GeometricObjects
{
    public class Rectangle3D : IShape3D<Rectangle3D>
    {
        public Rectangle3D(Point3D point, double margin) : this(
         point.X - margin, point.X + margin,
         point.Y - margin, point.Y + margin,
         point.Z - margin, point.Z + margin)
        { }

        public Rectangle3D(Point3D point, double marginX, double marginY, double marginZ) : this(
            point.X - marginX, point.X + marginX,
            point.Y - marginY, point.Y + marginY,
            point.Z - marginZ, point.Z + marginZ)
        { }

        public Rectangle3D(Point3D minPoint, Point3D maxPoint) : this(
            minPoint.X, maxPoint.X, minPoint.Y, maxPoint.Y, minPoint.Z, maxPoint.Z
            )
        { }
        public Rectangle3D(double minX, double maxX, double minY, double maxY, double minZ, double maxZ)
        {
            MinPoint = new Point3D(minX, minY, minZ);
            MaxPoint = new Point3D(maxX, maxY, maxZ);
        }

        public Point3D[] CardinalPoints
        {
            get { return [MinPoint, MaxPoint]; }
        }
        public Vector3D[] CardinalVectors { get { return []; } }
        public Rectangle3D Constructor(Point3D[] cardinalPoints, Vector3D[] cardinalVectors)
        {
            return new Rectangle3D(cardinalPoints[0], cardinalPoints[1]);
        }

        public Rectangle3D Margin(double margin)
        {
            return new Rectangle3D(MinPoint + new Vector3D(-margin, -margin, -margin), MaxPoint + new Vector3D(margin, margin, margin));
        }

        public Point3D GetVertex(bool maxX, bool maxY, bool maxZ)
        {
            int x = maxX ? 1 : 0;
            int y = maxY ? 1 : 0;
            int z = maxZ ? 1 : 0;
            return GetVertex((x << 2) | (y << 1) | z);
        }

        public Point3D GetVertex(int number)
        {
            switch (number)
            {
                case 0: return MinPoint;
                case 1: return new Point3D(MinPoint.X, MinPoint.Y, MaxPoint.Z);
                case 2: return new Point3D(MinPoint.X, MaxPoint.Y, MinPoint.Z);
                case 3: return new Point3D(MinPoint.X, MaxPoint.Y, MaxPoint.Z);
                case 4: return new Point3D(MaxPoint.X, MinPoint.Y, MinPoint.Z);
                case 5: return new Point3D(MaxPoint.X, MinPoint.Y, MaxPoint.Z);
                case 6: return new Point3D(MaxPoint.X, MaxPoint.Y, MinPoint.Z);
                case 7: return MaxPoint;
            }
            return null;
        }


        public Point3D MinPoint { get; private set; }
        private Point3D _centerPoint = null;
        public Point3D CenterPoint
        {
            get
            {
                if (_centerPoint is null)
                {
                    _centerPoint = new Point3D((MinPoint.X + MaxPoint.X) * 0.5, (MinPoint.Y + MaxPoint.Y) * 0.5, (MinPoint.Z + MaxPoint.Z) * 0.5);
                }
                return _centerPoint;
            }
        }
        public Point3D MaxPoint { get; private set; }

        private double _lengthX = 0;
        private bool _hasLengthX = false;
        public double LengthX
        {
            get
            {
                if (!_hasLengthX)
                {
                    _lengthX = MaxPoint.X - MinPoint.X;
                    _hasLengthX = true;
                }
                return _lengthX;
            }
        }
        private double _lengthY = 0;
        private bool _hasLengthY = false;
        public double LengthY
        {
            get
            {
                if (!_hasLengthY)
                {
                    _lengthY = MaxPoint.Y - MinPoint.Y;
                    _hasLengthY = true;
                }
                return _lengthY;
            }
        }
        private double _lengthZ = 0;
        private bool _hasLengthZ = false;
        public double LengthZ
        {
            get
            {
                if (!_hasLengthZ)
                {
                    _lengthZ = MaxPoint.Z - MinPoint.Z;
                    _hasLengthZ = true;
                }
                return _lengthZ;
            }
        }

        private double _diagonal = 0;
        private bool _hasDiagonal = false;
        public double Diagonal
        {
            get
            {
                if (!_hasDiagonal)
                {
                    _diagonal = (MaxPoint - MinPoint).Magnitude;
                    _hasDiagonal = true;
                }
                return _diagonal;
            }
        }

        public bool Contains(Point3D point)
        {
            return MinPoint.X < point.X && point.X < MaxPoint.X &&
                MinPoint.Y < point.Y && point.Y < MaxPoint.Y &&
                MinPoint.Z < point.Z && point.Z < MaxPoint.Z;
        }


        public bool Contains(Rectangle3D box)
        {
            return MinPoint.X < box.MinPoint.X && box.MaxPoint.X < MaxPoint.X &&
                MinPoint.Y < box.MinPoint.Y && box.MaxPoint.Y < MaxPoint.Y &&
                MinPoint.Z < box.MinPoint.Z && box.MaxPoint.Z < MaxPoint.Z;
        }

        public static bool IsDisjoint(Rectangle3D a, Rectangle3D b)
        {
            return (b.MinPoint.X > a.MaxPoint.X || a.MinPoint.X > b.MaxPoint.X) ||
                (b.MinPoint.Y > a.MaxPoint.Y || a.MinPoint.Y > b.MaxPoint.Y) ||
                (b.MinPoint.Z > a.MaxPoint.Z || a.MinPoint.Z > b.MaxPoint.Z);
        }

        public static bool IsDisjoint(double minAX, double minAY, double minAZ, double maxAX, double maxAY, double maxAZ,
            double minBX, double minBY, double minBZ, double maxBX, double maxBY, double maxBZ)
        {
            return (minBX > maxAX || minAX > maxBX) ||
                (minBY > maxAY || minAY > maxBY) ||
                (minBZ > maxAZ || minAZ > maxBZ);
        }

        public static bool IsDisjoint(Triangle3D a, LineSegment3D b)
        {
            var box = a.Box;
            return (E.Math.Min(b.Start.X, b.End.X) > box.MaxPoint.X || box.MinPoint.X > E.Math.Max(b.Start.X, b.End.X)) ||
                (E.Math.Min(b.Start.Y, b.End.Y) > box.MaxPoint.Y || box.MinPoint.Y > E.Math.Max(b.Start.Y, b.End.Y)) ||
                (E.Math.Min(b.Start.Z, b.End.Z) > box.MaxPoint.Z || box.MinPoint.Z > E.Math.Max(b.Start.Z, b.End.Z));
        }

        public static bool Overlaps(Rectangle3D a, Rectangle3D b)
        {
            return !IsDisjoint(a, b);
        }

        public IEnumerable<Rectangle3D> Overlapping(params Rectangle3D[] boxes)
        {
            foreach (var box in boxes)
            {
                if (Overlaps(this, box)) { yield return box; }
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is null || obj is not Rectangle3D) { return false; }
            Rectangle3D compare = (Rectangle3D)obj;
            return compare.MinPoint.Equals(MinPoint) && compare.MaxPoint.Equals(MaxPoint);
        }

        public static bool operator ==(Rectangle3D a, Rectangle3D b)
        {
            return a.MinPoint.Equals(b.MinPoint) && a.MaxPoint.Equals(b.MaxPoint);
        }
        public static bool operator !=(Rectangle3D a, Rectangle3D b)
        {
            return !(a.MinPoint.Equals(b.MinPoint) && a.MaxPoint.Equals(b.MaxPoint));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"Rectangle Min: {MinPoint} Max: {MaxPoint}";
        }

        public static Rectangle3D Containing(Rectangle3D a, Rectangle3D b)
        {
            return new Rectangle3D(
                E.Math.Min(a.MinPoint.X, b.MinPoint.X),
                E.Math.Max(a.MaxPoint.X, b.MaxPoint.X),
                E.Math.Min(a.MinPoint.Y, b.MinPoint.Y),
                E.Math.Max(a.MaxPoint.Y, b.MaxPoint.Y),
                E.Math.Min(a.MinPoint.Z, b.MinPoint.Z),
                E.Math.Max(a.MaxPoint.Z, b.MaxPoint.Z));
        }

        public static Rectangle3D Containing(Rectangle3D a, Point3D b)
        {
            return new Rectangle3D(
                E.Math.Min(a.MinPoint.X, b.X),
                E.Math.Max(a.MaxPoint.X, b.X),
                E.Math.Min(a.MinPoint.Y, b.Y),
                E.Math.Max(a.MaxPoint.Y, b.Y),
                E.Math.Min(a.MinPoint.Z, b.Z),
                E.Math.Max(a.MaxPoint.Z, b.Z));
        }

        public static Rectangle3D Containing(LineSegment3D b)
        {
            return new Rectangle3D(
                E.Math.Min(b.Start.X, b.End.X),
                E.Math.Max(b.Start.X, b.End.X),
                E.Math.Min(b.Start.Y, b.End.Y),
                E.Math.Max(b.Start.Y, b.End.Y),
                E.Math.Min(b.Start.Z, b.End.Z),
                E.Math.Max(b.Start.Z, b.End.Z));
        }

        public static Rectangle3D Containing(params Rectangle3D[] boxes)
        {
            if (boxes == null || boxes.Length == 0) { return null; }
            double minX = boxes.Min(p => p.MinPoint.X);
            double maxX = boxes.Max(p => p.MaxPoint.X);
            double minY = boxes.Min(p => p.MinPoint.Y);
            double maxY = boxes.Max(p => p.MaxPoint.Y);
            double minZ = boxes.Min(p => p.MinPoint.Z);
            double maxZ = boxes.Max(p => p.MaxPoint.Z);

            return new Rectangle3D(minX, maxX, minY, maxY, minZ, maxZ);
        }

        public static Rectangle3D Containing(params Point3D[] points)
        {
            if (points == null || points.Length == 0) { return null; }
            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);
            double minZ = points.Min(p => p.Z);
            double maxZ = points.Max(p => p.Z);

            return new Rectangle3D(minX, maxX, minY, maxY, minZ, maxZ);
        }

        public static Rectangle3D Containing(params LineSegment3D[] segments)
        {
            if (segments == null || segments.Length == 0) { return null; }
            double minX = segments.SelectMany(s => s.Points).Min(p => p.X);
            double maxX = segments.SelectMany(s => s.Points).Max(p => p.X);
            double minY = segments.SelectMany(s => s.Points).Min(p => p.Y);
            double maxY = segments.SelectMany(s => s.Points).Max(p => p.Y);
            double minZ = segments.SelectMany(s => s.Points).Min(p => p.Z);
            double maxZ = segments.SelectMany(s => s.Points).Max(p => p.Z);

            return new Rectangle3D(minX, maxX, minY, maxY, minZ, maxZ);
        }

        public static Rectangle3D CubeContaining(Rectangle3D box, double factor = 1.0)
        {
            var center = box.CenterPoint;
            var radius = E.Math.Max(box.LengthX, box.LengthY, box.LengthZ) * 0.5 * factor;

            return new Rectangle3D(center, radius);
        }
    }
}
