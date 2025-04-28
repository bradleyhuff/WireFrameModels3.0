using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics.V1
{
    internal class VertexCore
    {
        private static int _id = 0;
        public VertexCore()
        {
            Id = _id++;
        }

        public int Id { get; }

        public Point3D Point { get; set; }

        public bool IsPerimeter { get; internal set; }

        private List<IntersectionVertexContainer> _intersectionContainers = new List<IntersectionVertexContainer>();
        internal IReadOnlyList<IntersectionVertexContainer> IntersectionContainers { get { return _intersectionContainers; } }

        internal void Link(IntersectionVertexContainer container)
        {
            container.Vertex = this;
            if (_intersectionContainers.Any(c => c.Id == container.Id)) { return; }
            _intersectionContainers.Add(container);
        }

        internal void Delink(IntersectionVertexContainer container)
        {
            container.Vertex = null;
            _intersectionContainers.RemoveAll(c => c.Id == container.Id);
        }

        private List<DivisionVertexContainer> _divisionContainers = new List<DivisionVertexContainer>();
        internal IReadOnlyList<DivisionVertexContainer> DivisionContainers { get { return _divisionContainers; } }

        internal void Link(DivisionVertexContainer container)
        {
            container.Vertex = this;
            if (_divisionContainers.Any(c => c.Id == container.Id)) { return; }
            _divisionContainers.Add(container);
        }

        internal void Delink(DivisionVertexContainer container)
        {
            container.Vertex = null;
            _divisionContainers.RemoveAll(c => c.Id == container.Id);
        }

        internal static void Link(IntersectionVertexContainer a, IntersectionVertexContainer b)
        {
            if (a.Vertex is null && b.Vertex is null)
            {
                var vertex = new VertexCore();
                vertex.Point = a.Point;
                a.Link(vertex);
                b.Link(vertex);
                return;
            }
            if (a.Vertex is null)
            {
                a.Link(b.Vertex);
                return;
            }
            if (b.Vertex is null)
            {
                b.Link(a.Vertex);
                return;
            }

            if (a.Vertex.Id == b.Vertex.Id) { return; }

            var aVertex = a.Vertex;
            var bVertex = b.Vertex;
            var linkingContainers = b.Vertex.IntersectionContainers.ToArray();
            foreach (var container in linkingContainers)
            {
                container.Delink();
            }

            foreach (var container in linkingContainers)
            {
                aVertex.Link(container);
            }
            b.Vertex = a.Vertex;
        }

        internal static void Link(IntersectionVertexContainer a, DivisionVertexContainer b)
        {
            if (a.Vertex is null && b.Vertex is null)
            {
                var vertex = new VertexCore();
                vertex.Point = a.Point;
                a.Link(vertex);
                b.Link(vertex);
                return;
            }
            if (a.Vertex is null)
            {
                a.Link(b.Vertex);
                return;
            }
            if (b.Vertex is null)
            {
                b.Link(a.Vertex);
                return;
            }

            if (a.Vertex.Id == b.Vertex.Id) { return; }

            var aVertex = a.Vertex;
            var bVertex = b.Vertex;

            var linkingContainers = b.Vertex.DivisionContainers.ToArray();
            foreach (var container in linkingContainers)
            {
                container.Delink();
            }

            foreach (var container in linkingContainers)
            {
                aVertex.Link(container);
            }
            b.Vertex = a.Vertex;
        }

        internal static void Link(DivisionVertexContainer a, DivisionVertexContainer b)
        {
            if (a.Vertex is null && b.Vertex is null)
            {
                var vertex = new VertexCore();
                vertex.Point = a.Point;
                a.Link(vertex);
                b.Link(vertex);
                return;
            }
            if (a.Vertex is null)
            {
                a.Link(b.Vertex);
                return;
            }
            if (b.Vertex is null)
            {
                b.Link(a.Vertex);
                return;
            }

            if (a.Vertex.Id == b.Vertex.Id) { return; }

            var aVertex = a.Vertex;
            var bVertex = b.Vertex;
            var linkingContainers = b.Vertex.DivisionContainers.ToArray();
            foreach (var container in linkingContainers)
            {
                container.Delink();
            }

            foreach (var container in linkingContainers)
            {
                aVertex.Link(container);
            }
            b.Vertex = a.Vertex;
        }

        internal static void Link(VertexCore a, VertexCore b)
        {
            var linkingContainers = b.DivisionContainers.ToArray();
            foreach (var container in linkingContainers)
            {
                container.Delink();
            }
            foreach (var container in linkingContainers)
            {
                a.Link(container);
            }
        }
    }
}
