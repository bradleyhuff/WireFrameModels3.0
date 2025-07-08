using Collections.WireFrameMesh.Interfaces;
using Operations.Groupings.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
