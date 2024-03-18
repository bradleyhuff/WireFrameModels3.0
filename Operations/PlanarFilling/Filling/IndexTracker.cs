
namespace Operations.PlanarFilling.Filling
{
    internal class IndexTracker
    {
        private List<int> _track;
        private int _pointer = 0;


        public IndexTracker(IEnumerable<int> track)
        {
            _track = track.ToList();
        }

        private int Cycle(int index)
        {
            int m = 0;
            if (index < 0)
            {
                m = (int)Math.Ceiling(-index / (double)_track.Count);
            }
            return (index + m * _track.Count) % _track.Count;
        }

        public bool StartAtIndex(int index)
        {
            int foundIndex = _track.FindIndex(i => i == index);
            if (foundIndex > -1)
            {
                _pointer = foundIndex;
            }
            return foundIndex > -1;
        }

        public void InsertAt(int index, IEnumerable<int> insertionIndicies)
        {
            _track.InsertRange(index, insertionIndicies);
        }

        public void AdvanceStep(int step)
        {
            _pointer = Cycle(_pointer + step);
        }

        public int LookAheadStep(int step)
        {
            return _track[Cycle(_pointer + step)];
        }

        public void RemoveIndex(int index)
        {
            _track.Remove(index);
        }

        public void RemoveIndicies(IEnumerable<int> toRemove)
        {
            foreach (var element in toRemove)
            {
                _track.Remove(element);
            }
        }

        public List<int> Tracking
        {
            get { return _track; }
        }

        public int Count
        {
            get
            {
                return _track.Count;
            }
        }
    }
}
