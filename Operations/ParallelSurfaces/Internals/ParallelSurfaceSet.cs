using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.ParallelSurfaces.Internals
{
    internal class ParallelSurfaceSet
    {
        public IWireFrameMesh Mesh { get; set; }
        public int Index { get; set; }
        //public List<BasePoint[]> BaseLoops { get; set; }
        public List<Point3D[]> SurfaceLoops { get; set; }
        //public List<List<Quadrangle>> QuadrangleSets { get; set; }
        public List<Quadrangle> QuadrangleSets { get; set; }

        public IEnumerable<IPerimeterChainLink> BasePerimeterLinks { get; set; }
    }
}
