using BaseObjects;
using Collections.Threading;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Collections.WireFrameMesh.Interfaces;
using FileExportImport;
using Operations.Basics;
using Operations.Groupings.Basics;
using Operations.Intermesh;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles;
using Operations.ParallelSurfaces.Basics;
using Operations.SetOperators;
using Console = BaseObjects.Console;

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
            //ConsoleLog.WriteLine($"Cluster plate trim: Clusters {clusters.Count()} Simple {SimpleFillStrategyOLD.Count} NearDegenerate {NearDegenerateFillStrategyOLD.Count} Complex {ComplexFillStrategyOLD.Count} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
            Mode.ThreadedRun = true;
        }

        private static void ClusterAction(ClusterSet cluster, ClusterThread threadState, ClusterState state)
        {
            DateTime start = DateTime.Now;

            var disjointSets = cluster.Faces.Select(f => f.FacePlate).DisjointGroupsCombined().ToArray();
            IWireFrameMesh difference = null;
            try
            {
                cluster.TrimmedClusterGrid = WireFrameMesh.Create();
                GridIntermesh.ClusterId = cluster.Id;
                difference = cluster.Cluster.Create();

                //difference.ShowVitals();
                WavefrontFile.Export(difference, $"Wavefront/BeforeTrim/Cluster-{cluster.Id}");

                foreach (var set in disjointSets)
                {
                    difference = difference.Difference(set);
                }

                difference.ShowVitals();
                WavefrontFile.Export(difference, $"Wavefront/AfterTrim/Cluster-{cluster.Id}");

                cluster.TrimmedClusterGrid = difference;
                if (!cluster.TrimmedClusterGrid.Triangles.Any()) cluster.OriginalClusterGrid = cluster.Cluster.Create();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, ConsoleColor.Red);
            }
            var clusters = GroupingCollection.ExtractClusters(difference.Triangles);
            Console.WriteLine($"Cluster {cluster.Id} Disjoint sets {disjointSets.Length} Triangles {cluster.TrimmedClusterGrid.Triangles.Count} Clusters [{string.Join(",", clusters.Select(c => c.Triangles.Count()))}] Thread {threadState.ThreadId} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.",
                cluster.TrimmedClusterGrid.Triangles.Count > 0 ? ConsoleColor.Black : ConsoleColor.White,
                cluster.TrimmedClusterGrid.Triangles.Count > 0 ? (clusters.Count() == 1 ? ConsoleColor.Green : ConsoleColor.Yellow) : ConsoleColor.Red);
        }
        private class ClusterThread : BaseThreadState
        {
        }

        private class ClusterState : BaseState<ClusterThread>
        {

        }
    }
}
