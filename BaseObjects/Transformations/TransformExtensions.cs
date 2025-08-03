using BaseObjects.Transformations.Interfaces;
using BasicObjects.GeometricObjects;

namespace BaseObjects.Transformations
{
    public static class TransformExtensions
    {
        public static T Translate<T>(this IShape3D<T> shape, Vector3D vector)
        {
            return Translate(shape, vector.X, vector.Y, vector.Z);
        }
        public static T Translate<T>(this IShape3D<T> shape, double x, double y, double z)
        {
            var t = Transformations.Transform.Translation(x, y, z);
            return shape.Constructor(shape.CardinalPoints.Select(t.Apply).ToArray(), shape.CardinalVectors.Select(t.Apply).ToArray());
        }

        public static T Rotate<T>(this IShape3D<T> shape, Vector3D vector, double angle)
        {
            return Rotate(shape, vector.X, vector.Y, vector.Z, angle);
        }
        public static T Rotate<T>(this IShape3D<T> shape, double x, double y, double z, double angle)
        {
            var t = Transformations.Transform.Rotation(x, y, z, angle);
            return shape.Constructor(shape.CardinalPoints.Select(t.Apply).ToArray(), shape.CardinalVectors.Select(t.Apply).ToArray());
        }

        public static T Scale<T>(this IShape3D<T> shape, double s)
        {
            return Scale(shape, s, s, s);
        }

        public static T Scale<T>(this IShape3D<T> shape, double x, double y, double z)
        {
            var t = Transformations.Transform.Scale(x, y, z);
            return shape.Constructor(shape.CardinalPoints.Select(t.Apply).ToArray(), shape.CardinalVectors.Select(t.Apply).ToArray());
        }

        public static T Reflect<T>(this IShape3D<T> shape, Vector3D planeNormal)
        {
            return Reflect(shape, planeNormal.X, planeNormal.Y, planeNormal.Z);
        }
        public static T Reflect<T>(this IShape3D<T> shape, double x, double y, double z)
        {
            var t = Transformations.Transform.Reflection(x, y, z);
            return shape.Constructor(shape.CardinalPoints.Select(t.Apply).ToArray(), shape.CardinalVectors.Select(t.Apply).ToArray());
        }
        public static T ShearXY<T>(this IShape3D<T> shape, double x, double y)
        {
            var t = Transformations.Transform.ShearXY(x, y);
            return shape.Constructor(shape.CardinalPoints.Select(t.Apply).ToArray(), shape.CardinalVectors.Select(t.Apply).ToArray());
        }
        public static T ShearYZ<T>(this IShape3D<T> shape, double y, double z)
        {
            var t = Transformations.Transform.ShearXY(y, z);
            return shape.Constructor(shape.CardinalPoints.Select(t.Apply).ToArray(), shape.CardinalVectors.Select(t.Apply).ToArray());
        }
        public static T ShearXZ<T>(this IShape3D<T> shape, double x, double z)
        {
            var t = Transformations.Transform.ShearXZ(x, z);
            return shape.Constructor(shape.CardinalPoints.Select(t.Apply).ToArray(), shape.CardinalVectors.Select(t.Apply).ToArray());
        }
        public static T Transform<T>(this IShape3D<T> shape, ITransform transformation)
        {
            return shape.Constructor(shape.CardinalPoints.Select(transformation.Apply).ToArray(), shape.CardinalVectors.Select(transformation.Apply).ToArray());
        }
    }
}
