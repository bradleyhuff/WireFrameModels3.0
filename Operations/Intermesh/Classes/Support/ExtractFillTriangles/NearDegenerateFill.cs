using BasicObjects.GeometricObjects;

namespace Operations.Intermesh.Classes.Support.ExtractFillTriangles
{
    internal class NearDegenerateFill<T>
    {
        private class Node
        {
            static int _id = 0;
            private static object lockObject = new object();
            public Node(Point3D point, T reference)
            {
                Reference = reference;
                Point = point;
                lock (lockObject)
                {
                    Id = _id++;
                }
            }

            public int Id { get; }
            public Point3D Point { get; }

            public T Reference { get; }
            public bool IsPerimeter { get; set; }
            public bool IsVertex { get; set; }

            private List<Node> _links = new List<Node>();

            public IReadOnlyList<Node> Links
            {
                get { return _links; }
            }

            public IEnumerable<(Node, Node, Node)> Iterate()
            {
                if (!Links.Any()) yield break;
                var startLink = Links.First(l => l.IsPerimeter);
                var orderedLinks = Links.
                    Select(l => new { link = l, angle = Vector3D.Angle(l.Point - Point, startLink.Point - Point) }).
                    OrderBy(l => l.angle).
                    Select(l => l.link).ToArray();

                for (int i = 0; i < orderedLinks.Length - 1; i++)
                {
                    var first = orderedLinks[i];
                    var next = orderedLinks[i + 1];

                    yield return (this, first, next);
                }
            }

            private bool AddLink(Node link)
            {
                if (_links.Any(l => l.Id == link.Id)) { return false; }
                _links.Add(link);
                return true;
            }
            private bool RemoveLink(Node link)
            {
                return _links.Remove(link);
            }

            public static bool Link(Node a, Node b)
            {
                bool linkedA = a.AddLink(b);
                bool linkedB = b.AddLink(a);
                return linkedA && linkedB;
            }

            public static bool Unlink(Node a, Node b)
            {
                bool removedA = a.RemoveLink(b);
                bool removedB = b.RemoveLink(a);
                return removedA && removedB;
            }
        }

        static int _id = 0;
        private static object lockObject = new object();
        private Dictionary<int, Node> _referenceMapping;
        private List<LineSegment3D> _segments = new List<LineSegment3D>();
        private List<(T, T, T)> _pullList;
        private List<Triangle3D> _fillTriangles = new List<Triangle3D>();
        private Func<T, int> _getId;

        public NearDegenerateFill(IEnumerable<(T, T)> edges, Func<T, Point3D> getPoint, Func<T, int> getId, Func<T, bool> isVertex, Func<T, bool> isPerimeter)
        {
            lock (lockObject)
            {
                Id = _id++;
            }
            BuildReferenceMapping(edges, getPoint, getId, isVertex, isPerimeter);
        }

        public int Id { get; }

        private void BuildReferenceMapping(IEnumerable<(T, T)> edges, Func<T, Point3D> getPoint, Func<T, int> getId, Func<T, bool> isVertex, Func<T, bool> isPerimeter)
        {
            _referenceMapping = new Dictionary<int, Node>();
            _getId = getId;

            foreach (var edge in edges)
            {
                var a = GetReferenceMap(edge.Item1, getPoint(edge.Item1), getId(edge.Item1));
                a.IsPerimeter = isPerimeter(edge.Item1);
                a.IsVertex = isVertex(edge.Item1);
                var b = GetReferenceMap(edge.Item2, getPoint(edge.Item2), getId(edge.Item2));
                b.IsPerimeter = isPerimeter(edge.Item2);
                b.IsVertex = isVertex(edge.Item2);
                Node.Link(a, b);

                _segments.Add(new LineSegment3D(getPoint(edge.Item1), getPoint(edge.Item2)));
            }
        }

        private Node GetReferenceMap(T reference, Point3D point, int id)
        {
            if (!_referenceMapping.ContainsKey(id)) { _referenceMapping[id] = new Node(point, reference); }
            return _referenceMapping[id];
        }

        private bool FillIntersects((Node, Node, Node) s)
        {
            var fill = new Triangle3D(s.Item1.Point, s.Item2.Point, s.Item3.Point);
            if (_segments.Any(s => { var intersection = fill.LineSegmentIntersection(s); return intersection is not null && intersection != s; })) { return true; }

            return _fillTriangles.Any(f => Triangle3D.LineSegmentIntersections(f, fill).Any(i => !fill.Edges.Any(e => e == i)));
        }

        private IEnumerable<(T, T, T)> Sweep(Node node)
        {
            foreach (var set in node.Iterate().Where(s => !FillIntersects(s)).ToArray())
            {
                if (set.Item2.IsPerimeter) Node.Unlink(set.Item1, set.Item2);
                if (set.Item3.IsPerimeter) Node.Unlink(set.Item1, set.Item3);
                Node.Link(set.Item2, set.Item3);
                set.Item2.IsPerimeter = true;
                set.Item3.IsPerimeter = true;

                var fill = new Triangle3D(set.Item1.Point, set.Item2.Point, set.Item3.Point);
                _fillTriangles.Add(fill);
                yield return (set.Item1.Reference, set.Item2.Reference, set.Item3.Reference);
            }

            if (!node.Links.Any()) { _referenceMapping.Remove(_getId(node.Reference)); }
        }

        private void RemoveSingleLinkedNodes()
        {
            var singleLinkedNodes = _referenceMapping.Where(r => r.Value.Links.Count() == 1).ToArray();
            foreach (var pair in singleLinkedNodes)
            {
                var node = pair.Value;
                if (!node.Links.Any()) { continue; }
                var otherLink = node.Links[0];
                Node.Unlink(node, otherLink);
                _referenceMapping.Remove(pair.Key);
                if (!otherLink.Links.Any()) { _referenceMapping.Remove(_getId(otherLink.Reference)); }
            }
        }

        public IEnumerable<(T, T, T)> GetFill()
        {
            if (_pullList is null)
            {
                _pullList = Pull().ToList();
            }
            return _pullList;
        }

        private IEnumerable<(T, T, T)> Pull()
        {
            var list = _referenceMapping.Values.ToList();
            if (!list.Any()) { yield break; }

            while (true)
            {
                var pulled = false;
                foreach (var node in list.Where(n => n.Links.Count() > 2 && n.IsPerimeter).ToArray())
                {
                    foreach (var sweep in Sweep(node)) { pulled = true; yield return sweep; }
                }
                RemoveSingleLinkedNodes();
                var newList = _referenceMapping.Values.ToList();
                var listDecreased = newList.Count < list.Count;
                list = newList;
                if (!list.Any(n => n.Links.Count() > 2 && n.IsPerimeter)) { break; }
                if (!pulled && !listDecreased) { throw new Exception($"Loop 3 non-terminating. List {list.Count()}"); }
            }

            while (true)
            {
                var pulled = false;
                foreach (var node in list.Where(n => n.Links.Count() == 2).ToArray())
                {
                    if (node.Links.Count() != 2) { continue; }
                    foreach (var sweep in Sweep(node)) { pulled = true; yield return sweep; }
                }
                RemoveSingleLinkedNodes();
                var newList = _referenceMapping.Values.ToList();
                var listDecreased = newList.Count < list.Count;
                list = newList;
                if (!list.Any()) { break; }
                if (!pulled && !listDecreased) { throw new Exception($"Loop 2 non-terminating. List {list.Count()}"); }
            }
        }
    }
}
