﻿using BasicObjects.MathExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.SurfaceSegmentChaining.Basics
{
    internal class LinkedIndexSurfaceSegment<G, T> where G : class
    {
        public LinkedIndexSurfaceSegment(int groupKey, G groupObject, int indexPointA, int indexPointB, Rank rank)
        {
            GroupKey = groupKey;
            GroupObject = groupObject;
            IndexPointA = indexPointA;
            IndexPointB = indexPointB;
            Rank = rank;
        }
        public int GroupKey { get; }
        public G GroupObject { get; }
        public int IndexPointA { get; }
        public int IndexPointB { get; }
        public Rank Rank { get; }

        private Combination2 _key;
        private bool _keyIsAssigned = false;
        public Combination2 Key
        {
            get
            {
                if (!_keyIsAssigned)
                {
                    _key = new Combination2(IndexPointA, IndexPointB);
                    _keyIsAssigned = true;
                }
                return _key;
            }
        }

        public List<LinkedIndexSurfaceSegment<G, T>> LinksA { get; set; } = new List<LinkedIndexSurfaceSegment<G, T>>();
        public List<LinkedIndexSurfaceSegment<G, T>> LinksB { get; set; } = new List<LinkedIndexSurfaceSegment<G, T>>();
        public List<LinkedIndexSurfaceSegment<G, T>> Traversals { get; set; } = new List<LinkedIndexSurfaceSegment<G, T>>();
        public double? JunctionAngleA { get; set; }
        public double? JunctionAngleB { get; set; }
        private int _passes;
        public int Passes
        {
            get { return _passes; }
            set
            {
                if (value > 2) { throw new InvalidProgramException("Passes can't exceed 2."); }
                _passes = value;
            }
        }

        public List<LinkedIndexSurfaceSegment<G, T>> GetLinksAtIndex(int index)
        {
            if (IndexPointA == index) { return LinksA; }
            if (IndexPointB == index) { return LinksB; }
            throw new InvalidOperationException($"Index could not be found in segment [{IndexPointA}, {IndexPointB}]");
        }

        public double? GetJunctionAngleAtIndex(int index)
        {
            if (IndexPointA == index) { return JunctionAngleA; }
            if (IndexPointB == index) { return JunctionAngleB; }
            throw new InvalidOperationException($"Index could not be found in segment [{IndexPointA}, {IndexPointB}]");
        }

        public void AddTraversalPair(LinkedIndexSurfaceSegment<G, T> traversal)
        {
            if (traversal is null) { return; }
            if (!Traversals.Contains(traversal))
            {
                Traversals.Add(traversal);
            }

            if (!traversal.Traversals.Contains(this))
            {
                traversal.Traversals.Add(this);
            }
        }

        public bool WasTraversed(LinkedIndexSurfaceSegment<G, T> traversal)
        {
            return Traversals.Contains(traversal);
        }

        public int GetAJunctionPoint()
        {
            if (LinksA.Count > 1) { return IndexPointA; }
            if (LinksB.Count > 1) { return IndexPointB; }
            throw new InvalidOperationException($"Junction point can't be found in this segment.");
        }
    }

    internal class LinkedIndexSurfaceSegment<G> where G : class
    {
        public LinkedIndexSurfaceSegment(int indexPointA, int indexPointB, Rank rank, int groupKey = 0, G groupObject = null)
        {
            IndexPointA = indexPointA;
            IndexPointB = indexPointB;
            GroupObject = groupObject;
            Rank = rank;
            GroupKey = groupKey;
        }
        public int GroupKey { get; }
        public G GroupObject { get; }
        public int IndexPointA { get; }
        public int IndexPointB { get; }
        public Rank Rank { get; }

        private Combination2 _key;
        private bool _keyIsAssigned = false;
        public Combination2 Key
        {
            get
            {
                if (!_keyIsAssigned)
                {
                    _key = new Combination2(IndexPointA, IndexPointB);
                    _keyIsAssigned = true;
                }
                return _key;
            }
        }
    }
}
