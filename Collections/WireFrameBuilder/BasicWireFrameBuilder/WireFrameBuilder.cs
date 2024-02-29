using Collections.WireFrameBuilder.Basics;
using Collections.WireFrameBuilder.Interfaces;
using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collections.WireFrameBuilder.BasicWireFrameBuilder
{
    internal class WireFrameBuilder : PositionTriangleBuilder, IWireFrameMeshBuilder
    {
        public WireFrameBuilder()
        {
            PositionNormals = new ReadOnlyCollection<PositionNormal>(_positionNormalNodes);
        }

        private List<PositionNormal> _positionNormalNodes = new List<PositionNormal>();

        public IReadOnlyList<PositionNormal> PositionNormals { get; }

        public IReadOnlyList<PositionTriangle> Triangles => throw new NotImplementedException();

        public PositionNormal AddPoint(Point3D position, Vector3D normal)
        {
            return AddPoint(position, normal, 1);
        }

        public PositionNormal AddPointNoRow(Point3D position, Vector3D normal)
        {
            var node = CreateAndAdd();
            //node.Position = position;
            node.Normal = normal;
            return node;
        }

        public PositionNormal AddPoint(Point3D position, Vector3D normal, int number)
        {
            var node = CreateAndAdd();
            //node.Position = position;
            node.Normal = normal;
            AddPoint(node, number);
            return node;
        }

        public void AddGrid(IWireFrameMeshBuilder inputMesh)
        {
            EndGrid();

            Dictionary<int, PositionNormal> mapping = new Dictionary<int, PositionNormal>();

            foreach (var node in inputMesh.PositionNormals)
            {
                var newNode = AddPoint(node.Position.Point, node.Normal);
                //newNode.PositionIndex = node.PositionIndex;
                //mapping[node.Index] = newNode;
            }

            foreach (var node in inputMesh.Triangles)
            {
                //AddTriangle(new TriangleNode(
                //    mapping[node.A.Index],
                //    mapping[node.B.Index],
                //    mapping[node.C.Index]
                //    ));
            }

            EndGrid();
        }

        public void AddGrids(IEnumerable<IWireFrameMeshBuilder> grids)
        {
            foreach (var grid in grids)
            {
                AddGrid(grid);
            }
        }

        public IWireFrameMeshBuilder Clone()
        {
            IWireFrameMeshBuilder clone = CreateNewInstance();

            foreach (var node in PositionNormals)
            {
                var nodeClone = clone.CreateAndAdd();
                nodeClone.Position = node.Position;
                nodeClone.Normal = node.Normal;
                //nodeClone.PositionIndex = node.PositionIndex;
            }
            //foreach (var node in Triangles)
            //{
            //    clone.AddTriangle(
            //        new AdjacentsTriangleNode(
            //            clone.PositionNormals[node.A.Index],
            //            clone.PositionNormals[node.B.Index],
            //            clone.PositionNormals[node.C.Index]
            //            )
            //        );
            //}
            return clone;
        }

        public IEnumerable<IWireFrameMeshBuilder> Clones(int number)
        {
            var clones = new IWireFrameMeshBuilder[number];

            for (int i = 0; i < number; i++)
            {
                clones[i] = Clone();
            }

            return clones;
        }

        public PositionNormal CreateAndAdd()
        {
            return PositionNormal.CreateAndAdd(_positionNormalNodes);
        }
        public IWireFrameMeshBuilder CreateNewInstance()
        {
            return new WireFrameBuilder();
        }

        public void Clear()
        {
            _positionNormalNodes.Clear();
            //SetTriangles(new List<TriangleNode>());
        }

        public int RemoveTriangles(IEnumerable<Triangle3D> triangles)
        {
            throw new NotImplementedException();
        }

        void IWireFrameMeshBuilder.AddTriangleRange(IEnumerable<PositionTriangle> triangles)
        {
            throw new NotImplementedException();
        }

        void IWireFrameMeshBuilder.SetTriangles(IEnumerable<PositionTriangle> triangles)
        {
            throw new NotImplementedException();
        }
    }
}
