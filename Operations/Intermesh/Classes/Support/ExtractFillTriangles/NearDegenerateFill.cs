using BaseObjects;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Console = BaseObjects.Console;
using System.Collections.Generic;

namespace Operations.Intermesh.Classes.Support.ExtractFillTriangles
{
    public static class Logging
    {
        public static bool ShowLog { get; set; }
    }
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

                var angleLinks = Links.
                    Select(l => new { link = l, angle = Vector3D.Angle(l.Point - Point, startLink.Point - Point) }).OrderBy(l => l.angle);
                if (Logging.ShowLog)
                {
                    Console.WriteLine($"Iterate: {Id} => [{string.Join(", ", angleLinks.Select(al => $"{al.link.Id} {al.angle}"))}]");
                }

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

        private class PerimeterNode
        {
            public PerimeterNode(Node node)
            {
                Id = node.Id;
            }

            public int Id { get; }

            private List<PerimeterNode> _links = new List<PerimeterNode>();
            public IReadOnlyList<PerimeterNode> Links
            {
                get { return _links; }
            }

            private bool AddLink(PerimeterNode link)
            {
                if (_links.Any(l => l.Id == link.Id)) { return false; }
                _links.Add(link);
                return true;
            }
            private bool RemoveLink(PerimeterNode link)
            {
                return _links.Remove(link);
            }

            public static bool Link(PerimeterNode a, PerimeterNode b)
            {
                bool linkedA = a.AddLink(b);
                bool linkedB = b.AddLink(a);
                return linkedA && linkedB;
            }

            public static bool Unlink(PerimeterNode a, PerimeterNode b)
            {
                bool removedA = a.RemoveLink(b);
                bool removedB = b.RemoveLink(a);
                return removedA && removedB;
            }
        }

        static int _id = 0;
        private static object lockObject = new object();
        private Dictionary<int, Node> _referenceMapping;
        private Dictionary<int, Node> _nodeMapping;
        private Combination2Dictionary<bool> _perimeterSegments = new Combination2Dictionary<bool>();
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
            _nodeMapping = new Dictionary<int, Node>();
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
                if (a.IsPerimeter && b.IsPerimeter) { _perimeterSegments[a.Id, b.Id] = true; }

