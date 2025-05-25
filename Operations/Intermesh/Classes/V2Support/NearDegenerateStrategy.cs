namespace Operations.Intermesh.Classes.V2Support
{
    internal class NearDegenerateStrategy<T>
    {
        private class Node<TT>
        {
            static int _id = 0;
            public Node(TT reference)
            {
                Reference = reference;
                Id = _id++;
            }

            public int Id { get; }

            public TT Reference { get; }
            public bool IsVertex { get; set; }
            public Node<TT> Left { get; set; }
            public Node<TT> Right { get; set; }
            public bool IsRemoved { get; set; }

            private List<Node<TT>> _links = new List<Node<TT>>();

            public IReadOnlyList<Node<TT>> Links
            {
                get { return _links; }
            }
            private bool AddLink(Node<TT> link)
            {
                if (_links.Any(l => l.Id == link.Id)) { return false; }
                _links.Add(link);
                return true;
            }
            private bool RemoveLink(Node<TT> link)
            {
                return _links.Remove(link);
            }

            public static bool Link(Node<TT> a, Node<TT> b)
            {
                bool linkedA = a.AddLink(b);
                bool linkedB = b.AddLink(a);
                return linkedA && linkedB;
            }

            public static bool Unlink(Node<TT> a, Node<TT> b)
            {
                bool removedA = a.RemoveLink(b);
                bool removedB = b.RemoveLink(a);
                return removedA && removedB;
            }
        }

        static int _id = 0;
        private Dictionary<int, Node<T>> _referenceMapping;
        public NearDegenerateStrategy(IEnumerable<(T, T)> edges, Func<T, int> getId, Func<T, bool> isVertex)
        {
            Id = _id++;
            BuildReferenceMapping(edges, getId, isVertex);
        }

        public int Id { get; }

        private List<(T, T, T)> _pullList;

        public IEnumerable<(T, T, T)> GetFill()
        {
            if (_pullList is null)
            {
                _pullList = Pull().ToList();
            }
            return _pullList;
        }

        private void BuildReferenceMapping(IEnumerable<(T, T)> edges, Func<T, int> getId, Func<T, bool> isVertex)
        {
            _referenceMapping = new Dictionary<int, Node<T>>();

            foreach (var edge in edges)
            {
                var a = GetReferenceMap(edge.Item1, getId(edge.Item1));
                a.IsVertex = isVertex(edge.Item1);
                var b = GetReferenceMap(edge.Item2, getId(edge.Item2));
                b.IsVertex = isVertex(edge.Item2);
                Node<T>.Link(a, b);
            }
        }

        private Node<T> GetReferenceMap(T reference, int id)
        {
            if (!_referenceMapping.ContainsKey(id)) { _referenceMapping[id] = new Node<T>(reference); }
            return _referenceMapping[id];
        }

        private bool RemoveNode(Node<T> node, out (T, T, T) output)
        {
            if (!node.IsVertex || node.Links.Count != 2)
            {
                output = (default, default, default);
                return false;
            }

            node.Left = node.Links[0];
            node.Right = node.Links[1];

            node.Left.IsVertex = true;
            node.Right.IsVertex = true;
            Node<T>.Unlink(node, node.Left);
            Node<T>.Unlink(node, node.Right);

            if (node.Left.Links.Count == 1 && 
                node.Right.Links.Count == 1 && 
                node.Left.Links[0] == node.Right && 
                node.Right.Links[0] == node.Left)
            {
                Node<T>.Unlink(node.Left, node.Right);
                node.Left.IsRemoved = true;
                node.Right.IsRemoved = true;
            }
            else
            {
                Node<T>.Link(node.Left, node.Right);
            }

            node.IsRemoved = true;
            output = (node.Reference, node.Left.Reference, node.Right.Reference);
            return true;
        }

        private IEnumerable<(T, T, T)> Pull()
        {
            var list = _referenceMapping.Values.ToList();
            if (!list.Any()) { yield break; }
            int start = list.Count();
            int count = 0;
            do
            {
                foreach (var element in list)
                {
                    (T, T, T) output;
                    if (RemoveNode(element, out output)) { yield return output; }
                }
                list.RemoveAll(n => n.IsRemoved);
                count++;
                if (count > start) { 
                    Console.WriteLine($"{Id} Near Degenerate Infinite loop max links per node {_referenceMapping.Values.MaxBy(n => n.Links.Count)?.Links?.Count ?? 0}"); 
                    break; 
                }
            } while (list.Any());
        }
    }
}
