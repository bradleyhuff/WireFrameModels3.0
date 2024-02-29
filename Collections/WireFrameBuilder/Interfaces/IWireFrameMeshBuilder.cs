using Collections.WireFrameBuilder.Basics;
using BasicObjects.GeometricObjects;


namespace Collections.WireFrameBuilder.Interfaces
{
    public interface IWireFrameMeshBuilder
    {
        public IReadOnlyList<PositionNormal> PositionNormals { get; }
        public IReadOnlyList<PositionTriangle> Triangles { get; }
        public PositionNormal CreateAndAdd();
        public PositionNormal AddPointNoRow(Point3D? position, Vector3D? normal);
        public PositionNormal AddPoint(Point3D? position, Vector3D? normal);
        public PositionNormal AddPoint(Point3D? position, Vector3D? normal, int number);
        public int RemoveTriangles(IEnumerable<Triangle3D> triangles);
        internal void AddTriangle(PositionTriangle triangle);
        internal void AddTriangleRange(IEnumerable<PositionTriangle> triangles);       
        internal void SetTriangles(IEnumerable<PositionTriangle> triangles);
        public void EndRow();
        public void EndGrid();
        public void AddGrid(IWireFrameMeshBuilder inputMesh);
        public void AddGrids(IEnumerable<IWireFrameMeshBuilder> grids);
        public IWireFrameMeshBuilder Clone();
        public IEnumerable<IWireFrameMeshBuilder> Clones(int number);
        public IWireFrameMeshBuilder CreateNewInstance();
    }
}
