using BaseObjects;
using Collections.Buckets;
using Operations.Basics;
using Operations.Intermesh.Basics;
using Operations.Intermesh.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Classes
{
    internal class FillIntermesh
    {
        internal static void Action(IEnumerable<IntermeshTriangle> triangles)
        {
            var start = DateTime.Now;
            var fillings = triangles.SelectMany(t => t.Fillings).ToArray();

            var bucket = new BoxBucket<FillTriangle>(fillings);
            int fillingsAdded = 0;
            foreach (var triangle in triangles)
            {
                var triangleFillings = new List<FillTriangle>();
                foreach (var fill in triangle.Fillings.Where(f => !f.Disabled))
                {
                    var matches = bucket.Fetch(fill).Where(f => f.Id != fill.Id && !f.Disabled);
                    var divisions = new List<FillTriangle>() { fill };

                    foreach (var match in matches)
                    {
                        var subDivisions = new List<FillTriangle>();
                        foreach(var division in divisions)
                        {
                            var coplanarDivisions = division.CoplanarDivideFrom(match);
                            fillingsAdded += (coplanarDivisions.Count() - 1);
                            subDivisions.AddRange(coplanarDivisions);
                        }
                        divisions = subDivisions;
                    }
                    triangleFillings.AddRange(divisions);
                }
                triangle.Fillings = triangleFillings;
            }

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Fill intermesh. Fillings added {fillingsAdded} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
