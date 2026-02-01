using BaseObjects;
using Collections.Threading;
using Operations.Intermesh;
using Operations.ParallelSurfaces.Basics;
using Operations.SetOperators;
using Console = BaseObjects.Console;
using Operations.Basics;
using Collections.WireFrameMesh.BasicWireFrameMesh;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles;
using Collections.WireFrameMesh.Interfaces;

namespace Operations.ParallelSurfaces
{
    public static class FacePlateTrimming
    {
        public static void PlateTrim(this IEnumerable<ClusterSet> clusters, Func<IWireFrameMesh, IWireFrameMesh> o)
        {
            DateTime start = DateTime.Now;
            Mode.ThreadedRun = true;
            ConsoleLog.Push("Cluster plate trim");

            var clusterState = new ClusterState();
            clusterState.Operation = o;
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
                var grid = cluster.Cluster.Create();
                grid = state.Operation(grid);
                int index = 0;
                //WavefrontFile.Export(disjointSets.First(), $"Wavefront/Sets-{index}");
                index++;
                var difference = grid.Difference(disjointSets.First());

                foreach (var set in disjointSets.Skip(1)/*.Take(4)*/)
                {
                    
                    difference = difference.Difference(set);
                    //WavefrontFile.Export(set, $"Wavefront/Sets-{index}");
                    index++;
                }

                //var disjointSet = disjointSets.Skip(4).First();
                //WavefrontFile.Export(disjointSet, $"Wavefront/Sets-{4}");

                //foreach (var set in disjointSets.Skip(1).Take(1))
                //{
                //    difference = difference.Sum(set);
                //    difference.NearCollinearTrianglePairs();
                //    //index++;
                //}

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
            public Func<IWireFrameMesh, IWireFrameMesh> Operation;
        }
    }
}
