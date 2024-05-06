using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using Math = BasicObjects.Math.Math;

namespace FundamentalMeshes
{
    public class Cuboid
    {
        public static IWireFrameMesh Create(double width, int widthSubdivisions, double height, int heightSubdivisions, double depth, int depthSubdivisions)
        {
            double xDelta = width / widthSubdivisions;
            double yDelta = height / heightSubdivisions;
            double zDelta = depth / depthSubdivisions;

            IWireFrameMesh block = WireFrameMesh.Create();

            for (int i = 0; i <= widthSubdivisions; i++)
            {
                for (int j = 0; j <= heightSubdivisions; j++)
                {
                    block.AddPoint(new Point3D(i * xDelta, j * yDelta, 0), new Vector3D(0, 0, -1));
                }
                block.EndRow();
            }
            block.EndGrid();

            for (int i = 0; i <= widthSubdivisions; i++)
            {
                for (int j = 0; j <= heightSubdivisions; j++)
                {
                    block.AddPoint(new Point3D(i * xDelta, j * yDelta, depth), new Vector3D(0, 0, 1));
                }
                block.EndRow();
            }
            block.EndGrid();

            for (int i = 0; i <= widthSubdivisions; i++)
            {
                for (int j = 0; j <= depthSubdivisions; j++)
                {
                    block.AddPoint(new Point3D(i * xDelta, 0, j * zDelta), new Vector3D(0, -1, 0));
                }
                block.EndRow();
            }
            block.EndGrid();

            for (int i = 0; i <= widthSubdivisions; i++)
            {
                for (int j = 0; j <= depthSubdivisions; j++)
                {
                    block.AddPoint(new Point3D(i * xDelta, height, j * zDelta), new Vector3D(0, 1, 0));
                }
                block.EndRow();
            }
            block.EndGrid();

            for (int i = 0; i <= heightSubdivisions; i++)
            {
                for (int j = 0; j <= depthSubdivisions; j++)
                {
                    block.AddPoint(new Point3D(0, i * yDelta, j * zDelta), new Vector3D(-1, 0, 0));
                }
                block.EndRow();
            }
            block.EndGrid();

            for (int i = 0; i <= heightSubdivisions; i++)
            {
                for (int j = 0; j <= depthSubdivisions; j++)
                {
                    block.AddPoint(new Point3D(width, i * yDelta, j * zDelta), new Vector3D(1, 0, 0));
                }
                block.EndRow();
            }
            block.EndGrid();

            return block;
        }
    }
}
