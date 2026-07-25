using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using FileExportImport;
using Operations.Intermesh.Basics;
using System.Collections.Generic;

namespace Operations.Diagnostics
{
    internal static class IntermeshTriangleDiagnotics
    {
        internal static void Show(this IntermeshTriangle triangle)
        {
            var errorTableCapsule = new Dictionary<int, (bool A, bool B)>();
            var errorTablePoint = new Dictionary<int, bool>();

            var endPointGroups = triangle.PerimeterSlots.SelectMany(s => s.EndPoints).GroupBy(g => g.Id);
            var mismatchedGroups = endPointGroups.Where(g => g.Count() != 2);

            foreach (var group in mismatchedGroups)
            {
                errorTablePoint[group.Key] = true;
            }

            foreach (var slot in triangle.PerimeterSlots)
            {
                var capsules = slot.Segments.SelectMany(s => s.Capsules).ToArray();
                for (int i = 1; i < capsules.Length - 1; i++)
                {
                    SetErrors(capsules[i - 1], capsules[i], errorTableCapsule);
                }
            }


            foreach (var slot in triangle.IntersectionSlots)
            {
                var capsules = slot.Segments.SelectMany(s => s.Capsules).ToArray();
                for (int i = 1; i < capsules.Length - 1; i++)
                {
                    SetErrors(capsules[i - 1], capsules[i], errorTableCapsule);
                }
            }

            BaseObjects.Console.Write($"AB: {triangle.AB.Id,6:#####0} ", ConsoleColor.Green);
            triangle.AB.Show(errorTableCapsule, errorTablePoint);
            BaseObjects.Console.Write($"BC: {triangle.BC.Id,6:#####0} ", ConsoleColor.Green);
            triangle.BC.Show(errorTableCapsule, errorTablePoint);
            BaseObjects.Console.Write($"CA: {triangle.CA.Id,6:#####0} ", ConsoleColor.Green);
            triangle.CA.Show(errorTableCapsule, errorTablePoint);
            foreach (var intersectionSlot in triangle.IntersectionSlots/*.Where(s => s.Id == 5869)*/)
            {
                BaseObjects.Console.Write($"I:  {intersectionSlot.Id,6:#####0} ", ConsoleColor.Green);
                intersectionSlot.Show(errorTableCapsule, errorTablePoint);
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

        internal static void Show(this IntermeshEdgeSlot slot, Dictionary<int, (bool A, bool B)> errorTableCapsule, Dictionary<int, bool> errorTablePoint)
        {
            if (!slot.Segments.Any()) { BaseObjects.Console.WriteLine("[]", ConsoleColor.Green); return; }

            int length = slot.Segments.Count();
            if (length == 1)
            {
                var first = slot.Segments[0];
                BaseObjects.Console.Write("[", ConsoleColor.Green);
                first.Show(slot, errorTableCapsule, errorTablePoint, true, false);
                BaseObjects.Console.WriteLine("]", ConsoleColor.Green);
                return;
            }

            {
                var first = slot.Segments.First();
                BaseObjects.Console.Write("[", ConsoleColor.Green);
                first.Show(slot, errorTableCapsule, errorTablePoint, true, true);
            }

            foreach (var segment in slot.Segments.Skip(1).Take(length - 2))
            {
                segment.Show(slot, errorTableCapsule, errorTablePoint, false, true);
            }
            {
                var last = slot.Segments.Last();
                last.Show(slot, errorTableCapsule, errorTablePoint, false, false);
                BaseObjects.Console.WriteLine("]", ConsoleColor.Green);
            }
        }

        internal static void Show(this IntermeshSegment segment, IntermeshEdgeSlot slot, Dictionary<int, (bool A, bool B)> errorTableCapsule, Dictionary<int, bool> errorTablePoint, bool isFirst, bool nextLine)
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
                if (errorTableCapsule.ContainsKey(first.Id)) first.Show(slot, errorTableCapsule[first.Id], errorTablePoint); else first.Show(slot, (true, true), errorTablePoint);
                BaseObjects.Console.Write("]", ConsoleColor.Yellow);
                if (nextLine) BaseObjects.Console.WriteLine();
                return;
            }
            {
                var first = segment.Capsules.First();
                BaseObjects.Console.Write($"[", ConsoleColor.Yellow);
                if (errorTableCapsule.ContainsKey(first.Id)) first.Show(slot, errorTableCapsule[first.Id], errorTablePoint); else first.Show(slot, (true, true), errorTablePoint);
                BaseObjects.Console.WriteLine();
            }
            foreach (var capsule in segment.Capsules.Skip(1).Take(length - 2))
            {
                BaseObjects.Console.Write($"             ", ConsoleColor.Cyan);
                if (errorTableCapsule.ContainsKey(capsule.Id)) capsule.Show(slot, errorTableCapsule[capsule.Id], errorTablePoint); else capsule.Show(slot, (true, true), errorTablePoint);
                BaseObjects.Console.WriteLine();
            }
            {
                var last = segment.Capsules.Last();
                BaseObjects.Console.Write($"             ", ConsoleColor.Cyan);
                if (errorTableCapsule.ContainsKey(last.Id)) last.Show(slot, errorTableCapsule[last.Id], errorTablePoint); else last.Show(slot, (true, true), errorTablePoint);
                BaseObjects.Console.Write("]", ConsoleColor.Yellow);

                if (nextLine) BaseObjects.Console.WriteLine();
            }
        }

