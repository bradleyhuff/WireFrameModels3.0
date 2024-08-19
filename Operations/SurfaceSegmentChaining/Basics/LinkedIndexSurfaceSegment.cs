using BasicObjects.MathExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.SurfaceSegmentChaining.Basics
{
    internal class LinkedIndexSurfaceSegment<G, T>: LinkedIndexSegment<G> where G : class
    {
        public LinkedIndexSurfaceSegment(int groupKey, G groupObject, int indexPointA, int indexPointB, Rank rank) : base(indexPointA, indexPointB, rank, groupKey, groupObject) { }
        public List<LinkedIndexSurfaceSegment<G, T>> LinksA { get; set; } = new List<LinkedIndexSurfaceSegment<G, T>>();
        public List<LinkedIndexSurfaceSegment<G, T>> LinksB { get; set; } = new List<LinkedIndexSurfaceSegment<G, T>>();
        public List<LinkedIndexSurfaceSegment<G, T>> Traversals { get; set; } = new List<LinkedIndexSurfaceSegment<G, T>>();
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

        public int GetOppositeJunctionPoint()
        {
            if (LinksA.Count > 1) { return IndexPointB; }
            if (LinksB.Count > 1) { return IndexPointA; }
            throw new InvalidOperationException($"Junction point can't be found in this segment.");
        }

        public int Opposite(int index)
        {
            if (index == IndexPointA) { return IndexPointB; }
            if (index == IndexPointB) { return IndexPointA; }
            throw new InvalidOperationException($"Opposite of {index} can't be found in this segment.");
        }
    }

    internal class LinkedIndexSegment<G, T>: LinkedIndexSegment<G> where G : class
    {
        public LinkedIndexSegment(int groupKey, G groupObject, int indexPointA, int indexPointB, Rank rank) : base(indexPointA, indexPointB, rank, groupKey, groupObject) { }

        public List<LinkedIndexSegment<G, T>> LinksA { get; set; } = new List<LinkedIndexSegment<G, T>>();
        public List<LinkedIndexSegment<G, T>> LinksB { get; set; } = new List<LinkedIndexSegment<G, T>>();

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

        public List<LinkedIndexSegment<G, T>> GetLinksAtOppositeIndex(int index)
        {
            if (IndexPointA == index) { return LinksB; }
            if (IndexPointB == index) { return LinksA; }
            throw new InvalidOperationException($"Index could not be found in segment [{IndexPointA}, {IndexPointB}]");
        }
    }

    internal class LinkedIndexSegment<G> where G : class
    {
        public LinkedIndexSegment(int indexPointA, int indexPointB, Rank rank, int groupKey = 0, G groupObject = null)
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
