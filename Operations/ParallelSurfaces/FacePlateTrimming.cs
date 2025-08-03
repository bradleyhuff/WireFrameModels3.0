using BaseObjects;
using Collections.Threading;
using Operations.Intermesh;
using Operations.ParallelSurfaces.Basics;
using Operations.SetOperators;
using Console = BaseObjects.Console;
using Operations.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles;

namespace Operations.ParallelSurfaces
{
    public static class FacePlateTrimming
    {
        public static void PlateTrim(this IEnumerable<ClusterSet> clusters)
        {
            DateTime start = DateTime.Now;
            Mode.ThreadedRun = true;
            ConsoleLog.Push("Cluster plate trim");

            var clusterState = new ClusterState();
            var clusterIterator = new Iterator<ClusterSet>(clusters.ToArray());
            clusterIterator.Run<ClusterState, ClusterThread>(ClusterAction, clusterState, 1, 1);

            ConsoleLog.Pop();
            ConsoleLog.WriteLine($"Cluster plate trim: Clusters {clusters.Count()} Simple {SimpleFillStrategy.Count} NearDegenerate {NearDegenerateFillStrategy.Count} Complex {ComplexFillStrategy.Count} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
            Mode.ThreadedRun = true;
        }

        private static void ClusterAction(ClusterSet cluster, ClusterThread threadState, ClusterState state)
        {
            DateTime start = DateTime.Now;

            var disjointSets = cluster.Faces.Select(f => f.FacePlate).DisjointGroupsCombined().ToArray();

            try
            {
                cluster.TrimmedClusterGrid = WireFrameMesh.Create();
                GridIntermesh.ClusterId = cluster.Id;
                var difference = cluster.Cluster.Create().Difference(disjointSets.First());

                int index = 0;
                foreach (var set in disjointSets.Skip(1))
                {
                    difference = difference.Difference(set);
                    index++;
                }
                cluster.TrimmedClusterGrid = difference;
                if(!cluster.TrimmedClusterGrid.Triangles.Any()) cluster.OriginalClusterGrid = cluster.Cluster.Create();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, ConsoleColor.Red);
            }

            Console.WriteLine($"Cluster {cluster.Id} Disjoint sets {disjointSets.Length} Triangles {cluster.TrimmedClusterGrid.Triangles.Count} Thread {threadState.ThreadId} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.", cluster.TrimmedClusterGrid.Triangles.Count > 0 ? ConsoleColor.Cyan: ConsoleColor.Red);
        }
        private class ClusterThread : BaseThreadState
        {
        }

        private class ClusterState : BaseState<ClusterThread>
        {
        }
    }
}
