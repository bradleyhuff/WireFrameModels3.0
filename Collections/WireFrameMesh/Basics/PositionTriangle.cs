using Collections.Buckets.Interfaces;
using BasicObjects.GeometricObjects;
using M = BasicObjects.Math;
using Collections.Buckets;

namespace Collections.WireFrameMesh.Basics
{
    public class PositionTriangle : IBox
    {
        private static int _id = 0;
        public PositionTriangle(PositionNormal a, PositionNormal b, PositionNormal c)
        {
            A = a;
            B = b;
            C = c;
            A._triangles.Add(this);
            B._triangles.Add(this);
            C._triangles.Add(this);
            A.Mesh._triangles.Add(this);
            Id = _id++;
        }

        public int Id { get; }

        public PositionNormal A { get; private set; }
        public PositionNormal B { get; private set; }
        public PositionNormal C { get; private set; }

        public void LinkPositionNormals(PositionNormal a, PositionNormal b, PositionNormal c)
        {
            DelinkPositionNormals();

            A = a;
            B = b;
            C = c;
            A._triangles.Add(this);
            B._triangles.Add(this);
            C._triangles.Add(this);
            A.Mesh._triangles.Add(this);
        }

        public void DelinkPositionNormals()
        {
            A?._triangles.Remove(this);
            B?._triangles.Remove(this);
            C?._triangles.Remove(this);
            A?.Mesh._triangles.Remove(this);
            A = null;
            B = null;
            C = null;
        }

        public IEnumerable<PositionTriangle> GetABadjacents()
        {
            return A.Triangles.Intersect(B.Triangles).Where(t => t.Id != Id);
        }
        public IEnumerable<PositionTriangle> GetBCadjacents()
        {
            return B.Triangles.Intersect(C.Triangles).Where(t => t.Id != Id);
        }
        public IEnumerable<PositionTriangle> GetCAadjacents()
        {
            return C.Triangles.Intersect(A.Triangles).Where(t => t.Id != Id);
        }
        public IEnumerable<PositionTriangle> GetAverticies()
        {
            return A.Triangles.Where(t => t.Id != Id);
        }
        public IEnumerable<PositionTriangle> GetBverticies()
        {
            return B.Triangles.Where(t => t.Id != Id);
        }
        public IEnumerable<PositionTriangle> GetCverticies()
        {
            return C.Triangles.Where(t => t.Id != Id);
        }

        private Rectangle3D _box = null;
        public Rectangle3D Box
        {
            get
            {
                if (_box is null && Triangle is not null)
                {
                    _box = Triangle.Box.Margin(BoxBucket.MARGINS);
                }
                return _box;
            }
        }

        public IEnumerable<PositionNormal> Positions
        {
            get
            {
                yield return A;
                yield return B;
                yield return C;
            }
        }

        private Triangle3D _triangle = null;
        public Triangle3D Triangle
        {
            get
            {
                if (_triangle is null)
                {
                    _triangle = new Triangle3D(
                        A.Position,
                        B.Position,
                        C.Position);
                }
                return _triangle;
            }
        }

        public static SurfaceTriangle GetSurfaceTriangle(PositionTriangle triangle)
        {
            return new SurfaceTriangle(
                PositionNormal.GetRay(triangle.A),
                PositionNormal.GetRay(triangle.B),
                PositionNormal.GetRay(triangle.C));
        }

        public IEnumerable<Point3D> Points
        {
            get
            {
                yield return A.Position;
                yield return B.Position;
                yield return C.Position;
            }
        }
    }
}
