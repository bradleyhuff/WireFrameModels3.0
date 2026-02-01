using BaseObjects;
using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Extensions;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using Operations.Basics;
using Operations.ParallelSurfaces;
using WireFrameModels3._0;
using Console = BaseObjects.Console;

namespace Projects.Projects
{
    internal class UnionCheck : ProjectBase
    {
        protected override void RunProject()
        {
            //Console.WriteLine("Segment intersections");
            //var segment1 = new LineSegment3D(0.475014917905482, 0.359002248557203, 0.107003976697903, 0.475014383012836, 0.358883768101989, 0.107390120265219);
            //var segment2 = new LineSegment3D(0.475237013905030, 0.359217064835224, 0.107358417472800, 0.475014910791702, 0.359000557292749, 0.107009486537537);
            //Console.WriteLine($"Intersections {LineSegment3D.PointIntersection(segment1, segment2, 3e-9)}");

            var import = PntFile.Import(WireFrameMesh.Create, "Pnt/SphereDifference8 64");
            //import.NearCollinearTrianglePairs();
            //import.ShowVitals();
            //WavefrontFile.Export(import, "Wavefront/ModifiedOutput");

            //return;
            //import.Apply(Transform.Rotation((Vector3D.BasisX + Vector3D.BasisY + Vector3D.BasisZ).Direction, 1e-2));
            //import.Apply(Transform.ShearXY(1e-3, 1e-3));
            //import.Apply(Transform.ShearYZ(1e-3, 1e-3));
            //import.Apply(Transform.ShearXZ(1e-3, 1e-3));
            //WavefrontFile.Export(import, "Wavefront/Import");

            //var cornerCut = GroupingCollection.ExtractClusters(CornerCut(import).Triangles);

            //return;

            var clusters = import.BuildFacePlateClusters(-0.000500).ToArray();
            //var clusters = import.BuildFacePlateClusters(-0.00125).ToArray();
            clusters.PlateTrim(o => o);
            //clusters.PlateTrim(o => CornerCut(o));


            //var fill = new Triangle3D(new Point3D(0.114443516250287, 0.434066015928310, 0.40551524427083),new Point3D(0.113999950709140, 0.434181599758615, 0.405515357986652), new Point3D(0.114440816380691, 0.434066719367738, 0.405515244898318));
            //var triangle = new Triangle3D(new Point3D(0.114528705752820, 0.434043818779481, 0.405515223469387), new Point3D(0.113999950709140, 0.434181599758615, 0.405515357986652), new Point3D(0.114440816380691, 0.434066719367738, 0.405515244898318));
            //fill = fill.Scale(1000);
            //triangle = triangle.Scale(1000);
            //{
            //    var grid = WireFrameMesh.Create();
            //    var pointA = triangle.MinimumHeightScale(triangle.A, 0.25 / triangle.AspectRatio);
            //    var pointB = triangle.MinimumHeightScale(triangle.B, 0.25 / triangle.AspectRatio);
            //    var pointC = triangle.MinimumHeightScale(triangle.C, 0.25 / triangle.AspectRatio);
            //    var triangle2 = new Triangle3D(pointA, pointB, pointC);
            //    triangle2 = triangle2.Scale(1000);
            //    grid.AddTriangle(triangle2, "", 0);

            //    pointA = triangle.MinimumHeightScale(fill.A, 0.25 / triangle.AspectRatio);
            //    pointB = triangle.MinimumHeightScale(fill.B, 0.25 / triangle.AspectRatio);
            //    pointC = triangle.MinimumHeightScale(fill.C, 0.25 / triangle.AspectRatio);
            //    triangle2 = new Triangle3D(pointA, pointB, pointC);
            //    triangle2 = triangle2.Scale(1000);
            //    grid.AddTriangle(triangle2, "", 0);

            //    WavefrontFile.Export(grid, "Wavefront/ProblemTriangles");
            //}

            //var scaleA = Triangle.MinimumHeightScale(division.A.Point, 0.25 / Triangle.AspectRatio);
            //var scaleB = Triangle.MinimumHeightScale(division.B.Point, 0.25 / Triangle.AspectRatio);
            //var scaleC = Triangle.MinimumHeightScale(division.Segment.Center, 0.25 / Triangle.AspectRatio);
            //grid.AddTriangle(scaleA, Triangle.Normal, scaleB, Triangle.Normal, scaleC, Triangle.Normal, "", 0);




            //var cluster = clusters.Single(c => c.Id == 0);
            //cluster.TrimmedClusterGrid.ShowVitals(99);

            var output = clusters.Select(c => c.TrimmedClusterGrid).Combine();
            //var output = clusters.First().TrimmedClusterGrid;
            WavefrontFile.Export(output, "Wavefront/Output");
            output.ShowVitals(99);

            //var surfaces = GroupingCollection.ExtractSurfaces(output.Triangles);
            //int index = 0;
            //foreach (var surface in surfaces)
            //{
            //    WavefrontFile.Export(surface.Create(), $"Wavefront/Surface-{index}");

            //    index++;
            //}

            //var focusAt = new Point3D(0.340518189192871, 0.499999999999922, 0.998223810162009);
            //var focusAtB = new Point3D(0.340518189195709, 0.500000000000000, 0.998223810335592);
            //var magnification = 1e9;
            //var zone = new Rectangle3D(focusAt, 1 / magnification);
            //WavefrontFile.Export(zone.LineSegments.Select(z => z.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/Surfaces/Zone");
            //var clippedRegion = zone.Clip(output.Triangles.SelectMany(t => t.Edges.Select(e => e.Segment)));
            //clippedRegion = clippedRegion.Select(c => c.TranslateToPointAndScale(focusAt, magnification));
            //WavefrontFile.Export(clippedRegion, $"Wavefront/Surfaces/Graph");
            //var gapSegment = new LineSegment3D(focusAt, focusAtB);
            //var gapSegments = zone.Clip([gapSegment]);
            //gapSegments = gapSegments.Select(c => c.TranslateToPointAndScale(focusAt, magnification));
            //WavefrontFile.Export(gapSegments, $"Wavefront/Surfaces/Gap");

            //var borderTriangle1 = output.Triangles.Single(t => t.Id == 341328);
            //var borderTriangle2 = output.Triangles.Single(t => t.Id == 341308);

            //WavefrontFile.Export(zone.Clip(borderTriangle1.Triangle).Select(c => c.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/Surfaces/BorderTriangles1");
            //WavefrontFile.Export(zone.Clip(borderTriangle2.Triangle).Select(c => c.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/Surfaces/BorderTriangles2");

            //344904
            //344906
            //347002
            //344905
            //344907

            //347006
            //344903
            //344908
            //344909
            //344902
            //347023
            //347025
            /*
PointA only Border triangle id 336668
PointA only Border triangle id 336670

PointA only Border triangle id 336669
PointB only Border triangle id 336671
PointA only Border triangle id 336667

PointB only Border triangle id 336672
PointB only Border triangle id 336673
PointA only Border triangle id 336666

PointB only Border triangle id 341329
PointA only Border triangle id 341327
PointB only Border triangle id 341306
PointA only Border triangle id 341310            
             */
            //var cornerTriangle1 = output.Triangles.Single(t => t.Id == 336668);
            //var cornerTriangle2 = output.Triangles.Single(t => t.Id == 336670);

            //var cornerTriangle3 = output.Triangles.Single(t => t.Id == 336669);
            //var cornerTriangle4 = output.Triangles.Single(t => t.Id == 336671);
            //var cornerTriangle5 = output.Triangles.Single(t => t.Id == 336667);

            //var cornerTriangle6 = output.Triangles.Single(t => t.Id == 336672);
            //var cornerTriangle7 = output.Triangles.Single(t => t.Id == 336673);
            //var cornerTriangle8 = output.Triangles.Single(t => t.Id == 336666);

            //var cornerTriangle9 = output.Triangles.Single(t => t.Id == 341329);
            //var cornerTriangle10 = output.Triangles.Single(t => t.Id == 341327);
            //var cornerTriangle11 = output.Triangles.Single(t => t.Id == 341306);
            //var cornerTriangle12 = output.Triangles.Single(t => t.Id == 341310);

            //WavefrontFile.Export(zone.Clip(cornerTriangle1.Triangle).Select(c => c.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/Surfaces/CornerTriangles1");
            //WavefrontFile.Export(zone.Clip(cornerTriangle2.Triangle).Select(c => c.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/Surfaces/CornerTriangles2");
            //WavefrontFile.Export(zone.Clip(cornerTriangle3.Triangle).Select(c => c.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/Surfaces/CornerTriangles3");
            //WavefrontFile.Export(zone.Clip(cornerTriangle4.Triangle).Select(c => c.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/Surfaces/CornerTriangles4");
            //WavefrontFile.Export(zone.Clip(cornerTriangle5.Triangle).Select(c => c.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/Surfaces/CornerTriangles5");
            //WavefrontFile.Export(zone.Clip(cornerTriangle6.Triangle).Select(c => c.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/Surfaces/CornerTriangles6");
            //WavefrontFile.Export(zone.Clip(cornerTriangle7.Triangle).Select(c => c.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/Surfaces/CornerTriangles7");
            //WavefrontFile.Export(zone.Clip(cornerTriangle8.Triangle).Select(c => c.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/Surfaces/CornerTriangles8");
            //WavefrontFile.Export(zone.Clip(cornerTriangle9.Triangle).Select(c => c.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/Surfaces/CornerTriangles9");
            //WavefrontFile.Export(zone.Clip(cornerTriangle10.Triangle).Select(c => c.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/Surfaces/CornerTriangles10");
            //WavefrontFile.Export(zone.Clip(cornerTriangle11.Triangle).Select(c => c.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/Surfaces/CornerTriangles11");
            //WavefrontFile.Export(zone.Clip(cornerTriangle12.Triangle).Select(c => c.TranslateToPointAndScale(focusAt, magnification)), $"Wavefront/Surfaces/CornerTriangles12");

            {
                var grid = WireFrameMesh.Create();
                var pointCount = new Dictionary<int, int>();
                var indexTable = new Combination2Dictionary<bool>();
                foreach (var triangle in output.Triangles)
                {
                    if (triangle.ABadjacents.Count > 1)
                    {
                        var position = grid.AddTriangle(new Triangle3D(triangle.Triangle.EdgeAB.Start, triangle.Triangle.EdgeAB.Center, triangle.Triangle.EdgeAB.End), "", 0);
                        var edgeIndex = new Combination2(position.A.PositionObject.Id, position.C.PositionObject.Id);
                        if (!indexTable.ContainsKey(edgeIndex))
                        {
                            AddCount(position.A.PositionObject.Id, pointCount);
                            AddCount(position.C.PositionObject.Id, pointCount);
                            indexTable[edgeIndex] = true;
                        }
                    }
                    if (triangle.BCadjacents.Count > 1)
                    {
                        var position = grid.AddTriangle(new Triangle3D(triangle.Triangle.EdgeBC.Start, triangle.Triangle.EdgeBC.Center, triangle.Triangle.EdgeBC.End), "", 0);
                        var edgeIndex = new Combination2(position.A.PositionObject.Id, position.C.PositionObject.Id);
                        if (!indexTable.ContainsKey(edgeIndex))
                        {
                            AddCount(position.A.PositionObject.Id, pointCount);
                            AddCount(position.C.PositionObject.Id, pointCount);
                            indexTable[edgeIndex] = true;
                        }
                    }
                    if (triangle.CAadjacents.Count > 1)
                    {
                        var position = grid.AddTriangle(new Triangle3D(triangle.Triangle.EdgeCA.Start, triangle.Triangle.EdgeCA.Center, triangle.Triangle.EdgeCA.End), "", 0);
                        var edgeIndex = new Combination2(position.A.PositionObject.Id, position.C.PositionObject.Id);
                        if (!indexTable.ContainsKey(edgeIndex))
                        {
                            AddCount(position.A.PositionObject.Id, pointCount);
                            AddCount(position.C.PositionObject.Id, pointCount);
                            indexTable[edgeIndex] = true;
                        }
                    }
                }

                var tallyTable = new Dictionary<int, int>();
                foreach (var kp in pointCount)
                {
                    if (!tallyTable.ContainsKey(kp.Value)) { tallyTable[kp.Value] = 0; }
                    tallyTable[kp.Value]++;
                }
                foreach (var kp in tallyTable)
                {
                    Console.WriteLine($"Tally {kp.Key}: {kp.Value}");
                }

                var brokenPoints = pointCount.Where(kp => kp.Value == 1).Select(kp => kp.Key);
                Console.WriteLine($"Broken points {string.Join(",", brokenPoints)}");
                var brokenPointsP = grid.Positions.Where(p => brokenPoints.Contains(p.Id)).ToArray();
                Console.WriteLine($"Broken points \n{string.Join("\n", brokenPointsP.Select(p => $"{p.Id} {p.Point}"))}");

                WavefrontFile.Export(grid, $"Wavefront/SurfaceBoundary");
                //var gridB = WireFrameMesh.Create();
                //gridB.AddTriangle(new Triangle3D(brokenPointsP[0].Point,Point3D.Average([brokenPointsP[0].Point, brokenPointsP[1].Point]), brokenPointsP[1].Point), "", 0);
                //gridB.AddTriangle(new Triangle3D(brokenPointsP[2].Point, Point3D.Average([brokenPointsP[2].Point, brokenPointsP[3].Point]), brokenPointsP[3].Point), "", 0);
                //WavefrontFile.Export(gridB, $"Wavefront/BrokenPoints");

            }

            //WavefrontFileGroups.ExportByFaces(output, "Wavefront/Faces");

            var erred = clusters.Select(c => c.OriginalClusterGrid).Combine();
            WavefrontFile.Export(erred, "Wavefront/Erred");

            //WavefrontFileGroups.ExportBySurfaces(output, "Wavefront/Surface");

            //foreach (var cluster in clusters)
            //{
            //    var disjoints = cluster.Faces.Select(f => f.FacePlate).DisjointGroups().ToArray();
            //    Console.WriteLine($"Cluster {cluster.Id} Disjoints {disjoints.Length}", ConsoleColor.Green);
            //    if (disjoints.Length == 6)
            //    {
            //        WavefrontFile.Export(cluster.Cluster.Create(), $"Wavefront/DisjointCluster-{cluster.Id}");
            //        foreach (var group in disjoints.Select((d, i) => new { d, i }))
            //        {
            //            WavefrontFile.Export(group.d.Combine(), $"Wavefront/DisjointGroups-{cluster.Id}-{group.i}");
            //        }
            //    }
            //}

            //var start = DateTime.Now;
            //var facePlates = clusters.SelectMany(c => c.Faces.Select(f => f.FacePlate));

            //var disjointGroups = facePlates.DisjointGroups().ToArray();

            //Console.WriteLine($"Disjoint sets {disjointGroups.Length} {(DateTime.Now - start).TotalSeconds} seconds.", ConsoleColor.Yellow);

            //var combinedGroups = disjointGroups.Select(g => g.Combine()).ToArray();

            //foreach (var set in combinedGroups.Select((s, i) => new { s, i }))
            //{
            //    WavefrontFile.Export(set.s, $"Wavefront/DisjointSet-{set.i}");
            //    set.s.ShowVitals(99);
            //}

            //var output = import;
            //foreach(var set in combinedGroups.Select((s, i) => new { s, i }))
            //{
            //    Console.WriteLine($"Disjoint set {set.i + 1}", ConsoleColor.Yellow);
            //    output = output.Difference(set.s);
            //    //WavefrontFile.Export(output, $"Wavefront/AppliedDisjointSet-{set.i + 1}");
            //    //output.ShowVitals(99);
            //}
            //Console.WriteLine($"Face plate build {(DateTime.Now - start).TotalSeconds} seconds.\n");
            //output.ShowVitals(99);
            //WavefrontFile.Export(output, "Wavefront/Output");


            //var export = WireFrameMesh.Create();
            //var facePlates2 = clusters2.SelectMany(c => c.Faces.Select(f => f.FacePlate)).ToArray();
            //foreach (var facePlate in facePlates2) { facePlate.ShowVitals(99); }
            //export.AddGrids(facePlates2);
            //WavefrontFile.Export(export, "Wavefront/FacePlates");
            //export.ShowVitals(99);
            //return;

            //import.Apply(Transform.Rotation((Vector3D.BasisX + Vector3D.BasisY + Vector3D.BasisZ).Direction, 1e-2));
            //var clusters = GroupingCollection.ExtractClusters(import.Triangles).Take(256).ToArray();
            //var output = clusters[180].Create();
            //var output = clusters[137].Create();
            //var output = clusters[51].Create();//chaining error
            //var output = clusters[7].Create();//chaining error
            //var output = clusters[38].Create();//banana with error
            //var output = clusters[99].Create();
            //var output = clusters[100].Create();

            //WavefrontFile.Export(output, "Wavefront/Input");
            //WavefrontFileGroups.ExportByFaces(output, "Wavefront/Input");

            //double offset = -0.0007;// 7
            ////double offset = -0.1050;
            //var facePlates = output.BuildFacePlates(offset).ToArray();
            //WavefrontFile.Export(facePlates, "Wavefront/FacePlates");

            //foreach (var facePlate in facePlates.Select((f, i) => new { Grid = f, Index = i }))
            //{
            //    Console.WriteLine("Face plate");
            //    output = output.Difference(facePlate.Grid);
            //    output.ShowVitals(99);
            //}

            //var output = WireFrameMesh.Create();
            //output.AddRangeTriangles(clusters.SelectMany(c => c.Triangles), "", 0);

            //WavefrontFile.Export(output, "Wavefront/Import");
            ////var output = import;
            //var start = DateTime.Now;
            //var clusterSets = new List<IWireFrameMesh[]>();
            //ConsoleLog.MaximumLevels = 1;
            //var index = 1;
            //foreach (var cluster in clusters)
            //{
            //    double offset = -0.0025;
            //    Console.Write($"{index} ");
            //    var facePlates = cluster.Create().BuildFacePlates(offset).ToArray();
            //    clusterSets.Add(facePlates);
            //    index++;
            //}
            //Console.WriteLine($"Face plate build {(DateTime.Now - start).TotalSeconds} seconds.\n");
            //ConsoleLog.MaximumLevels = 8;
            //var maxFaces = BasicObjects.Math.Math.Max(clusterSets.Select(c => c.Length).ToArray());// - 1;
            //Console.WriteLine($"Max faces {maxFaces}");

            //var faceGroups = new List<IWireFrameMesh>[maxFaces];
            //var faceGroupGrids = new IWireFrameMesh[maxFaces];

            //for (int i = 0; i < maxFaces; i++)
            //{
            //    faceGroups[i] = new List<IWireFrameMesh>();
            //    foreach (var cluster in clusterSets.Where(c => i < c.Length))
            //    {
            //        var facePlate = cluster[i];
            //        faceGroups[i].Add(facePlate);
            //    }
            //}

            //Console.WriteLine($"Face groups\n{string.Join("\n", faceGroups.Select((f, i) => $"{i}: {f.Count()}"))}");
            //for (int i = 0; i < maxFaces; i++)
            //{
            //    faceGroupGrids[i] = WireFrameMesh.Create();
            //    faceGroupGrids[i].AddGrids(faceGroups[i]);
            //}
            //Console.WriteLine($"Face group grids\n{string.Join("\n", faceGroupGrids.Select((f, i) => $"{i}: {f.Triangles.Count()}"))}");

            //for (int i = 0; i < maxFaces; i++)
            //{
            //    Console.WriteLine($"Face {i + 1}", ConsoleColor.Yellow);
            //    output = output.Difference(faceGroupGrids[i]);
            //    output.ShowVitals(99);
            //    WavefrontFile.Export(output, $"Wavefront/Output-Face-{i + 1}");
            //    WavefrontFile.Export(faceGroupGrids[i], $"Wavefront/FacePlates-{i + 1}");
            //}


            //Console.WriteLine("Output");
            //output.ShowVitals(99);
            //WavefrontFile.Export(output, "Wavefront/Output");

            //WavefrontFileGroups.ExportByFaces(output, "Wavefront/Output");

            //var union = Sets.Union(facePlates);
            //union.BaseStrip();
            //union.ShowVitals();

            //WavefrontFile.Export(output, "Wavefront/UnionCheck");
            //WavefrontFile.Export(facePlates, "Wavefront/FacePlates");
            //WavefrontFile.Export(union, "Wavefront/Union");
            //WavefrontFile.Export(NormalOverlay(output, 0.05), "Wavefront/UnionNormals");
        }

        private void AddCount(int index, Dictionary<int, int> table)
        {
            if (!table.ContainsKey(index)) { table[index] = 0; }
            table[index]++;
        }

        private IWireFrameMesh NormalOverlay(IWireFrameMesh input, double radius)
        {
            var output = WireFrameMesh.Create();

            foreach (var positionNormal in input.Positions.SelectMany(p => p.PositionNormals))
            {
                output.AddTriangle(positionNormal.Position, Vector3D.Zero, positionNormal.Position + 0.5 * radius * positionNormal.Normal.Direction, Vector3D.Zero, positionNormal.Position + radius * positionNormal.Normal.Direction, Vector3D.Zero, "", 0);
            }

            return output;
        }
    }
}
