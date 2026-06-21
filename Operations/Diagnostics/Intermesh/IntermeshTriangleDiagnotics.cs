using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using FileExportImport;
using Operations.Intermesh.Basics;
using System.Collections.Generic;

namespace Operations.Diagnostics
{
    internal static class IntermeshTriangleDiagnotics
    {
        internal static void Show(this Operations.Intermesh.Basics.IntermeshTriangle triangle)
        {
            var errorTable = new Dictionary<int, (bool A, bool B)>();

            var perimeterCapsules = triangle.PerimeterSlots.SelectMany(s => s.Segments.SelectMany(ss => ss.Capsules)).ToArray();
            SetErrors(perimeterCapsules[perimeterCapsules.Length - 1], perimeterCapsules[0], errorTable);
            for (int i = 1; i < perimeterCapsules.Length - 1; i++)
            {
                SetErrors(perimeterCapsules[i - 1], perimeterCapsules[i], errorTable);
            }

            foreach (var slot in triangle.IntersectionSlots)
            {
                var capsules = slot.Segments.SelectMany(s => s.Capsules).ToArray();
                for (int i = 1; i < capsules.Length - 1; i++)
                {
                    SetErrors(capsules[i - 1], capsules[i], errorTable);
                }
            }

            BaseObjects.Console.Write($"AB: {triangle.AB.Id, 6:#####0} ", ConsoleColor.Green);
            triangle.AB.Show(errorTable);
            BaseObjects.Console.Write($"BC: {triangle.BC.Id, 6:#####0} ", ConsoleColor.Green);
            triangle.BC.Show(errorTable);
            BaseObjects.Console.Write($"CA: {triangle.CA.Id, 6:#####0} ", ConsoleColor.Green);
            triangle.CA.Show(errorTable);
            foreach (var intersectionSlot in triangle.IntersectionSlots)
            {
                BaseObjects.Console.Write($"I:  {intersectionSlot.Id, 6:#####0} ", ConsoleColor.Green);
                intersectionSlot.Show(errorTable);
            }

        }

        internal static void SetErrors(IntermeshCapsule a, IntermeshCapsule b, Dictionary<int, (bool A, bool B)> errorTable)
        {
            var capsuleAId = a.Id;
            var capsuleBId = b.Id;

            if (a.B.Id != b.A.Id)
            {
                if (!errorTable.ContainsKey(capsuleAId)) { errorTable[capsuleAId] = (true, true); }
                if (!errorTable.ContainsKey(capsuleBId)) { errorTable[capsuleBId] = (true, true); }

                errorTable[capsuleAId] = (errorTable[capsuleAId].A, false);
                errorTable[capsuleBId] = (false, errorTable[capsuleBId].B);              
            }
        }

        internal static void Show(this IntermeshEdgeSlot slot, Dictionary<int, (bool A, bool B)> errorTable)
        {
            if (!slot.Segments.Any()) { BaseObjects.Console.WriteLine("[]", ConsoleColor.Green); return; }

            int length = slot.Segments.Count();
            if (length == 1)
            {
                var first = slot.Segments[0];
                BaseObjects.Console.Write("[", ConsoleColor.Green);
                first.Show(errorTable, true, false);
                BaseObjects.Console.WriteLine("]", ConsoleColor.Green);
                return;
            }

            {
                var first = slot.Segments.First();
                BaseObjects.Console.Write("[", ConsoleColor.Green);
                first.Show(errorTable, true, true);
            }

            foreach (var segment in slot.Segments.Skip(1).Take(length - 2))
            {
                segment.Show(errorTable, false, true);
            }
            {
                var last = slot.Segments.Last();
                last.Show(errorTable, false, false);
                BaseObjects.Console.WriteLine("]", ConsoleColor.Green);
            }
        }

