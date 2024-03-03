using BasicObjects.GeometricObjects;

namespace Collections.Buckets.Interfaces
{
    internal interface IBoxBucketInternal<T> where T: IBox
    {
        List<T> BoxNodes { get; }
        Rectangle3D Box { get; }
        Rectangle3D CenterBox { get; }
        IBoxBucketInternal<T> CreateInstance(Rectangle3D box, IEnumerable<T> boxNodes);
        List<T> Fetch(Rectangle3D box);
        void Add(T box);
        void AddRange(IEnumerable<T> boxes);
    }
}
