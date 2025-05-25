using BaseObjects;
using BasicObjects.GeometricObjects;
using System.Xml.Linq;

namespace Operations.Intermesh.Classes.V2
{
    internal class BuildDivisions
    {
        internal static void Action(IEnumerable<Basics.V2.IntermeshTriangle> intermeshTriangles)
        {
            DateTime start = DateTime.Now;

            foreach (var element in intermeshTriangles)
            {
                foreach (var segment in element.Segments)
                {
                    var divisions = segment.BuildDivisions();
                    element.AddRange(divisions);
                }
            }

            //AddNearCollinearPointsToPerimeter(intermeshTriangles);
 
            ConsoleLog.WriteLine($"Build divisions. Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
        }

        private static void AddNearCollinearPointsToPerimeter(IEnumerable<Basics.V2.IntermeshTriangle> intermeshTriangles)
        {
            foreach (var triangle in intermeshTriangles)
            {
                foreach (var nonVertexPoint in triangle.NonVertexPoints)
                {
                    if (triangle.AB.Segment.PointIsOnSegment(nonVertexPoint.Point, 3e-9))
                    {
                        var line = new Line3D(triangle.A.Point, triangle.B.Point);
                        var projection = line.Projection(nonVertexPoint.Point);
                        var distance = Point3D.Distance(nonVertexPoint.Point, projection);

                        triangle.AB.Add(nonVertexPoint);
                        nonVertexPoint.Add(triangle.AB);
                        triangle.ClearDivisions();
                        foreach (var segment in triangle.Segments)
                        {
                            var divisions = segment.BuildDivisions();
                            triangle.AddRange(divisions);
                        }
                        continue;
                    }
                    if (triangle.BC.Segment.PointIsOnSegment(nonVertexPoint.Point, 3e-9))
                    {
                        var line = new Line3D(triangle.B.Point, triangle.C.Point);
                        var projection = line.Projection(nonVertexPoint.Point);
                        var distance = Point3D.Distance(nonVertexPoint.Point, projection);

                        triangle.BC.Add(nonVertexPoint);
                        nonVertexPoint.Add(triangle.BC);
                        triangle.ClearDivisions();
                        foreach (var segment in triangle.Segments)
                        {
                            var divisions = segment.BuildDivisions();
                            triangle.AddRange(divisions);
                        }
                        continue;
                    }
                    if (triangle.CA.Segment.PointIsOnSegment(nonVertexPoint.Point, 3e-9))
                    {
                        var line = new Line3D(triangle.C.Point, triangle.A.Point);
                        var projection = line.Projection(nonVertexPoint.Point);
                        var distance = Point3D.Distance(nonVertexPoint.Point, projection);


                        triangle.CA.Add(nonVertexPoint);
                        nonVertexPoint.Add(triangle.CA);
                        triangle.ClearDivisions();
                        foreach (var segment in triangle.Segments)
                        {
                            var divisions = segment.BuildDivisions();
                            triangle.AddRange(divisions);
                        }
                    }
                }
            }
        }
    }
}
