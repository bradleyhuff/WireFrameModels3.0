using Collections.WireFrameMesh.BasicWireFrameMesh;
using FileExportImport;
using Operations.PositionRemovals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireFrameModels3._0;

namespace Projects.Projects
{
    public class PositionRemovals : ProjectBase
    {
        protected override void RunProject()
        {
            var grid = PntFile.Import(WireFrameMesh.Create, "Pnt/SphereDifference8 32");
            grid.RemoveShortSegments(3e-4);

            //var edges = grid.Positions.Where(p => p.Cardinality == 2).ToArray();

            //Console.WriteLine($"Edges {grid.Positions.Count(p => p.Cardinality == 2)}");
            //Console.WriteLine($"Corners {grid.Positions.Count(p => p.Cardinality == 3)}");

            //grid.RemovePosition(edges[0]);
            //grid.RemovePosition(edges[1]);
            //grid.RemovePosition(edges[2]);
            //grid.RemovePosition(edges[3]);
            //grid.RemovePosition(edges[4]);
            //grid.RemovePosition(edges[5]);
            //grid.RemovePosition(edges[6]);
            //grid.RemovePosition(edges[7]);
            //grid.RemovePosition(edges[8]);
            //grid.RemovePosition(edges[9]);
            //grid.RemovePosition(edges[10]);

            WavefrontFile.Export(grid, "Wavefront/GridPositionRemovals");
        }
    }
}
