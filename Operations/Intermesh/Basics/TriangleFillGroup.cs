using BasicObjects.GeometricObjects;
using Operations.RegionalFilling.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal class TriangleFillingGroup : PlanarFillingGroup
    {
        public TriangleFillingGroup(SurfaceTriangle triangle)
        {
            Triangle = triangle;
            Plane = triangle.Triangle.Plane;
            //Planar = new PlanarGroupNode(triangle.Triangle.Plane);
            TestSegmentLength = triangle.Triangle.Box.Diagonal;
        }
        public SurfaceTriangle Triangle { get; set; }
    }
}