                _segments.Add(new LineSegment3D(getPoint(edge.Item1), getPoint(edge.Item2)));
            }

            var perimeterNodeReference = LinkPerimeterNodes(_perimeterSegments.Keys.ToArray());

            // get unbranchedPerimeterSegments

            if (perimeterNodeReference.Values.Any(p => p.Links.Count > 2))
            {
                RemoveBranchedPerimeterSegments(perimeterNodeReference);
                _perimeterSegments = ConvertToPerimeterSegments(perimeterNodeReference);

                if (perimeterNodeReference.Any(p => p.Value.Links.Count > 2)) { Console.WriteLine($"Branched perimeter links. Perimeters {_perimeterSegments.Count}: {perimeterNodeReference.Count} => {perimeterNodeReference.Count(p => p.Value.Links.Count > 2)}"); }
            }

            //foreach (var segment in _perimeterSegments.Keys)
            //{
            //    var nodeA = _referenceMapping.Values.Single(n => n.Id == segment.A);
            //    //var nodeB = _referenceMapping[segment.B];

            //    if (nodeA.Links.Count(l => _perimeterSegments.ContainsKey(nodeA.Id, l.Id)) > 2 && _perimeterSegments.Count > 10) { 
            //        Console.WriteLine($"Branched perimeter links. Perimeters {_perimeterSegments.Count}"); 
            //    }
            //}

            if (Logging.ShowLog) { Console.WriteLine($"Perimeter Segments {string.Join(",", _perimeterSegments.Keys)}"); }
            if (Logging.ShowLog) { Console.WriteLine($"Reference links\n{string.Join("\n", _referenceMapping.Values.Select(r => $"{r.Id} links {r.Links.Count} perimeter {r.IsPerimeter} vertex {r.IsVertex} point {r.Point}"))}"); }
        }

        private Dictionary<int, PerimeterNode>  LinkPerimeterNodes(Combination2[] perimeterSegments)
        {
            var table = new Dictionary<int, PerimeterNode>();

            var segments = perimeterSegments.Select(p => (GetPerimeterNode(_nodeMapping[p.A], table), GetPerimeterNode(_nodeMapping[p.B], table))).ToArray();

            foreach (var segment in segments)
            {
                PerimeterNode.Link(segment.Item1, segment.Item2);
            }

            return table;
        }

        private PerimeterNode GetPerimeterNode(Node node, Dictionary<int, PerimeterNode> table)
        {
            if (!table.ContainsKey(node.Id)) { table[node.Id] = new PerimeterNode(node); }
            return table[node.Id];
        }

        private Combination2Dictionary<bool> ConvertToPerimeterSegments(Dictionary<int, PerimeterNode> input)
        {
            var output = new Combination2Dictionary<bool>();

            foreach(var node in input.Values)
            {
                foreach (var link in node.Links)
                {
                    output[node.Id, link.Id] = true;
                }
            }

            return output;
        }

        private void RemoveBranchedPerimeterSegments(Dictionary<int, PerimeterNode> input)
        {
            //
            while (true)
            {
                var pairs = new List<(PerimeterNode, PerimeterNode)>();
                var wasUnlinked = false;
                foreach (var branchNode in input.Values.Where(n => n.Links.Count > 2 && n.Links.Any(l => l.Links.Count == 2) && n.Links.Any(l => l.Links.Count > 2)))
                {
                    var pair = branchNode.Links.First(l => l.Links.Count > 2);
                    pairs.Add(new(branchNode, pair));
                }
                var distinctPairs = pairs.DistinctBy(p => new Combination2(p.Item1.Id, p.Item2.Id), new Combination2Comparer());
                foreach (var pair in distinctPairs)
                {
                    PerimeterNode.Unlink(pair.Item1, pair.Item2);
                    wasUnlinked = true;
                }
                if (!wasUnlinked) { break; }
            }
        }

        private Node GetReferenceMap(T reference, Point3D point, int id)
        {
            if (!_referenceMapping.ContainsKey(id)) 
            { 
                var node = new Node(point, reference);
                _referenceMapping[id] = node;
                _nodeMapping[node.Id] = node;
            }
            return _referenceMapping[id];
        }

        private bool FillIntersects((Node, Node, Node) s)
        {
            var fill = new Triangle3D(s.Item1.Point, s.Item2.Point, s.Item3.Point);
            if (_segments.Any(s => { var intersection = fill.LineSegmentIntersection(s); return intersection is not null && intersection != s; })) { return true; }
            if (_fillTriangles.Any(f => f == fill)) { return true; }
            return _fillTriangles.Any(f => Triangle3D.LineSegmentIntersections(f, fill).Any(i => !fill.Edges.Any(e => e == i)));
        }

        private IEnumerable<(T, T, T)> Sweep(Node node)
        {
            if (Logging.ShowLog) { Console.WriteLine($"Sweep {node.Id}"); }
            foreach (var set in node.Iterate().Where(s => !FillIntersects(s)).ToArray())
            {
                if(_perimeterSegments.ContainsKey(set.Item1.Id, set.Item2.Id))
                {
                    Node.Unlink(set.Item1, set.Item2);
                    _perimeterSegments.Remove(set.Item1.Id, set.Item2.Id);
                    if (Logging.ShowLog) { Console.WriteLine($"Unlink {set.Item1.Id}, {set.Item2.Id}"); }
                }
                else
                {
                    _perimeterSegments[set.Item1.Id, set.Item2.Id] = true;
                }

                if (_perimeterSegments.ContainsKey(set.Item1.Id, set.Item3.Id))
                {
                    Node.Unlink(set.Item1, set.Item3);
                    _perimeterSegments.Remove(set.Item1.Id, set.Item3.Id);
                    if (Logging.ShowLog) { Console.WriteLine($"Unlink {set.Item1.Id}, {set.Item3.Id}"); }
                }
                else
                {
                    _perimeterSegments[set.Item1.Id, set.Item3.Id] = true;
                }

                Node.Link(set.Item2, set.Item3);
                _perimeterSegments[set.Item2.Id, set.Item3.Id] = true;
                if (Logging.ShowLog) { Console.WriteLine($"Link {set.Item2.Id}, {set.Item3.Id}"); }
                set.Item2.IsPerimeter = true;
                set.Item3.IsPerimeter = true;

                if (Logging.ShowLog)
                {
                    Console.WriteLine($"     Fill [{set.Item1.Id} {set.Item2.Id} {set.Item3.Id}]");
                    Console.WriteLine($"     Perimeter Segments {string.Join(",", _perimeterSegments.Keys)}");
                    Console.WriteLine($"     Reference links\n     {string.Join("\n     ", _referenceMapping.Values.Select(r => $"{r.Id} links {r.Links.Count} perimeter {r.IsPerimeter} vertex {r.IsVertex} point {r.Point}"))}");
                }
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
                _perimeterSegments.Remove(node.Id, otherLink.Id);
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
                if (!pulled && !listDecreased)
                { /*throw new Exception($"Loop 3 non-terminating. List {list.Count()}");*/
                    Console.WriteLine($"Loop 3 non-terminating. List {list.Count()}  Collinear {Point3D.AreCollinear(list.Select(e => e.Point).ToArray())}"); break;
                }
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
                if (!pulled && !listDecreased)
                { /*throw new Exception($"Loop 2 non-terminating. List {list.Count()}");*/
                    Console.WriteLine($"Loop 2 non-terminating. List {list.Count()}  Collinear {Point3D.AreCollinear(list.Select(e => e.Point).ToArray())}"); break;
                }
            }
        }
    }
}
