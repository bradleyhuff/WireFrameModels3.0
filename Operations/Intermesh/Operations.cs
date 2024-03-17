using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using Operations.Intermesh.Basics;

namespace Operations.Intermesh.ElasticIntermeshOperations
{
    public static class Operations
    {
        public static IWireFrameMesh Intermesh(IWireFrameMesh mesh)
        {
            var collections = new IntermeshCollection(mesh);

            TriangleGathering.Action(collections.Triangles);
            CalculateIntersections.Action(collections.Triangles);

            var processTriangles = collections.Triangles.Where(t => t.Intersections.Any()).ToList();
            var byPassTriangles = collections.Triangles.Where(t => !t.Intersections.Any()).ToList();
            Console.WriteLine($"Process triangles {processTriangles.Count} ByPass triangles {byPassTriangles.Count}");

            CalculateDivisions.Action(processTriangles);

            SetIntersectionLinks.Action(processTriangles);
            SetDivisionLinks.Action(processTriangles);
            var elasticLinks = BuildElasticLinks.Action(processTriangles);
            PullElasticLinks.Action(elasticLinks);
            var fillTriangles = ExtractFillTriangles.Action(elasticLinks);

            var output = WireFrameMesh.CreateMesh();
            foreach(var triangle in byPassTriangles)
            {
                triangle.AddWireFrameTriangle(output);
            }
            foreach(var triangle in fillTriangles)
            {
                triangle.AddWireFrameTriangle(output);
            }
            return output;
        }
    }
}
