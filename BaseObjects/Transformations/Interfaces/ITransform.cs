using BasicObjects.GeometricObjects;

namespace BaseObjects.Transformations.Interfaces
{
    public interface ITransform
    {
        Point3D Apply(Point3D point);
        Vector3D Apply(Vector3D normal);
        Triangle3D Apply(Triangle3D normal);
    }
}
