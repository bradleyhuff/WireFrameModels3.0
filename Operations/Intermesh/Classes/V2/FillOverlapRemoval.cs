using BaseObjects;
using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Operations.Intermesh.Basics.V2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Classes.V2
{
    internal class FillOverlapRemoval
    {
        internal static void Action(IEnumerable<IntermeshTriangle> triangles)
        {
            var start = DateTime.Now;

            var appliedTriangles = new BoxBucket<IntermeshTriangle>();

            foreach (var triangle in triangles)
            {
                foreach(var fill in triangle.Fillings)
                {
                    if (fill.Disabled) { continue; }

                    var testPoint = fill.Triangle.Center;
                    var matches = appliedTriangles.Fetch(new Rectangle3D(testPoint, BoxBucket.MARGINS));
                    var alreadyCovered = matches.Any(m => m.Triangle.PointIsIn(testPoint));
                    //if (alreadyCovered) { BaseObjects.Console.WriteLine("Already covered"); }
                    fill.Disabled = alreadyCovered;
                }

                appliedTriangles.Add(triangle);
            }

            ConsoleLog.WriteLine($"Fill overlap removal. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
