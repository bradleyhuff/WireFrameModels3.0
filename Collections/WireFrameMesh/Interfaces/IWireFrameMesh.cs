using Collections.WireFrameMesh.Basics;
using BasicObjects.GeometricObjects;
using BaseObjects.Transformations;
using BaseObjects.Transformations.Interfaces;


namespace Collections.WireFrameMesh.Interfaces
{
    public interface IWireFrameMesh
    {
        public IReadOnlyList<Position> Positions { get; }
        public IReadOnlyList<PositionTriangle> Triangles { get; }
        public PositionNormal AddPoint(Point3D? position);
        public PositionNormal AddPoint(Point3D? position, Vector3D? normal);
        public PositionNormal AddPoint(Point3D position, Vector3D normal, ITransform transform);
        public PositionTriangle AddTriangle(Point3D a, Point3D b, Point3D c, string trace = "");
        public PositionTriangle AddTriangle(Triangle3D triangle, string trace = "");
        public PositionTriangle AddTriangle(Point3D a, Vector3D aN, Point3D b, Vector3D bN, Point3D c, Vector3D cN, string trace = "");
        public void EndRow();
        public void EndGrid();
        public void AddGrid(IWireFrameMesh inputMesh);
        public void AddGrids(IEnumerable<IWireFrameMesh> grids);
        public IWireFrameMesh Clone();
        public IEnumerable<IWireFrameMesh> Clones(int number);
        public IWireFrameMesh CreateNewInstance();
        public void Apply(ITransform transform);
    }
}
