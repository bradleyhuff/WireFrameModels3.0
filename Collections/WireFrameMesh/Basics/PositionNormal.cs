using Collections.Buckets.Interfaces;
using BasicObjects.GeometricObjects;
using M = BasicObjects.Math;
using Collections.Buckets;
using Collections.WireFrameMesh.Interfaces;

namespace Collections.WireFrameMesh.Basics
{
    public class PositionNormal : IBox
    {
        private static int _id = 0;
        private static object lockObject = new object();

        internal PositionNormal(Point3D position, Vector3D normal, IWireFrameMeshInternal mesh)
        {
            _position = position;
            _normal = normal;
            Mesh = mesh;
            lock (lockObject)
            {
                Id = _id++;
            }
        }

        public int Id { get; }
        internal IWireFrameMeshInternal Mesh { get; }
        public Position? PositionObject { get; private set; }

        internal List<PositionTriangle> _triangles { get; } = new List<PositionTriangle>();

        public IReadOnlyList<PositionTriangle> Triangles
        {
            get { return _triangles; }
        }
        private Rectangle3D _box;
        public Rectangle3D Box
        {
            get
            {
                if (PositionObject is null) {
                    if(_box is null)
                    {
                        _box = new Rectangle3D(_position, BoxBucket.MARGINS);
                    }
                    return _box; 
                }
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
            set
            {
                if (PositionObject is not null) { PositionObject.Point = value; }
                _position = value;
                _box = null;
            }
        }

        public Vector3D Normal
        {
            get { return _normal; }
            set { _normal = value; }
        }

        public static Ray3D GetRay(PositionNormal element)
        {
            return new Ray3D(element.Position, element.Normal);
        }

        internal void LinkPosition(Position positionObject)
        {
            DelinkPosition();
            PositionObject = positionObject;
            PositionObject._positionNormals.Add(this);
        }

        internal void DelinkPosition()
        {
            if(PositionObject is null) { return; }
            PositionObject._positionNormals.Remove(this);
            PositionObject = null;
        }

        public override int GetHashCode()
        {
            return Id;
        }
        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not PositionNormal) { return false; }
            PositionNormal compare = (PositionNormal)obj;
            return Id == compare.Id;
        }

        public void MergeFrom(PositionNormal other)
        {
            var triangles = other.PositionObject.Triangles;

            foreach (var triangle in triangles)
            {
                Mesh.AddTriangle(
                    triangle.A.Id == other.Id ? this : triangle.A,
                    triangle.B.Id == other.Id ? this : triangle.B,
                    triangle.C.Id == other.Id ? this : triangle.C,
                    triangle.Trace, triangle.Tag);
            }

            Mesh.RemoveAllTriangles(triangles);
        }
    }
}
