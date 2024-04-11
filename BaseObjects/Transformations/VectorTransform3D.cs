using BasicObjects.GeometricObjects;

namespace BaseObjects.Transformations
{
    public static class VectorTransform3D
    {
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
                yield return Transform.Rotation(cross, stepAngle).Apply(start);
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
                yield return Transform.Rotation(cross, stepAngle).Apply(start);
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
