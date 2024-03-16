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
            CalculateDivisions.Action(collections.Triangles);

            SetIntersectionLinks.Action(collections.Triangles);
            SetDivisionLinks.Action(collections.Triangles);
            //var elasticLinks = BuildElasticLinks.Action(intermeshTriangles);
            //PullElasticLinks.Action(elasticLinks);
            //var fillTriangles = ExtractFillTriangles.Action(elasticLinks);
            //var grid = BuildNewGrid.Action(fillTriangles);

            //return grid;
            return mesh;
        }
    }
}
