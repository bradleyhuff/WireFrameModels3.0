using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal class IntersectionVertexContainer
    {
        private static int _id = 0;
        public IntersectionVertexContainer()
        {
            Id = _id++;
        }

        public int Id { get; }

        private IntermeshIntersection _intersection;
        private char _tag;
        public IntersectionVertexContainer(IntermeshIntersection intersection, char tag) : this() { _intersection = intersection; _tag = tag; }
        public VertexCore Vertex { get; set; }

        public IntermeshIntersection Intersection { get { return _intersection; } }

        public List<IntermeshIntersection> DisabledIntersectors { get; set; } = new List<IntermeshIntersection>();

        public Point3D Point
        {
            get
            {
                if (Vertex is not null) { return Vertex.Point; }
                if (_tag == 'a') { return _intersection.Intersection.Start; }
                return _intersection.Intersection.End;
            }
        }

        public IntersectionVertexContainer Opposite
        {
            get
            {
                if (Intersection.VertexA != this) { return Intersection.VertexB; }
                return Intersection.VertexA;
            }
        }

        private IEnumerable<IntersectionVertexContainer> GetChildren()
        {
            if (Vertex is null) { yield break; }

            foreach (var child in Vertex.IntersectionContainers.Select(c => c.Intersection).
                Where(l => l.Id != Intersection.Id && !l.VerticiesAB.Any(v => v.Vertex is null)).
                Select(l => l.VerticiesAB.Where(v => v.Vertex.Id != Vertex.Id)).Where(c => c.Count() == 1))
            {
                foreach (var element in child) { yield return element; }
            }
        }

        public IEnumerable<IntersectionVertexContainer> GetTree()
        {
            return GetTreeUntil(v => false);
        }

        public IEnumerable<IntersectionVertexContainer> GetTreeUntil(Func<IntersectionVertexContainer, bool> stop)
        {
            Dictionary<int, bool> childrenTable = new Dictionary<int, bool>();
            var children = new IntersectionVertexContainer[] { this };
            children = children.Where(c => !stop(c)).ToArray();

            while (children.Any())
            {
                foreach (var child in children) { childrenTable[VertexId(child)] = true; yield return child; }
                var children0 = children.SelectMany(c => c.GetChildren()).DistinctBy(c => VertexId(c)).Where(c => !stop(c));
                children = children0.Where(c => !childrenTable.ContainsKey(VertexId(c))).ToArray();
            }
        }

        private static int VertexId(IntersectionVertexContainer container)
        {
            if (container.Vertex is null) { return container.Id; }
            return container.Vertex.Id;
        }

        public void Link(VertexCore vertex)
        {
            Vertex = vertex;
            vertex.Link(this);
        }

        public void Delink()
        {
            Vertex.Delink(this);
        }
    }
}
