﻿using BaseObjects;
using Collections.Threading;
using Collections.WireFrameMesh.Interfaces;
using Operations.Groupings.Types;
using Operations.Intermesh;
using Operations.ParallelSurfaces.Basics;
using Operations.SetOperators;
using Console = BaseObjects.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Operations.Basics;

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
            clusterIterator.Run<ClusterState, ClusterThread>(ClusterAction, clusterState, 1, 4);

            ConsoleLog.Pop();
            ConsoleLog.WriteLine($"Cluster plate trim: Clusters {clusters.Count()} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.");
            Mode.ThreadedRun = true;
        }

        private static void ClusterAction(ClusterSet cluster, ClusterThread threadState, ClusterState state)
        {
            DateTime start = DateTime.Now;

            var disjointSets = cluster.Faces.Select(f => f.FacePlate).DisjointGroupsCombined().ToArray();

            try
            {
                var difference = cluster.Cluster.Create().Difference(disjointSets.First());
                //Console.WriteLine($"Difference for {cluster.Id} {difference.Triangles.Count}", difference.Triangles.Any() ? ConsoleColor.Green : ConsoleColor.Red);

                //foreach (var set in disjointSets.Skip(1))
                //{
                //    difference = difference.Difference(set);
                //    //Console.WriteLine($"Difference for {cluster.Id} {difference.Triangles.Count}", difference.Triangles.Any() ? ConsoleColor.Green : ConsoleColor.Red);
                //}
                cluster.ClusterGrid = difference;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, ConsoleColor.Red);
            }

            Console.WriteLine($"Cluster {cluster.Id} Disjoint sets {disjointSets.Length} Thread {threadState.ThreadId} Elapsed time {(DateTime.Now - start).TotalSeconds} seconds.", ConsoleColor.Cyan);
        }
        private class ClusterThread : BaseThreadState
        {
        }

        private class ClusterState : BaseState<ClusterThread>
        {
        }
    }
}
