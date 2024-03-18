using BasicObjects.GeometricObjects;
using Double = BasicObjects.Math.Double;

namespace Operations.Regions
{
    internal enum Region
    {
        Uninitialized,
        Indeterminant,
        Interior,
        Exterior,
        OnBoundary
    }

    internal static class Manifold
    {
        internal static Region GetRegion(Point3D point, Point3D[] intersections)
        {
            var directions = intersections.Select(i => (i - point).Direction);
            int countSameSide = 0;
            int countOppositeSide = 0;

            if (intersections.Any(p => p is not null && p == point)) { return Region.OnBoundary; }
            if (intersections.Any(p => p is null)) { return Region.Indeterminant; }

            if (directions.Any())
            {
                var firstDirection = directions.First();
                foreach (var direction in directions)
                {
                    if (!Double.IsEqual(direction.Magnitude, 1))
                    {
                        return Region.Indeterminant;
                    }
                    var parity = Math.Sign(Vector3D.Dot(firstDirection, direction));
                    if (parity == 0) { return Region.Indeterminant; }
                    var sameSide = parity > 0;
                    if (sameSide) { countSameSide++; } else { countOppositeSide++; }
                }
            }

            if (countSameSide % 2 == 0 && countOppositeSide % 2 == 0) { return Region.Exterior; }
            if (countSameSide % 2 == 1 && countOppositeSide % 2 == 1) { return Region.Interior; }
            return Region.Indeterminant;
        }
    }
}
