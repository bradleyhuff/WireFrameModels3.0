using Collections.WireFrameMesh.Basics;
using BasicObjects.GeometricObjects;
using BaseObjects.Transformations.Interfaces;
using Collections.Buckets;


namespace Collections.WireFrameMesh.Interfaces
{
    public interface IWireFrameMesh
    {
        public int Id { get; }
        public int Tag { get; set; }
        public IReadOnlyList<Position> Positions { get; }
        public IReadOnlyList<PositionTriangle> Triangles { get; }
        public PositionNormal AddPoint(Point3D? position);
        public PositionNormal AddPoint(Point3D? position, Vector3D? normal);
        public PositionNormal AddPoint(Point3D position, Vector3D normal, ITransform transform);
        public PositionTriangle AddTriangle(Point3D a, Point3D b, Point3D c, string trace, int tag);
        public PositionTriangle AddTriangle(Ray3D a, Ray3D b, Ray3D c, string trace, int tag);
        public PositionTriangle AddTriangle(Triangle3D triangle, string trace, int tag);
        public PositionTriangle AddTriangle(SurfaceTriangle triangle, string trace, int tag);
        public IEnumerable<PositionTriangle> AddRangeTriangles(IEnumerable<Triangle3D> triangles, string trace, int tag);
        public IEnumerable<PositionTriangle> AddRangeTriangles(IEnumerable<SurfaceTriangle> triangles, string trace, int tag);
        public IEnumerable<PositionTriangle> AddRangeTriangles(IEnumerable<PositionTriangle> triangles, string trace, int tag);
        public PositionTriangle AddTriangle(Point3D a, Vector3D aN, Point3D b, Vector3D bN, Point3D c, Vector3D cN, string trace, int tag);
        public PositionTriangle AddTriangle(PositionNormal a, PositionNormal b, PositionNormal c, string trace, int tag);
        public bool RemoveTriangle(Point3D a, Point3D b, Point3D c);
        public bool RemoveTriangle(PositionNormal a, PositionNormal b, PositionNormal c);
        public bool RemoveTriangle(PositionTriangle removalTriangle);
        public int RemoveAllTriangles(IEnumerable<PositionTriangle> removalTriangles);
        public IEnumerable<PositionTriangle> EndRow();
        public IEnumerable<PositionTriangle> EndGrid();
        public IEnumerable<PositionTriangle> AddGrid(IWireFrameMesh inputMesh);
        public void AddGrids(IEnumerable<IWireFrameMesh> grids);
        public IWireFrameMesh Clone();
        public IEnumerable<IWireFrameMesh> Clones(int number);
        public IWireFrameMesh CreateNewInstance();
        public void Apply(ITransform transform);
        public IWireFrameMesh Clone(ITransform transform);
    }
}