        private static void Show(this IntermeshCapsule capsule, IntermeshEdgeSlot slot, (bool A, bool B) check, Dictionary<int, bool> others)
        {
            BaseObjects.Console.Write($"{{", ConsoleColor.Cyan);
            BaseObjects.Console.Write($"{capsule.A.Id,6:#####0}", ConsoleColor.Cyan, (!check.A || others.ContainsKey(capsule.A.Id)) ? ConsoleColor.DarkRed : System.Console.BackgroundColor);
            BaseObjects.Console.Write($", ", ConsoleColor.Cyan);
            BaseObjects.Console.Write($"{capsule.B.Id,6:#####0}", ConsoleColor.Cyan, (!check.B || others.ContainsKey(capsule.B.Id)) ? ConsoleColor.DarkRed : System.Console.BackgroundColor);
            BaseObjects.Console.Write($"}}", ConsoleColor.Cyan);
            BaseObjects.Console.Write($" L {capsule.Segment.Length.ToString("E2")} D [{capsule.Segment.Vector.Direction.X.ToString("0.000")}, {capsule.Segment.Vector.Direction.Y.ToString("0.000")}, {capsule.Segment.Vector.Direction.Z.ToString("0.000")}]", capsule.Segment.Length < GapConstants.Resolver ? ConsoleColor.Yellow : ConsoleColor.Gray);
            //BaseObjects.Console.WriteLine(("A", ConsoleColor.Red), ("B", ConsoleColor.Green));
            BaseObjects.Console.Write($" λMin {slot.TotalSegment.Coordinate(capsule.A.Point),7:#0.0000} λMax {slot.TotalSegment.Coordinate(capsule.B.Point),7:#0.0000}  ΣL {slot.TotalSegment.Length.ToString("E2")}", ConsoleColor.Gray);
        }

        private static void ShowDirection(Vector3D direction)
        {

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

            foreach (var intersectingTriangle in triangle.IntersectingTriangles)
            {
                var clips = zone.Clip(intersectingTriangle.Triangle.Edges);
                clips = clips.TranslateToPointAndScale(focusAt, magnification);
                WavefrontFile.Export(clips, $"Wavefront/IntermeshTriangle-{triangle.Id}/IntersectingTriangle -{intersectingTriangle.Id}");
            }
        }
    }
}
