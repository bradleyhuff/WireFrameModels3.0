using Collections.Buckets.Interfaces;
using BasicObjects.GeometricObjects;
using M = BasicObjects.Math;

namespace Collections.WireFrameBuilder.Basics
{
    public class Position : IBox
    {
        private static int _id = 0;

        internal Position()
        {
            Id = _id++;
        }

        public int Id { get; }

        private Point3D _position = null;
        private Rectangle3D _box = null;
        private List<PositionNormal> _positionNormals = new List<PositionNormal>();

        public IReadOnlyList<PositionNormal> PositionNormals 
        {
            get { return _positionNormals; }
        }

        internal void SetPositionNormals(IEnumerable<PositionNormal> positionNormals)
        {
            _positionNormals = positionNormals.ToList();
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
                    _box = new Rectangle3D(_position, M.Double.DifferenceError);
                }
                return _box;
            }
        }
    }
}
