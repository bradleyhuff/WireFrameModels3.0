using Collections.WireFrameMesh.Basics;

namespace Collections.WireFrameMesh.Interfaces
{
    internal interface IWireFrameMeshInternal
    {
        int Id { get; }
        void IncrementMark();
        int Mark { get; }
        bool AddNewTriangle(PositionTriangle triangle);
        PositionTriangle AddTriangle(PositionNormal a, PositionNormal b, PositionNormal c, string trace, int tag);
        int RemoveAllTriangles(IEnumerable<PositionTriangle> removalTriangles);
    }
}
