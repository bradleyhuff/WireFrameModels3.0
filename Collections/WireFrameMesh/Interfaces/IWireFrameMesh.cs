using Collections.WireFrameMesh.Basics;
using BasicObjects.GeometricObjects;


namespace Collections.WireFrameMesh.Interfaces
{
    public interface IWireFrameMesh
    {
        public IReadOnlyList<Position> Positions { get; }
        public IReadOnlyList<PositionTriangle> Triangles { get; }
        public PositionNormal AddPointNoRow(Point3D? position, Vector3D? normal);
        public PositionNormal AddPoint(Point3D? position, Vector3D? normal);
        public int RemoveTriangles(IEnumerable<Triangle3D> triangles);
        public void AddTriangle(PositionTriangle triangle);
        public void AddTriangleRange(IEnumerable<PositionTriangle> triangles);       
        public void EndRow();
        public void EndGrid();
        public void AddGrid(IWireFrameMesh inputMesh);
        public void AddGrids(IEnumerable<IWireFrameMesh> grids);
        public IWireFrameMesh Clone();
        public IEnumerable<IWireFrameMesh> Clones(int number);
        public IWireFrameMesh CreateNewInstance();
    }
}
