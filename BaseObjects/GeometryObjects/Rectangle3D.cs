using E = BasicObjects.Math;

namespace BasicObjects.GeometricObjects
{
    public class Rectangle3D
    {
        public Rectangle3D(Point3D point, double margin) : this(
         point.X - margin, point.X + margin,
         point.Y - margin, point.Y + margin,
         point.Z - margin, point.Z + margin)
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


        public bool Contains(Point3D point, double flexMargin = E.Double.DifferenceError)
        {
            return MinPoint.X < point.X + flexMargin && point.X < MaxPoint.X + flexMargin &&
                MinPoint.Y < point.Y + flexMargin && point.Y < MaxPoint.Y + flexMargin &&
                MinPoint.Z < point.Z + flexMargin && point.Z < MaxPoint.Z + flexMargin;
        }


        public bool Contains(Rectangle3D box, double flexMargin = E.Double.DifferenceError)
        {
            return MinPoint.X < box.MinPoint.X + flexMargin && box.MaxPoint.X < MaxPoint.X + flexMargin &&
                MinPoint.Y < box.MinPoint.Y + flexMargin && box.MaxPoint.Y < MaxPoint.Y + flexMargin &&
                MinPoint.Z < box.MinPoint.Z + flexMargin && box.MaxPoint.Z < MaxPoint.Z + flexMargin;
        }

        public static bool IsDisjoint(Rectangle3D a, Rectangle3D b, double flexMargin = E.Double.DifferenceError)
        {
            return (b.MinPoint.X > a.MaxPoint.X + flexMargin || a.MinPoint.X > b.MaxPoint.X + flexMargin) ||
                (b.MinPoint.Y > a.MaxPoint.Y + flexMargin || a.MinPoint.Y > b.MaxPoint.Y + flexMargin) ||
                (b.MinPoint.Z > a.MaxPoint.Z + flexMargin || a.MinPoint.Z > b.MaxPoint.Z + flexMargin);
        }

        public static bool IsDisjoint(double minAX, double minAY, double minAZ, double maxAX, double maxAY, double maxAZ,
            double minBX, double minBY, double minBZ, double maxBX, double maxBY, double maxBZ, double flexMargin = E.Double.DifferenceError)
        {
            return (minBX > maxAX + flexMargin || minAX > maxBX + flexMargin) ||
                (minBY > maxAY + flexMargin || minAY > maxBY + flexMargin) ||
                (minBZ > maxAZ + flexMargin || minAZ > maxBZ + flexMargin);
        }

        public static bool IsDisjoint(Triangle3D a, LineSegment3D b, double flexMargin = E.Double.DifferenceError)
        {
            var box = a.Box;
            return (E.Math.Min(b.Start.X, b.End.X) > box.MaxPoint.X + flexMargin || box.MinPoint.X > E.Math.Max(b.Start.X, b.End.X) + flexMargin) ||
                (E.Math.Min(b.Start.Y, b.End.Y) > box.MaxPoint.Y + flexMargin || box.MinPoint.Y > E.Math.Max(b.Start.Y, b.End.Y) + flexMargin) ||
                (E.Math.Min(b.Start.Z, b.End.Z) > box.MaxPoint.Z + flexMargin || box.MinPoint.Z > E.Math.Max(b.Start.Z, b.End.Z) + flexMargin);
        }

        public static bool Overlaps(Rectangle3D a, Rectangle3D b, double flexMargin = E.Double.DifferenceError)
        {
            return !IsDisjoint(a, b, flexMargin);
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
    }
}
