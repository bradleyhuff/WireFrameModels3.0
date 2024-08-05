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

        public static IEnumerable<Vector3D> BarycentricArc(Vector3D start, Vector3D end, int steps)
        {
            var theta = Vector3D.Angle(start, end);
            var D = Math.Sqrt(2 - 2 * Math.Cos(theta));

            for (int i = 0; i <= steps; i++)
            {
                double alpha = (double)i / steps;
                double angle = theta * alpha;
                double beta = Math.Sin(angle) /(D * Math.Sin(Math.PI/2  + theta/2 - angle));

                yield return ((1 - beta) * start + beta * end).Direction;
            }
        }

        public static Vector3D[][] BarycentricSteradian(Vector3D n0, Vector3D n1, Vector3D n2, int steps)
        {
            var triangle = BuildVectorTriangle(steps, n0, n1, n2);
            BarycentricArc(triangle.Select(r => r[0]).ToArray());
            BarycentricArc(triangle.Select(r => r[r.Length - 1]).ToArray());
            foreach (var row in triangle)
            {
                BarycentricArc(row);
            }

            return triangle.Select(r => r.Select(t => t.Vector).ToArray()).ToArray();
        }

        private static VectorContainer[][] BuildVectorTriangle(int n, Vector3D n0, Vector3D n1, Vector3D n2)
        {
            var triangle = new VectorContainer[n + 1][];

            for (int i = 0; i <= n; i++)
            {
                triangle[i] = new VectorContainer[n - i + 1];
                for (int j = 0; j < triangle[i].Length; j++) { triangle[i][j] = new VectorContainer(); }
            }

            triangle[0][0].Vector = n0.Direction;
            triangle[0][n].Vector = n1.Direction;
            triangle[n][0].Vector = n2.Direction;

            return triangle;
        }

        private class VectorContainer
        {
            public Vector3D Vector { get; set; }
        }

        private static void BarycentricArc(VectorContainer[] row)
        {
            var steps = row.Length;
            var start = row[0].Vector.Direction;
            var end = row[row.Length - 1].Vector.Direction;

            var theta = Vector3D.Angle(start, end);
            var D = Math.Sqrt(2 - 2 * Math.Cos(theta));

            for (int i = 1; i < steps - 1; i++)
            {
                double alpha = (double)i / (steps - 1);
                double angle = theta * alpha;
                double beta = Math.Sin(angle) / (D * Math.Sin(Math.PI / 2 + theta / 2 - angle));
                var interpolation = (1 - beta) * start + beta * end;

                row[i].Vector = interpolation.Direction;
            }
        }


        //public static IEnumerable<Vector3D[]> Steradian(Vector3D pole, Vector3D surfaceStart, Vector3D surfaceEnd, double maxSteppingAngle)
        //{
        //    double angle = Vector3D.Angle(pole, surfaceStart);

        //    int lateralSteps = (int)Math.Ceiling(angle / maxSteppingAngle);
        //    return Steradian(pole, surfaceStart, surfaceEnd, lateralSteps, maxSteppingAngle);
        //}

        //public static IEnumerable<Vector3D[]> Steradian(Vector3D pole, Vector3D surfaceStart, Vector3D surfaceEnd, int lateralSteps, double maxSteppingAngle)
        //{
        //    var surfaceStartArc = Arc(pole, surfaceStart, lateralSteps).ToArray();
        //    var surfaceEndArc = Arc(pole, surfaceEnd, lateralSteps).ToArray();

        //    yield return new[] { pole };

        //    for (int i = 1; i <= lateralSteps; i++)
        //    {
        //        yield return Arc(surfaceStartArc[i], surfaceEndArc[i], maxSteppingAngle).ToArray();
        //    }
        //}

        //public static IEnumerable<Vector3D[]> Steradian(Vector3D pole, Vector3D surfaceStart, Vector3D surfaceEnd, int lateralSteps, int steps)
        //{
        //    var surfaceStartArc = Arc(pole, surfaceStart, lateralSteps).ToArray();
        //    var surfaceEndArc = Arc(pole, surfaceEnd, lateralSteps).ToArray();

        //    yield return new[] { pole };

        //    for (int i = 1; i <= lateralSteps; i++)
        //    {
        //        yield return Arc(surfaceStartArc[i], surfaceEndArc[i], steps).ToArray();
        //    }
        //}
    }
}
