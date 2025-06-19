using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.SetOperators
{
    public static class SetsDisjoint
    {
        public static IEnumerable<IWireFrameMesh[]> DisjointGroups(this IEnumerable<IWireFrameMesh> meshes)
        {
            var disjointSets = new List<DisjointSet>();
            foreach (var element in meshes)
            {
                if (!IsAddedToASet(element, disjointSets))
                {
                    var newDisjointSet = new DisjointSet();
                    newDisjointSet.AddToDisjointSet(element);
                    disjointSets.Add(newDisjointSet);
                }
            }

            foreach (var set in disjointSets)
            {
                yield return set.List.ToArray();
            }
        }

        private static bool IsAddedToASet(IWireFrameMesh element, List<DisjointSet> list)
        {
            if (!list.Any()) { return false; }
            foreach(var set in list.OrderByDescending(l => l.List.Count))
            {
                if (set.AddToDisjointSet(element)) { return true; }
            }
            return false;
        }
    }

    internal class DisjointSet()
    {
        private BoxBucket<PositionTriangle> _bucket;
        private List<IWireFrameMesh> _list = new List<IWireFrameMesh>();

        public bool AddToDisjointSet(IWireFrameMesh input)
        {
            if(_bucket is null)
            {
                _bucket = new BoxBucket<PositionTriangle>(input.Triangles);
                Size = input.Triangles.Count;
                _list.Add(input);
                return true;
            }
            if (Intersects(input))
            {
                return false;
            }

            _bucket.AddRange(input.Triangles);
            Size += input.Triangles.Count;
            _list.Add(input);
            return true;
        }

        public int Size { get; private set; }
        public IReadOnlyList<IWireFrameMesh> List
        {
            get
            {
                return _list;
            }
        }

        private bool Intersects(IWireFrameMesh input)
        {
            foreach (var triangle in input.Triangles)
            {
                var matches = _bucket.Fetch(triangle);

                foreach(var match in matches)
                {
                    if (Triangle3D.LineSegmentIntersections(match.Triangle, triangle.Triangle).Any()) { return true; }
                }
            }
            return false;
        }
    }
}
