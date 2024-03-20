using Collections.WireFrameMesh.Basics;
using Operations.Groupings.Interface;
using Operations.Groupings.Types;

namespace Operations.Groupings.Basics
{
    public class GroupingCollection
    {
        private static int _id = 0;
        private IEnumerable<PositionTriangle> _gridTriangles;
        private IReadOnlyList<GroupingTriangle> _triangles;
        private Dictionary<PositionTriangle, GroupingTriangle> _lookup = new Dictionary<PositionTriangle, GroupingTriangle>();

        public GroupingCollection(IEnumerable<PositionTriangle> triangles)
        {
            _gridTriangles = triangles;
            Id = _id++;
        }

        public GroupingCollection(IEnumerable<GroupingTriangle> triangles)
        {
            _triangles = triangles.ToList();
            Id = _id++;
        }

        public int Id { get; }

        public override int GetHashCode()
        {
            return Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not GroupingCollection) { return false; }
            GroupingCollection compare = (GroupingCollection)obj;
            return Id == compare.Id;
        }

        public IReadOnlyList<GroupingTriangle> Triangles
        {
            get
            {
                if (_triangles is null && _gridTriangles is not null)
                {
                    _triangles = _gridTriangles.Select(GetTriangle).ToList();
                }
                return _triangles;
            }
        }

        private List<GroupEdge> _perimeterEdges;

        public IReadOnlyList<GroupEdge> PerimeterEdges
        {
            get
            {
                if (_perimeterEdges is null)
                {
                    _perimeterEdges = GetPerimeterEdges().ToList();
                }
                return _perimeterEdges;
            }
        }

        private IEnumerable<GroupEdge> GetPerimeterEdges()
        {
            var table = Triangles.Select(v => new KeyValuePair<int, bool>(v.Id, true)).ToDictionary(p => p.Key, p => p.Value);
            foreach (var triangle in Triangles)
            {
                if (!triangle.ABadjacents.Any() ||
                    triangle.ABadjacents.All(t => !table.ContainsKey(t.Id))) { yield return new GroupEdge(triangle.A, triangle.B); }

                if (!triangle.BCadjacents.Any() ||
                    triangle.BCadjacents.All(t => !table.ContainsKey(t.Id))) { yield return new GroupEdge(triangle.B, triangle.C); }

                if (!triangle.CAadjacents.Any() ||
                    triangle.CAadjacents.All(t => !table.ContainsKey(t.Id))) { yield return new GroupEdge(triangle.C, triangle.A); }
            }
        }

        private GroupingTriangle GetTriangle(PositionTriangle triangle)
        {
            if (!_lookup.ContainsKey(triangle)) { _lookup[triangle] = new GroupingTriangle(triangle, _lookup); }
            return _lookup[triangle];
        }

        public IEnumerable<GroupingCollection> ExtractSurfaces()
        {
            return Extract(new Surface());
        }
        public IEnumerable<GroupingCollection> ExtractFaces()
        {
            return Extract(new Face());
        }
        public IEnumerable<GroupingCollection> ExtractClusters()
        {
            return Extract(new Cluster());
        }
        public IEnumerable<GroupingCollection> ExtractPlanars()
        {
            return Extract(new Planar());
        }

        internal IEnumerable<GroupingCollection> Extract(IGrouping grouping)
        {
            List<GroupingCollection> groups = new List<GroupingCollection>();
            GroupingTriangle[] array = GetSortedArray(grouping);
            if (!array.Any()) { return groups; }

            var table = array.Select(v => new KeyValuePair<int, bool>(v.Id, true)).ToDictionary(p => p.Key, p => p.Value);

            int startIndex = 0;
            foreach (var element in array) { element.Spanned = false; }
            while (true)
            {
                var first = GetStart(array, ref startIndex);
                if (first is null) { break; }
                grouping.FirstTriangle = first;
                var newGroupTriangles = PullGroup(first, table, grouping).ToArray();
                var newGroup = new GroupingCollection(newGroupTriangles);
                groups.Add(newGroup);
            }
            foreach (var element in array) { element.Spanned = false; }
            return groups;
        }

        private GroupingTriangle[] GetSortedArray(IGrouping grouping)
        {
            foreach (var element in Triangles) { grouping.SeedSet(element); }
            return Triangles.OrderBy(e => e.Seed).ToArray();
        }

        private GroupingTriangle GetStart(GroupingTriangle[] input, ref int index)
        {
            int count = 0;

            while (input[index].Spanned)
            {
                index++;
                count++;
                index = index % input.Length;
                if (count > input.Length) { return null; }
            }

            return input[index];
        }

        private IEnumerable<GroupingTriangle> PullGroup(GroupingTriangle first,
            Dictionary<int, bool> table, IGrouping grouping)
        {
            List<GroupingTriangle> list = new List<GroupingTriangle>() { first };

            do
            {
                list = GetSpanning(list, table, first, grouping).ToList();
                foreach (var element in list)
                {
                    yield return element;
                }
            }
            while (list.Count > 0);
        }

        private IEnumerable<GroupingTriangle> GetSpanning(IEnumerable<GroupingTriangle> input, Dictionary<int, bool> table, GroupingTriangle first,
            IGrouping grouping)
        {
            if (!first.Spanned && table.ContainsKey(first.Id))
            {
                first.Spanned = true;
                yield return first;
            }

            foreach (var element in input)
            {
                foreach (var node in element.Adjacents.Where(grouping.EdgeFilter).SelectMany(e => e.Select(t => t)).
                    Where(t => !t.Spanned && table.ContainsKey(t.Id) && grouping.TriangleFilter(element, t)))
                {
                    node.Spanned = true;
                    yield return node;
                }
            }
        }
    }
}
