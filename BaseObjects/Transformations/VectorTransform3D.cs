using BasicObjects.GeometricObjects;
using E = BasicObjects.Math;

namespace BaseObjects.Transformations
{
    public static class VectorTransform3D
    {
        public static IEnumerable<Vector3D> BarycentricArc(Vector3D start, Vector3D end, int steps)
        {
            var theta = Vector3D.Angle(start, end);
            var D = Math.Sqrt(2 - 2 * Math.Cos(theta));

            for (int i = 0; i <= steps; i++)
            {
                double alpha = (double)i / steps;
                double angle = theta * alpha;
                double beta = Math.Sin(angle) / (D * Math.Sin(Math.PI / 2 + theta / 2 - angle));

                yield return ((1 - beta) * start + beta * end).Direction;
            }
        }

        public static IEnumerable<Vector3D> BarycentricArc2(Vector3D start, Vector3D end, int steps)
        {
            var theta = Vector3D.Angle(start, end);
            var D = Math.Sqrt(2 - 2 * Math.Cos(theta));

            for (int i = 0; i <= steps; i++)
            {
                double alpha = (double)i / steps;
                double angle = theta * alpha;
                double beta = Math.Sin(angle) / (D * Math.Sin(Math.PI / 2 + theta / 2 - angle));
                
                yield return (start.Magnitude * (1 - beta) * start.Direction + end.Magnitude * beta * end.Direction);
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

        public static IEnumerable<Vector2D> BarycentricArcWeights(Vector3D start, Vector3D end, int steps)
        {
            var theta = Vector3D.Angle(start.Direction, end.Direction);
            var D = Math.Sqrt(2 - 2 * Math.Cos(theta));
            var cross = Vector3D.Cross(start, end).Direction;

            for (int i = 0; i <= steps; i++)
            {
                double alpha = (double)i / steps;
                double angle = theta * alpha;
                double beta = Math.Sin(angle) / (D * Math.Sin(Math.PI / 2 + theta / 2 - angle));

                var vector = ((1 - beta) * start + beta * end).Direction;
                E.LinearSystems.Solve3x3(
                    start.Direction.X, end.Direction.X, cross.X,
                    start.Direction.Y, end.Direction.Y, cross.Y,
                    start.Direction.Z, end.Direction.Z, cross.Z,
                    vector.X, vector.Y, vector.Z,
                    out double x, out double y, out double z);

                yield return new Vector2D(vector.X, vector.Y);
            }
        }

        public static Vector3D[][] BarycentricSteradianWeights(Vector3D n0, Vector3D n1, Vector3D n2, int steps)
        {
            var plot = BarycentricSteradian(n0.Direction, n1.Direction, n2.Direction, steps);
            var output = new Vector3D[plot.Length][];

            for (int i = 0; i < plot.Length; i++)
            {
                var row = plot[i];
                var rowOutput = new Vector3D[row.Length];
                for (int j = 0; j < row.Length; j++)
                {
                    var col = row[j];

                    E.LinearSystems.Solve3x3(
                        n0.Direction.X, n1.Direction.X, n2.Direction.X,
                        n0.Direction.Y, n1.Direction.Y, n2.Direction.Y,
                        n0.Direction.Z, n1.Direction.Z, n2.Direction.Z,
                        col.X, col.Y, col.Z,
                        out double x, out double y, out double z);

                    rowOutput[j] = new Vector3D(x, y, z);
                }
                output[i] = rowOutput;
            }
            return output;
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
    }
}
