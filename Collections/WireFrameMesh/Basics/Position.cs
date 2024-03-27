using Collections.Buckets.Interfaces;
using BasicObjects.GeometricObjects;
using Collections.Buckets;

namespace Collections.WireFrameMesh.Basics
{
    public class Position : IBox
    {
        private static int _id = 0;

        internal Position(Point3D point)
        {
            _position = point;

            Id = _id++;
        }

        public int Id { get; }

        public override int GetHashCode()
        {
            return Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not Position) { return false; }
            Position compare = (Position)obj;
            return Id == compare.Id;
        }

        private Point3D _position;
        private Rectangle3D _box;
        internal List<PositionNormal> _positionNormals = new List<PositionNormal>();

        public IReadOnlyList<PositionNormal> PositionNormals 
        {
            get { return _positionNormals; }
        }

        public Point3D Point
        {
            get { return _position; }
            set
            {
                _position = value;
                _box = null;
            }
        }

        public Rectangle3D Box
        {
            get
            {
                if (_box is null && _position is not null)
                {
                    _box = new Rectangle3D(_position, BoxBucket.MARGINS);
                }
                return _box;
            }
        }
    }
}
