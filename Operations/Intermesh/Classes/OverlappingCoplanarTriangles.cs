using BaseObjects;
using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Collections.WireFrameMesh.Basics;
using FileExportImport;
using Operations.Basics;
using Operations.Diagnostics;
using Operations.Groupings.Types;
using Operations.Intermesh.Basics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Classes
{
    internal static class OverlappingCoplanarTriangles
    {
        private static int loop = 1;

        internal static void Action(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            if (loop > 0) { return; }

            DateTime start = DateTime.Now;

            ShowOverlaps(intermeshTriangles);
            DisabledOverlaps(intermeshTriangles);
            ShowOverlaps(intermeshTriangles);
            loop++;
            //BaseObjects.Console.WriteLine($"Fills: {fills.Length} Overlaps: {pairs.Count}  Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.", ConsoleColor.Yellow);


            //if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Overlapping fills  Fills: {fills.Length}  Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
            //if (!Mode.ThreadedRun) ConsoleLog.WriteLine($"Overlapping coplanar triangles  Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static void DisabledOverlaps(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var pairs = new Combination2Dictionary<(FillTriangle, FillTriangle)>();
            var fills = intermeshTriangles.SelectMany(t => t.Fillings).ToArray();
            var bucket = new BoxBucket<FillTriangle>(fills);
            //int overlaps = 0;
            foreach (var fill in fills)
            {
                var matches = bucket.Fetch(fill, 1e-6);
                foreach (var match in matches.Where(m => fill.Id != m.Id && fill.Key != m.Key))
                {
                    var test = Triangle3D.Overlaps(fill.Triangle, match.Triangle, 1e-9);
                    if (test)
                    {
                        var key = new Combination2(fill.Id, match.Id);
                        if (!pairs.ContainsKey(key)) { pairs[key] = (fill, match); }
                    }
                }
            }

            foreach (var pair in pairs)
            {
                var fill = pair.Value.Item1;
                var match = pair.Value.Item2;
                var pointGroup = fill.Key.Indicies.Concat(match.Key.Indicies).GroupBy(i => i);
                var sharedPoints = pointGroup.Where(g => g.Count() > 1).Select(g => g.Key);
                //if (sharedPoints.Count() == 2) { continue; }
                match.IsDisabled = true;
            }
        }

        private static void ShowOverlaps(IEnumerable<IntermeshTriangle> intermeshTriangles)
        {
            var pairs = new Combination2Dictionary<(FillTriangle, FillTriangle)>();
            var fills = intermeshTriangles.SelectMany(t => t.Fillings.Where(f => !f.IsDisabled)).ToArray();
            var bucket = new BoxBucket<FillTriangle>(fills);
            foreach (var fill in fills)
            {
                var matches = bucket.Fetch(fill, 1e-6);
                foreach (var match in matches.Where(m => fill.Id != m.Id && fill.Key != m.Key && !m.IsDisabled))
                {
                    var test = Triangle3D.Overlaps(fill.Triangle, match.Triangle, 1e-9);
                    if (test)
                    {
                        var key = new Combination2(fill.Id, match.Id);
                        if (!pairs.ContainsKey(key)) { pairs[key] = (fill, match); }
                    }
                }
            }

            foreach (var pair in pairs)
            {
                var fill = pair.Value.Item1;
                var match = pair.Value.Item2;
                var pointGroup = fill.Key.Indicies.Concat(match.Key.Indicies).GroupBy(i => i);
                var sharedPoints = pointGroup.Where(g => g.Count() > 1).Select(g => g.Key);
                //if (sharedPoints.Count() == 2) { continue; }
                BaseObjects.Console.WriteLine(
                    ($"Overlap {pair.Key}", ConsoleColor.Cyan),
                    ($"  {fill.Parent.Id} => {fill.Id} {fill.Key} Max Length: {fill.Triangle.MaxEdge.Length.ToString("E2")} Max Height: {fill.Triangle.MinHeight.ToString("E2")} Aspect: {fill.Triangle.AspectRatio.ToString("E2")}", ConsoleColor.Yellow),
                    ($"  {match.Parent.Id} => {match.Id} {match.Key} Max Length: {match.Triangle.MaxEdge.Length.ToString("E2")} Max Height: {match.Triangle.MinHeight.ToString("E2")} Aspect: {match.Triangle.AspectRatio.ToString("E2")}", ConsoleColor.Green),
                    ($" Shared [{string.Join(",", sharedPoints)}] Parent coplanar {Triangle3D.AreCoplanar(fill.Parent.Triangle, match.Parent.Triangle, 1e-9)}", ConsoleColor.Cyan));
            }
            BaseObjects.Console.WriteLine($"Fills: {fills.Length} Overlaps: {pairs.Count}", ConsoleColor.Yellow);
        }
    }
}
