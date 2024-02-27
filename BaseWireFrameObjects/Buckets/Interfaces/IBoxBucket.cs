using BasicObjects.GeometricObjects;

namespace BaseWireFrameObjects.Buckets.Interfaces
{
    public interface IBoxBucket<T> where T: IBox
    {
        T[] BoxNodes { get; }
        T[] ContainedBoxNodes { get; }
        Rectangle3D Box { get; }
        Rectangle3D CenterBox { get; }
        int Level { get; }
        IBoxBucket<T> CreateInstance(T[] boxNodes, Rectangle3D box, int level);

        T[] Fetch(T input);
        T[] Fetch<G>(G input) where G : IBox;
        T[] Fetch(Rectangle3D box);
        T[] RawFetch(Rectangle3D box);
        void Profile(string label);
    }
}
