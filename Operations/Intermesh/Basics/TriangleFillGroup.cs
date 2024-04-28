using BasicObjects.GeometricObjects;
using Operations.Filling.Basics;

namespace Operations.Intermesh.Basics
{
    internal class TriangleFillingGroup : PlanarFillingGroup
    {
        public TriangleFillingGroup(SurfaceTriangle triangle)
        {
            Triangle = triangle;
            Plane = triangle.Triangle.Plane;
            TestSegmentLength = triangle.Triangle.Box.Diagonal;
        }
        public SurfaceTriangle Triangle { get; set; }
    }
}
