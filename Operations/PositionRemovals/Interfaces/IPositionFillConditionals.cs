using Collections.WireFrameMesh.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.PositionRemovals.Interfaces
{
    public interface IPositionFillConditionals: ISharedFillConditionals
    {
        public PositionNormal RemovalPoint { get; }
        public IEnumerable<PositionTriangle> Triangles { get; }
    }
}
