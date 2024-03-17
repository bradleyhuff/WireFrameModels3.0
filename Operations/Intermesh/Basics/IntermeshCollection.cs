using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshCollection
    {
        private IReadOnlyList<PositionTriangle> gridTriangles;
        private IReadOnlyList<IntermeshTriangle> triangles;
        private Dictionary<PositionTriangle, IntermeshTriangle> _lookup = new Dictionary<PositionTriangle, IntermeshTriangle>();

        public IntermeshCollection(IWireFrameMesh grid)
        {
            gridTriangles = grid.Triangles;
        }

        public IReadOnlyList<IntermeshTriangle> Triangles
        {
            get
            {
                if (triangles is null)
                {
                    triangles = gridTriangles.Select(GetTriangle).ToList();
                }
                return triangles;
            }
        }

        private IntermeshTriangle GetTriangle(PositionTriangle triangle)
        {
            if (!_lookup.ContainsKey(triangle)) { _lookup[triangle] = new IntermeshTriangle(triangle, _lookup); }
            return _lookup[triangle];
        }
    }
}
