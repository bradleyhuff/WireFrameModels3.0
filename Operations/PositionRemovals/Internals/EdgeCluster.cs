using Collections.WireFrameMesh.Basics;

namespace Operations.PositionRemovals.Internals
{
    internal class EdgeCluster
    {
        public EdgeCluster(Position p)
        {
            GetCluster(p);
        }

        public Position Position { get; private set; }
        public Position[] Cluster { get; private set; }

        private void GetCluster(Position pp)
        {
            if (pp.Cardinality != 2) { throw new InvalidOperationException($"Position {pp.Id} is not an edge with cardinality {pp.Cardinality}"); }
            var cluster1 = pp.PositionNormals[0].Triangles.SelectMany(t => t.Positions.Where(p => p.PositionObject.Cardinality > 1 && p.PositionObject.Id != pp.Id)).Select(p => p.PositionObject).DistinctBy(p => p.Id).ToArray();
            var cluster2 = pp.PositionNormals[1].Triangles.SelectMany(t => t.Positions.Where(p => p.PositionObject.Cardinality > 1 && p.PositionObject.Id != pp.Id)).Select(p => p.PositionObject).DistinctBy(p => p.Id).ToArray();

            Position = pp;
            Cluster = cluster1.IntersectBy(cluster2.Select(c => c.Id), c => c.Id).ToArray();
        }
    }
}
