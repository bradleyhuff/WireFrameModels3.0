using BasicObjects.GeometricObjects;

namespace Operations.SurfaceSegmentChaining.Basics
{
    internal class SurfaceRayContainer<T> : Ray3D
    {
        public SurfaceRayContainer(Ray3D ray) : base(ray.Point, ray.Normal)
        {
        }
        public SurfaceRayContainer(Ray3D ray, int index, T reference) : base(ray.Point, ray.Normal)
        {
            Reference = reference;
            Index = index;
        }
        public T Reference { get; }
        public int Index { get; }
    }
}
