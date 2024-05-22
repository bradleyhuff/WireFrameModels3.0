using Collections.WireFrameMesh.Basics;
using Operations.PositionRemovals.Interfaces;

namespace Operations.PositionRemovals.Conditionals
{
    internal class EdgeCornerConditionals : IPositionFillConditionals
    {
        public EdgeCornerConditionals(PositionNormal removalPoint, PositionEdge segment) {
            if (removalPoint.PositionObject.Cardinality != 2) { throw new InvalidOperationException($"Cardinality of removal position {removalPoint.PositionObject.Id} must equal 2."); }
            RemovalPoint = removalPoint; Segment = segment; 
        }

        public PositionNormal RemovalPoint { get; }
        public PositionEdge Segment { get; }
        private Position _oppositePoint;
        public Position OppositePoint
        {
            get
            {
                if (_oppositePoint is null)
                {
                    var cluster = GetCluster(RemovalPoint.PositionObject);  
                    _oppositePoint = cluster.Cluster.SingleOrDefault(p => p.Id != Segment.A.PositionObject.Id && p.Id != Segment.B.PositionObject.Id);
                    //Console.WriteLine($"Edge cluster {cluster.Position.Id} {cluster.Cluster.Length} [{string.Join(",", cluster.Cluster.Select(c => c.Cardinality))}] Opposite {_oppositePoint.Cardinality} {_oppositePoint.Id}");
                }
                return _oppositePoint;
            }
        }

        public IEnumerable<PositionTriangle> Triangles
        {
            get
            {
                return Segment.A.PositionObject.Triangles.Concat(Segment.B.PositionObject.Triangles).DistinctBy(t => t.Id).ToArray();
            }
        }

        private EdgeCluster GetCluster(Position pp)
        {
            var cluster1 = pp.PositionNormals[0].Triangles.SelectMany(t => t.Positions.Where(p => p.PositionObject.Cardinality > 1 && p.PositionObject.Id != pp.Id)).Select(p => p.PositionObject).DistinctBy(p => p.Id).ToArray();
            var cluster2 = pp.PositionNormals[1].Triangles.SelectMany(t => t.Positions.Where(p => p.PositionObject.Cardinality > 1 && p.PositionObject.Id != pp.Id)).Select(p => p.PositionObject).DistinctBy(p => p.Id).ToArray();

            return new EdgeCluster() { Position = pp, Cluster = cluster1.IntersectBy(cluster2.Select(c => c.Id), c => c.Id).ToArray() };
        }

        private class EdgeCluster
        {
            public Position Position { get; set; }
            public Position[] Cluster { get; set; }
        }

        public bool AllowFill(PositionNormal a, PositionNormal b, PositionNormal c)
        {
            if (OppositePoint is not null && (OppositePoint.Id == a.PositionObject.Id || OppositePoint.Id == b.PositionObject.Id || OppositePoint.Id == c.PositionObject.Id)) { return true; }
            return a.PositionObject.Cardinality < 2 || b.PositionObject.Cardinality < 2 || c.PositionObject.Cardinality < 2;
        }
    }
}
