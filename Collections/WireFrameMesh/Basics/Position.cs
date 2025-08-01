﻿using Collections.Buckets.Interfaces;
using BasicObjects.GeometricObjects;
using Collections.Buckets;
namespace Collections.WireFrameMesh.Basics
{
    public class Position : IBox
    {
        private static int _id = 0;
        private static object lockObject = new object();

        internal Position(Point3D point)
        {
            _position = point;

            lock (lockObject)
            {
                Id = _id++;
            }
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

        internal List<PositionNormal> _positionNormals { get; } = new List<PositionNormal>();

        public IReadOnlyList<PositionNormal> PositionNormals 
        {
            get { return _positionNormals; }
        }

        public IEnumerable<PositionTriangle> Triangles
        {
            get { return _positionNormals.SelectMany(p => p.Triangles); }
        }

        public int Cardinality
        {
            get { return _positionNormals.Count; }
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

        public Vector3D Normal
        {
            get
            {
                return Vector3D.Average(_positionNormals.Select(p => p.Normal));
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

        internal bool Disabled { get; set; }
    }
}
