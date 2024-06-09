using Collections.WireFrameMesh.Basics;
using BasicObjects.GeometricObjects;
using BaseObjects.Transformations.Interfaces;
using Collections.Buckets;


namespace Collections.WireFrameMesh.Interfaces
{
    public interface IWireFrameMesh
    {
        public int Id { get; }
        public IReadOnlyList<Position> Positions { get; }
        public IReadOnlyList<PositionTriangle> Triangles { get; }
        public PositionNormal AddPoint(Point3D? position);
        public PositionNormal AddPoint(Point3D? position, Vector3D? normal);
        public PositionNormal AddPoint(Point3D position, Vector3D normal, ITransform transform);
        public PositionTriangle AddTriangle(Point3D a, Point3D b, Point3D c, string trace = "");
        public PositionTriangle AddTriangle(Triangle3D triangle, string trace = "");
        public IEnumerable<PositionTriangle> AddRangeTriangles(IEnumerable<Triangle3D> triangles, string trace = "");
        public IEnumerable<PositionTriangle> AddRangeTriangles(IEnumerable<SurfaceTriangle> triangles, string trace = "");
        public PositionTriangle AddTriangle(Point3D a, Vector3D aN, Point3D b, Vector3D bN, Point3D c, Vector3D cN, string trace = "");
        public PositionTriangle AddTriangle(PositionNormal a, PositionNormal b, PositionNormal c, string trace = "");
        public bool RemoveTriangle(Point3D a, Point3D b, Point3D c);
        public bool RemoveTriangle(PositionNormal a, PositionNormal b, PositionNormal c);
        public bool RemoveTriangle(PositionTriangle removalTriangle);
        public int RemoveAllTriangles(IEnumerable<PositionTriangle> removalTriangles);
        public void EndRow();
        public void EndGrid();
        public void AddGrid(IWireFrameMesh inputMesh);
        public void AddGrids(IEnumerable<IWireFrameMesh> grids);
        public IWireFrameMesh Clone();
        public IEnumerable<IWireFrameMesh> Clones(int number);
        public IWireFrameMesh CreateNewInstance();
        public void Apply(ITransform transform);
        public IWireFrameMesh Clone(ITransform transform);

        //public static IWireFrameMesh operator +(IWireFrameMesh a, IWireFrameMesh b)
        //{
        //    return a;
        //}
    }
}
