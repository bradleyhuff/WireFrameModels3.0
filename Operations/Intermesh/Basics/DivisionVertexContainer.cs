using BasicObjects.GeometricObjects;

namespace Operations.Intermesh.Basics
{
    public class DivisionVertexContainer
    {
        private static int _id = 0;
        public DivisionVertexContainer()
        {
            Id = _id++;
        }

        public int Id { get; }

        private IntermeshDivision _division;
        private char _tag;
        public DivisionVertexContainer(IntermeshDivision division, char tag) : this() { _division = division; _tag = tag; }
        public VertexCore Vertex { get; set; }
        public char Tag { get { return _tag; } }
        public IntermeshDivision Division { get { return _division; } }
        public Point3D Point
        {
            get
            {
                if (Vertex is not null) { return Vertex.Point; }
                if (_tag == 'a') { return _division.Division.Start; }
                return _division.Division.End;
            }
        }

        public DivisionVertexContainer Opposite
        {
            get
            {
                if (Division.VertexA != this) { return Division.VertexB; }
                return Division.VertexA;
            }
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

        private IEnumerable<DivisionVertexContainer> GetChildren()
        {
            if (Vertex is null) { yield break; }

            foreach (var child in Vertex.DivisionContainers.Select(c => c.Division).
                Where(l => l.Id != Division.Id && !l.VerticiesAB.Any(v => v.Vertex is null)).
                Select(l => l.VerticiesAB.Where(v => v.Vertex.Id != Vertex.Id)).Where(c => c.Count() == 1))
            {
                foreach (var element in child) { yield return element; }
            }
        }

        public IEnumerable<DivisionVertexContainer> GetTree()
        {
            return GetTreeUntil(v => false);
        }

        public IEnumerable<DivisionVertexContainer> GetTreeUntil(Func<DivisionVertexContainer, bool> stop)
        {
            Dictionary<int, bool> childrenTable = new Dictionary<int, bool>();
            var children = new DivisionVertexContainer[] { this };
            children = children.Where(c => !stop(c)).ToArray();

            while (children.Any())
            {
                foreach (var child in children) { childrenTable[child.Id] = true; yield return child; }
                var children0 = children.SelectMany(c => c.GetChildren()).DistinctBy(c => c.Id).Where(c => !stop(c));
                children = children0.Where(c => !childrenTable.ContainsKey(c.Id)).ToArray();
            }
        }
    }
}
