using BasicObjects.GeometricObjects;

namespace BaseObjects.Transformations.Interfaces
{
    public interface IShape3D<T>
    {
        Point3D[] CardinalPoints { get; }
        Vector3D[] CardinalNormals { get; }
        T Constructor(Point3D[] cardinalPoints, Vector3D[] cardinalNormals);
    }
}
