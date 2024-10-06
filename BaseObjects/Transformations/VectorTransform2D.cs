using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseObjects.Transformations
{
    public static class VectorTransform2D
    {
        public static IEnumerable<Vector2D> BarycentricArc(Vector2D start, Vector2D end, int steps)
        {
            var theta = Vector2D.Angle(start, end);
            var D = Math.Sqrt(2 - 2 * Math.Cos(theta));

            for (int i = 0; i <= steps; i++)
            {
                double alpha = (double)i / steps;
                double angle = theta * alpha;
                double beta = Math.Sin(angle) / (D * Math.Sin(Math.PI / 2 + theta / 2 - angle));

                yield return ((1 - beta) * start + beta * end).Direction;
            }
        }
    }
}