        internal static void Show(this IntermeshSegment segment, Dictionary<int, (bool A, bool B)> errorTable, bool isFirst, bool nextLine)
        {
            if (!isFirst) BaseObjects.Console.Write($"            ", ConsoleColor.Yellow);
            if (!segment.Capsules.Any())
            {
                if (nextLine) BaseObjects.Console.WriteLine($"[]", ConsoleColor.Yellow); else BaseObjects.Console.Write("[]", ConsoleColor.Yellow);
                return;
            }

            int length = segment.Capsules.Count();
            if (length == 1)
            {
                var first = segment.Capsules[0];
                BaseObjects.Console.Write($"[", ConsoleColor.Yellow);
                if (errorTable.ContainsKey(first.Id)) first.Show(errorTable[first.Id]); else first.Show((true, true));
                BaseObjects.Console.Write("]", ConsoleColor.Yellow);
                if (nextLine) BaseObjects.Console.WriteLine();
                return;
            }
            {
                var first = segment.Capsules.First();
                BaseObjects.Console.Write($"[", ConsoleColor.Yellow);
                if (errorTable.ContainsKey(first.Id)) first.Show(errorTable[first.Id]); else first.Show((true, true));
                BaseObjects.Console.WriteLine();
            }
            foreach (var capsule in segment.Capsules.Skip(1).Take(length - 2))
            {
                BaseObjects.Console.Write($"             ", ConsoleColor.Cyan);
                if (errorTable.ContainsKey(capsule.Id)) capsule.Show(errorTable[capsule.Id]); else capsule.Show((true, true));
                BaseObjects.Console.WriteLine();
            }
            {
                var last = segment.Capsules.Last();
                BaseObjects.Console.Write($"             ", ConsoleColor.Cyan);
                if (errorTable.ContainsKey(last.Id)) last.Show(errorTable[last.Id]); else last.Show((true, true));
                BaseObjects.Console.Write("]", ConsoleColor.Yellow);
                if (nextLine) BaseObjects.Console.WriteLine();
            }
        }

        internal static void Show(this IntermeshCapsule capsule, (bool A, bool B) check)
        {
            BaseObjects.Console.Write($"{{", ConsoleColor.Cyan);
            BaseObjects.Console.Write($"{capsule.A.Id,6:#####0}", ConsoleColor.Cyan, !check.A ? ConsoleColor.DarkRed : System.Console.BackgroundColor);
            BaseObjects.Console.Write($", ", ConsoleColor.Cyan);
            BaseObjects.Console.Write($"{capsule.B.Id,6:#####0}", ConsoleColor.Cyan, !check.B ? ConsoleColor.DarkRed : System.Console.BackgroundColor);
            BaseObjects.Console.Write($"}}", ConsoleColor.Cyan);
            BaseObjects.Console.Write($" {capsule.Segment.Length.ToString("E2")} ", capsule.Segment.Length < GapConstants.Resolver ? ConsoleColor.Yellow : ConsoleColor.Gray);
        }


        internal static void Dump(this Operations.Intermesh.Basics.IntermeshTriangle triangle, Point3D focusAt, double magnification, string text = "")
        {
            var zone = new Rectangle3D(focusAt, 1 / magnification);
            {
                var clips = zone.Clip(triangle.PerimeterSegments.Select(s => s.Segment));
                clips = clips.TranslateToPointAndScale(focusAt, magnification);
                WavefrontFile.Export(clips, $"Wavefront/IntermeshTriangle-{triangle.Id}/Perimeter");
            }
            {
                var clips = zone.Clip(triangle.IntersectionSegments.Select(s => s.Segment));
                clips = clips.TranslateToPointAndScale(focusAt, magnification);
                WavefrontFile.Export(clips, $"Wavefront/IntermeshTriangle-{triangle.Id}/Intersections");
            }
            {
                var clips = zone.Clip(triangle.IntersectingTriangles.Select(t => t.Triangle).SelectMany(t => t.Edges));
                clips = clips.TranslateToPointAndScale(focusAt, magnification);
                WavefrontFile.Export(clips, $"Wavefront/IntermeshTriangle-{triangle.Id}/IntersectingTriangles");
            }

            foreach (var segment in triangle.PerimeterSegments)
            {
                var clip = zone.Clip(segment.Segment);
                clip = clip.TranslateToPointAndScale(focusAt, magnification);
                WavefrontFile.Export([clip], $"Wavefront/IntermeshTriangle-{triangle.Id}/Perimeter-Segment-{segment.Key}-{segment.Id}");
            }
            foreach (var segment in triangle.IntersectionSegments)
            {
                var clip = zone.Clip(segment.Segment);
                clip = clip.TranslateToPointAndScale(focusAt, magnification);
                WavefrontFile.Export([clip], $"Wavefront/IntermeshTriangle-{triangle.Id}/Intersection-Segment-{segment.Key}-{segment.Id}");
            }

            foreach (var filling in triangle.Fillings)
            {
                var clips = zone.Clip(filling.Triangle);
                clips = clips.TranslateToPointAndScale(focusAt, magnification);
                WavefrontFile.Export(clips, $"Wavefront/IntermeshTriangle-{triangle.Id}/FillTriangle-{filling.Id}");
            }
        }
    }
}
