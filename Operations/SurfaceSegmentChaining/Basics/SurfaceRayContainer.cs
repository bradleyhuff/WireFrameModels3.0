using BasicObjects.GeometricObjects;

namespace Operations.SurfaceSegmentChaining.Basics
{
    internal class SurfaceRayContainer<T> : Ray3D
    {
        public SurfaceRayContainer(Ray3D ray, Vector3D triangleNormal, int index, T reference) : base(ray.Point, ray.Normal)
        {
            Reference = reference;
            Index = index;
            TriangleNormal = triangleNormal;
        }
        public T Reference { get; }
        public Vector3D TriangleNormal { get; }
        public int Index { get; }
    }
}
