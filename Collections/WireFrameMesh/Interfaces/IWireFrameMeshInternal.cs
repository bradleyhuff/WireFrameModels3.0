using Collections.WireFrameMesh.Basics;

namespace Collections.WireFrameMesh.Interfaces
{
    internal interface IWireFrameMeshInternal
    {
        public int Id { get; }
        public void IncrementMark();
        public int Mark { get; }
        bool AddNewTriangle(PositionTriangle triangle);
    }
}
