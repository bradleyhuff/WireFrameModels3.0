using BaseObjects;
using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Operations.Intermesh.Basics;

namespace Operations.Intermesh.Classes
{
    internal class FillOverlapRemoval
    {
        internal static void Action(IEnumerable<IntermeshTriangle> triangles)
        {
            var start = DateTime.Now;

            var appliedTriangles = new BoxBucket<IntermeshTriangle>();
            var fillsDisabled = 0;

            foreach (var triangle in triangles)
            {
                foreach(var fill in triangle.Fillings)
                {
                    if (fill.Disabled) { continue; }

                    var testPoint = fill.Triangle.Center;
                    var matches = appliedTriangles.Fetch(new Rectangle3D(testPoint, BoxBucket.MARGINS));
                    var alreadyCovered = matches.Any(m => m.Triangle.PointIsIn(testPoint));
                    fill.Disabled = alreadyCovered;
                    if (alreadyCovered) { fillsDisabled++; }
                }

                appliedTriangles.Add(triangle);
            }

            ConsoleLog.WriteLine($"Fill overlap removal. Fills disabled {fillsDisabled} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
