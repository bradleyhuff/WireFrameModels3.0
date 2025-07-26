using Collections.WireFrameMesh.Interfaces;
using Operations.Groupings.Basics;

namespace Operations.ParallelSurfaces.Basics
{
    public class ClusterSet
    {
        private static int _id = 0;
        private static object lockObject = new object();
        public ClusterSet(GroupingCollection cluster) 
        { 
            Cluster = cluster;
            lock (lockObject)
            {
                Id = _id++;
            }
        }
        public GroupingCollection Cluster { get; }
        public IWireFrameMesh TrimmedClusterGrid { get; set; }
        public IWireFrameMesh OriginalClusterGrid { get; set; }

        public int Id { get; }
        public List<FaceSet> Faces { get; set; } = new List<FaceSet>();
    }
}
