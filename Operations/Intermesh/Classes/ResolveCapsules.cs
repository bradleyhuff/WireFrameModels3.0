using BaseObjects;
using Operations.Basics;
using Operations.Intermesh.Basics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Classes
{
    internal class ResolveCapsules
    {
        internal static void Action(IEnumerable<IntermeshTriangleOLD> intermeshTriangles)
        {
            DateTime start = DateTime.Now;
            //var perimeterDivisions = intermeshTriangles.SelectMany(i => i.PerimeterDivisions);
            //var internalDivisions = intermeshTriangles.SelectMany(i => i.InternalDivisions);
            //var capsules = perimeterDivisions.Select(p => new IntermeshCapsule(p, IntermeshCapsule.CapsuleType.Perimeter))
            //    .Concat(internalDivisions.Select(i => new IntermeshCapsule(i, IntermeshCapsule.CapsuleType.Internal))).ToArray();
            //ConsoleLog.WriteLine($"Perimeter divisions {perimeterDivisions.Count()} Internal divisions {internalDivisions.Count()}");


            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Resolve capsules. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }
    }
}
