using BasicObjects.GeometricObjects;

namespace BaseObjects.Transformations.Old
{
    public static class VectorTransform3D
    {
        public static Transform3x3 RotationTransform(Vector3D axis, double angle)
        {
            Matricies.AxisRotation3x3(axis.X, axis.Y, axis.Z, angle, out double m00, out double m01, out double m02, out double m10, out double m11, out double m12, out double m20, out double m21, out double m22);

            return new Transform3x3() { m00 = m00, m01 = m01, m02 = m02, m10 = m10, m11 = m11, m12 = m12, m20 = m20, m21 = m21, m22 = m22 };
        }
        public static Vector3D Transform(Vector3D basis, Transform3x3 transform)
        {
            Matricies.TransformVector3x3(
                transform.m00, transform.m01, transform.m02, transform.m10, transform.m11, transform.m12, transform.m20, transform.m21, transform.m22,
                basis.X, basis.Y, basis.Z, out double tx, out double ty, out double tz);

            return new Vector3D(tx, ty, tz);
        }
        //public static Vector3D Rotate(this Vector3D vector, Vector3D axis, double angle)
        //{
        //    double m00, m01, m02, m10, m11, m12, m20, m21, m22;
        //    Matricies.AxisRotation3x3(axis.X, axis.Y, axis.Z, angle, out m00, out m01, out m02, out m10, out m11, out m12, out m20, out m21, out m22);
        //    double tx, ty, tz;
        //    Matricies.TransformVector3x3(m00, m01, m02, m10, m11, m12, m20, m21, m22, vector.X, vector.Y, vector.Z, out tx, out ty, out tz);

        //    return new Vector3D(tx, ty, tz);
        //}

        public static Vector3D Rotate(this Vector3D vector, Vector3D axis, double angle)
        {
            double[] rotation = Matricies.AxisRotation(new double[] { axis.X, axis.Y, axis.Z, 0 }, angle);
            double[] vector2 = Matricies.TransformVector(rotation, new double[] { vector.X, vector.Y, vector.Z, 0 });
            return new Vector3D(vector2[0], vector2[1], vector2[2]);
        }

        public static IEnumerable<Vector3D> Rotate(IEnumerable<Vector3D> basisArray, Vector3D axis, double angle)
        {
            Matricies.AxisRotation3x3(axis.X, axis.Y, axis.Z, angle, out double m00, out double m01, out double m02, out double m10, out double m11, out double m12, out double m20, out double m21, out double m22);

            foreach (var basis in basisArray)
            {
                Matricies.TransformVector3x3(m00, m01, m02, m10, m11, m12, m20, m21, m22, axis.X, axis.Y, axis.Z, out double tx, out double ty, out double tz);

                yield return new Vector3D(tx, ty, tz);
            }
        }

        public static IEnumerable<Vector3D> Arc(Vector3D start, Vector3D end, double maxSteppingAngle)
        {
            double angle = Vector3D.Angle(start, end);
            start = start.Direction;
            end = end.Direction;
            Vector3D cross = Vector3D.Cross(start, end).Direction;
            int steps = (int)Math.Ceiling(angle / maxSteppingAngle);
            for (int i = 0; i <= steps; i++)
            {
                double stepAngle = angle * i / steps;
                yield return start.Rotate(cross, stepAngle);
            }

        }

        public static IEnumerable<Vector3D> Arc(Vector3D start, Vector3D end, int steps)
        {
            double angle = Vector3D.Angle(start, end);
            start = start.Direction;
            end = end.Direction;
            Vector3D cross = Vector3D.Cross(start, end).Direction;
            for (int i = 0; i <= steps; i++)
            {
                double stepAngle = angle * i / steps;
                yield return start.Rotate(cross, stepAngle);
            }
        }

        public static IEnumerable<Vector3D[]> Steradian(Vector3D pole, Vector3D surfaceStart, Vector3D surfaceEnd, double maxSteppingAngle)
        {
            double angle = Vector3D.Angle(pole, surfaceStart);

            int lateralSteps = (int)Math.Ceiling(angle / maxSteppingAngle);
            return Steradian(pole, surfaceStart, surfaceEnd, lateralSteps, maxSteppingAngle);
        }

        public static IEnumerable<Vector3D[]> Steradian(Vector3D pole, Vector3D surfaceStart, Vector3D surfaceEnd, int lateralSteps, double maxSteppingAngle)
        {
            var surfaceStartArc = Arc(pole, surfaceStart, lateralSteps).ToArray();
            var surfaceEndArc = Arc(pole, surfaceEnd, lateralSteps).ToArray();

            yield return new[] { pole };

            for (int i = 1; i <= lateralSteps; i++)
            {
                yield return Arc(surfaceStartArc[i], surfaceEndArc[i], maxSteppingAngle).ToArray();
            }
        }
    }
}
