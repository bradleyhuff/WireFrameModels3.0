using BaseObjects;
using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using Operations.Basics;
using Operations.Intermesh.Basics;
using Console = BaseObjects.Console;

namespace Operations.Intermesh.Classes
{
    internal class FillOverlapRemoval
    {
        private static int count = 0;
        private static int count2 = 0;
        private static int example = 0;
        private static int example2 = 0;
        private static object lockObject = new object();
        internal static void Action(IEnumerable<IntermeshTriangle> triangles)
        {
            var start = DateTime.Now;

            var fillsDisabled = 0;

            var fillings = triangles.SelectMany(t => t.Fillings).ToArray();
            var grouping = new GroupingDictionary<int, List<FillTriangle>>(() => new List<FillTriangle>());

            var bucket = new BoxBucket<FillTriangle>(fillings);
            var intersectedFillings = new List<FillTriangle>();
            foreach(var filling in fillings)
            {
                var matches = bucket.Fetch(filling).Where(f => TrianglesInterferance(f.Triangle, filling.Triangle));
                if (matches.Any())
                {
                    grouping[filling.ParentIntermesh.Id].Add(filling);
                    intersectedFillings.Add(filling);
                }
            }

            foreach (var filling in intersectedFillings) { filling.Disabled = true; }

            var bucket2 = new BoxBucket<FillTriangle>(intersectedFillings);
            foreach(var filling in intersectedFillings)
            {
                var matches = bucket2.Fetch(filling).Where(f => f.Id != filling.Id && f.ParentIntermesh.Id != filling.ParentIntermesh.Id && TrianglesInterferance(f.Triangle, filling.Triangle));
                if (!matches.Any(m => !m.Disabled)) { filling.Disabled = false; }
            }

            if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Fill overlap removal. Fills disabled {fillsDisabled} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static bool TrianglesInterferance(Triangle3D a, Triangle3D b)
        {
            return Triangle3D.Intersects(a, b) || (Triangle3D.Overlaps(a, b) && !a.Equals(b));
        }
    }
}
