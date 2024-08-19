using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
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

        internal GroupingCollection(IEnumerable<GroupingTriangle> triangles)
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

        public static IEnumerable<GroupingCollection> ExtractSurfaces(IEnumerable<PositionTriangle> triangles)
        {
            var grouping = new GroupingCollection(triangles);
            return grouping.ExtractSurfaces();
        }
        public static IEnumerable<GroupingCollection> ExtractSurfaces(GroupingCollection collection)
        {
            var grouping = new GroupingCollection(collection.GroupingTriangles);
            return grouping.ExtractSurfaces();
        }
        public static IEnumerable<GroupingCollection> ExtractFaces(IEnumerable<PositionTriangle> triangles)
        {
            var grouping = new GroupingCollection(triangles);
            return grouping.ExtractFaces();
        }
        public static IEnumerable<GroupingCollection> ExtractFaces(GroupingCollection collection)
        {
            var grouping = new GroupingCollection(collection.GroupingTriangles);
            return grouping.ExtractFaces();
        }
        public static IEnumerable<GroupingCollection> ExtractClusters(IEnumerable<PositionTriangle> triangles)
        {
            var grouping = new GroupingCollection(triangles);
            return grouping.ExtractClusters();
        }
        public static IEnumerable<GroupingCollection> ExtractClusters(GroupingCollection collection)
        {
            var grouping = new GroupingCollection(collection.GroupingTriangles);
            return grouping.ExtractClusters();
        }
        public static IEnumerable<GroupingCollection> ExtractPlanars(IEnumerable<PositionTriangle> triangles)
        {
            var grouping = new GroupingCollection(triangles);
            return grouping.ExtractPlanars();
        }
        public static IEnumerable<GroupingCollection> ExtractPlanars(GroupingCollection collection)
        {
            var grouping = new GroupingCollection(collection.GroupingTriangles);
            return grouping.ExtractPlanars();
        }

        public static IEnumerable<GroupingCollection> ExtractFolds(IEnumerable<PositionTriangle> triangles)
        {
            var grouping = new GroupingCollection(triangles);
            return grouping.ExtractFolds();
        }
        public static IEnumerable<GroupingCollection> ExtractFolds(GroupingCollection collection)
        {
            var grouping = new GroupingCollection(collection.GroupingTriangles);
            return grouping.ExtractFolds();
        }

        public IWireFrameMesh Create()
        {
            return Create(WireFrameMesh.Create());
        }

        public IWireFrameMesh Create(IWireFrameMesh mesh)
        {
            foreach (var triangle in GroupingTriangles)
            {
                triangle.AddWireFrameTriangle(mesh);

            }
            return mesh;
        }

        private IReadOnlyList<GroupingTriangle> GroupingTriangles
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

        public IEnumerable<PositionTriangle> Triangles
        {
            get
            {
                return GroupingTriangles.Select(t => t.PositionTriangle);
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

        private List<Position> _internalPoints;

        public IReadOnlyList<Position> InternalPoints
        {
            get
            {
                if (_internalPoints is null)
                {
                    _internalPoints = GetInternalPoints().DistinctBy(p => p.Id).ToList();
                }
                return _internalPoints;
            }
        }

        private IEnumerable<GroupEdge> GetPerimeterEdges()
        {
            var table = GroupingTriangles.Select(v => new KeyValuePair<int, bool>(v.PositionTriangle.Id, true)).ToDictionary(p => p.Key, p => p.Value);
            foreach (var triangle in GroupingTriangles)
            {
                if (!triangle.PositionTriangle.ABadjacents.Any() ||
                    triangle.PositionTriangle.ABadjacents.All(t => !table.ContainsKey(t.Id))) { yield return new GroupEdge(triangle.A, triangle.B); }

                if (!triangle.PositionTriangle.BCadjacents.Any() ||
                    triangle.PositionTriangle.BCadjacents.All(t => !table.ContainsKey(t.Id))) { yield return new GroupEdge(triangle.B, triangle.C); }

                if (!triangle.PositionTriangle.CAadjacents.Any() ||
                    triangle.PositionTriangle.CAadjacents.All(t => !table.ContainsKey(t.Id))) { yield return new GroupEdge(triangle.C, triangle.A); }
            }
        }

        private IEnumerable<Position> GetInternalPoints()
        {
            var perimeterPoints = PerimeterEdges.SelectMany(e => e.Positions.Select(p => p.PositionObject)).
                DistinctBy(p => p.Id).Select(p => new KeyValuePair<int, bool>(p.Id, true)).ToDictionary(p => p.Key, p => p.Value);

            var allPoints = GroupingTriangles.SelectMany(t => t.PositionTriangle.Positions.Select(p => p.PositionObject)).DistinctBy(p => p.Id).ToArray();

            return allPoints.Where(p => !perimeterPoints.ContainsKey(p.Id));
        }

        private GroupingTriangle GetTriangle(PositionTriangle triangle)
        {
            if (!_lookup.ContainsKey(triangle)) { _lookup[triangle] = new GroupingTriangle(triangle, _lookup); }
            return _lookup[triangle];
        }

        private IEnumerable<GroupingCollection> ExtractSurfaces()
        {
            return Extract(new Surface());
        }
        private IEnumerable<GroupingCollection> ExtractFaces()
        {
            return Extract(new Face());
        }
        private IEnumerable<GroupingCollection> ExtractClusters()
        {
            return Extract(new Cluster());
        }
        private IEnumerable<GroupingCollection> ExtractPlanars()
        {
            return Extract(new Planar());
        }
        private IEnumerable<GroupingCollection> ExtractFolds()
        {
            return Extract(new Fold());
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
            foreach (var element in GroupingTriangles) { grouping.SeedSet(element); }
            return GroupingTriangles.OrderBy(e => e.Seed).ToArray();
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
