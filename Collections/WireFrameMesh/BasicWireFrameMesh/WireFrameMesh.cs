using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using BasicObjects.GeometricObjects;
using E = BasicObjects.Math;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collections.Buckets;

namespace Collections.WireFrameMesh.BasicWireFrameMesh
{
    public class WireFrameMesh : PositionTriangleMesh, IWireFrameMesh
    {
        internal WireFrameMesh()
        {
            Positions = new ReadOnlyCollection<Position>(_positions);
        }

        private List<Position> _positions = new List<Position>();
        private BoxBucket<Position> _bucket = new BoxBucket<Position>(Enumerable.Empty<Position>());

        public IReadOnlyList<Position> Positions { get; }

        public PositionNormal AddPoint(Point3D position, Vector3D normal)
        {
            var positionNormal = AddPointNoRow(position, normal);
            AddPoint(positionNormal);
            return positionNormal;
        }

        public PositionNormal AddPointNoRow(Point3D position, Vector3D normal)
        {   
            var positionObject = _bucket.Fetch(new Rectangle3D(position, BoxBucket.MARGINS)).SingleOrDefault(p => p.Point == position);
            if (positionObject is null)
            {
                var positionNormal = new PositionNormal(position, normal, this);
                positionObject = new Position(position);
                positionNormal.LinkPosition(positionObject);
                _bucket.Add(positionObject);
                _positions.Add(positionObject);
                return positionNormal;
            }

            var existingPositionNormal = positionObject.PositionNormals.SingleOrDefault(pn => Vector3D.DirectionsEqual(pn.Normal, normal));
            if (existingPositionNormal is not null) { return existingPositionNormal; }
            {
                var positionNormal = new PositionNormal(position, normal, this);
                positionNormal.LinkPosition(positionObject);
                return positionNormal;
            }
        }

        private PositionNormal CloneAddOrExisting(PositionNormal positionNormal)
        {
            var positionObject = _bucket.Fetch(positionNormal).SingleOrDefault(p => p.Point == positionNormal.Position);
            if (positionObject is null)
            {
                var clone = new PositionNormal(positionNormal.Position, positionNormal.Normal, this);
                positionObject = new Position(positionNormal.Position);
                clone.LinkPosition(positionObject);
                _bucket.Add(positionObject);
                _positions.Add(positionObject);
                return clone;
            }

            var existingPositionNormal = positionObject.PositionNormals.SingleOrDefault(pn => Vector3D.DirectionsEqual(pn.Normal, positionNormal.Normal));
            if (existingPositionNormal is not null) { return existingPositionNormal; }
            {
                var clone = new PositionNormal(positionNormal.Position, positionNormal.Normal, this);
                clone.LinkPosition(positionObject);
                return clone;
            }
        }

        public void AddGrid(IWireFrameMesh inputMesh)
        {
            EndGrid();

            Dictionary<int, List<PositionNormal>> mapping = new Dictionary<int, List<PositionNormal>>();

            foreach (var position in inputMesh.Positions)
            {
                foreach(var positionNormal in position.PositionNormals)
                {
                    foreach(var triangle in positionNormal.Triangles)
                    {
                        if (!mapping.ContainsKey(triangle.Id)) { mapping[triangle.Id] = new List<PositionNormal>(3); }
                        mapping[triangle.Id].Add(CloneAddOrExisting(positionNormal));
                    }
                }
            }
            foreach(var triangle in mapping.Values.Where(v => v.Count == 3))
            {
                new PositionTriangle(triangle[0], triangle[1], triangle[2]);
            }

            EndGrid();
        }

        public void AddGrids(IEnumerable<IWireFrameMesh> grids)
        {
            foreach (var grid in grids)
            {
                AddGrid(grid);
            }
        }

        public IWireFrameMesh Clone()
        {
            var clone = new WireFrameMesh();

            Dictionary<int, List<PositionNormal>> mapping = new Dictionary<int, List<PositionNormal>>();

            foreach (var position in Positions)
            {
                foreach (var positionNormal in position.PositionNormals)
                {
                    foreach (var triangle in positionNormal.Triangles)
                    {
                        if (!mapping.ContainsKey(triangle.Id)) { mapping[triangle.Id] = new List<PositionNormal>(3); }
                        mapping[triangle.Id].Add(clone.CloneAddOrExisting(positionNormal));
                    }
                }
            }

            foreach (var triangle in mapping.Values.Where(v => v.Count == 3))
            {
                new PositionTriangle(triangle[0], triangle[1], triangle[2]);
            }

            return clone;
        }

        public IEnumerable<IWireFrameMesh> Clones(int number)
        {
            var clones = new IWireFrameMesh[number];

            for (int i = 0; i < number; i++)
            {
                clones[i] = Clone();
            }

            return clones;
        }

        public IWireFrameMesh CreateNewInstance()
        {
            return new WireFrameMesh();
        }

        public static IWireFrameMesh CreateMesh()
        {
            return new WireFrameMesh();
        }

    }
}
