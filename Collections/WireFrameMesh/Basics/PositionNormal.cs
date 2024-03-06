using Collections.Buckets.Interfaces;
using BasicObjects.GeometricObjects;
using M = BasicObjects.Math;

namespace Collections.WireFrameMesh.Basics
{
    public class PositionNormal : IBox
    {
        private static int _id = 0;

        internal PositionNormal(Point3D position, Vector3D normal, BasicWireFrameMesh.WireFrameMesh mesh)
        {
            _position = position;
            _normal = normal;
            Mesh = mesh;
            Id = _id++;
        }

        public int Id { get; }
        internal BasicWireFrameMesh.WireFrameMesh Mesh { get; }
        public Position? PositionObject { get; private set; }

        internal List<PositionTriangle> _triangles = new List<PositionTriangle>();

        public IReadOnlyList<PositionTriangle> Triangles
        {
            get { return _triangles; }
        }

        public Rectangle3D Box
        {
            get
            {
                if (PositionObject is null) { return new Rectangle3D(_position, M.Double.DifferenceError); }
                return PositionObject.Box;
            }
        }

        private Point3D _position;
        private Vector3D _normal;

        public Point3D Position
        {
            get 
            {
                if (PositionObject is not null) { return PositionObject.Point; }
                return _position;
            }
        }

        public Vector3D Normal
        {
            get { return _normal; }
            set
            {
                _normal = value;
                _wasInverted = false;
            }
        }

        public static Ray3D GetRay(PositionNormal element)
        {
            return new Ray3D(element.Position, element.Normal);
        }

        public void LinkPosition(Position positionObject)
        {
            DelinkPosition();
            PositionObject = positionObject;
            PositionObject._positionNormals.Add(this);
        }

        public void DelinkPosition()
        {
            if(PositionObject is null) { return; }
            PositionObject._positionNormals.Remove(this);
            PositionObject = null;
        }

        private bool _wasInverted = false;

        public void InvertNormal()
        {
            if (_wasInverted || _normal is null) { return; }
            Normal = -Normal;
            _wasInverted = true;
        }

        public void CommitInvert()
        {
            _wasInverted = false;
        }
    }
}
