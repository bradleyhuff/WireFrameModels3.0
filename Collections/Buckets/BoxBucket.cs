using BasicObjects.GeometricObjects;
using Collections.Buckets.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collections.Buckets
{
    public class BoxBucket
    {
        public const double MARGINS = 1e-6;
    }
    public class BoxBucket<T> : IBoxBucket<T> where T : IBox
    {
        private BoxBucketInternal<T>? _boxBuckets = null;
        private List<T>? _boxNodes = null;
        private IEnumerable<T>? _allNodes = null;
        public BoxBucket():this(Enumerable.Empty<T>()) { }
        public BoxBucket(IEnumerable<T> boxes)
        {
            _allNodes = boxes;
            if (boxes.Count() < BoxBucketInternal<T>.MAX_GROUP)
            {
                _boxNodes = boxes.ToList();
                return;
            }
            _boxBuckets = new BoxBucketInternal<T>(boxes);
        }

        public IEnumerable<T> AllNodes
        {
            get { return _allNodes; }
        }

        private IEnumerable<T> FetchInternal(Rectangle3D box)
        {
            if (_boxBuckets is null) { return _boxNodes; }
            return _boxBuckets.Fetch(box);
        }

        public T[] Fetch(T input)
        {
            return FetchInternal(input.Box).Where(n => Rectangle3D.Overlaps(input.Box, n.Box)).ToArray();
        }

        public T[] Fetch<G>(G input) where G : IBox
        {
            return FetchInternal(input.Box).Where(n => Rectangle3D.Overlaps(input.Box, n.Box)).ToArray();
        }

        public T[] Fetch(Rectangle3D box)
        {
            return FetchInternal(box).Where(n => Rectangle3D.Overlaps(box, n.Box)).ToArray();
        }

        public void Add(T box)
        {
            _boxNodes?.Add(box);
            SetBoxBuckets();
            if (_boxBuckets is null) { return; }
            if (!_boxBuckets.Box.Contains(box.Box))
            {
                var boxNodes = _boxBuckets.BoxNodes;
                boxNodes.Add(box);
                _boxBuckets = new BoxBucketInternal<T>(boxNodes);
                _boxNodes = null;
                return;
            }

            if (_boxNodes is null) { _boxBuckets.Add(box); }
            _boxNodes = null;
        }
        public void AddRange(IEnumerable<T> boxes)
        {
            _boxNodes?.AddRange(boxes);
            SetBoxBuckets();
            if (_boxBuckets is null) { return; }

            if (boxes.Any(b => !_boxBuckets.Box.Contains(b.Box)))
            {
                var boxNodes = _boxBuckets.BoxNodes;
                boxNodes.AddRange(boxes);
                _boxBuckets = new BoxBucketInternal<T>(boxNodes);
                return;
            }

            _boxBuckets.AddRange(boxes);
        }

        private void SetBoxBuckets()
        {
            if (_boxBuckets is null && _boxNodes.Count <= BoxBucketInternal<T>.MAX_GROUP) { return; }

            if (_boxBuckets is null)
            {
                _boxBuckets = new BoxBucketInternal<T>(_boxNodes);
            }
        }
    }
}
