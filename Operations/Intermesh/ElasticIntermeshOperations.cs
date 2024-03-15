using Collections.WireFrameMesh.Interfaces;
using Operations.Intermesh.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh
{
    public static class ElasticIntermeshOperations
    {
        public static IWireFrameMesh Intermesh(IWireFrameMesh mesh)
        {
            var intermeshTriangles = new IntermeshCollection(mesh);

            //TriangleGathering.Action(intermeshTriangles);
            //CalculateIntersections.Action(intermeshTriangles);
            //CalculateDivisions.Action(intermeshTriangles);

            //SetIntersectionLinks.Action(intermeshTriangles);
            //SetDivisionLinks.Action(intermeshTriangles);
            //var elasticLinks = BuildElasticLinks.Action(intermeshTriangles);
            //PullElasticLinks.Action(elasticLinks);
            //var fillTriangles = ExtractFillTriangles.Action(elasticLinks);
            //var grid = BuildNewGrid.Action(fillTriangles);

            //return grid;
            return mesh;
        }
    }
}
