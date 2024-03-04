using Collections.Buckets.Interfaces;
using BasicObjects.GeometricObjects;

namespace Collections.Buckets
{
    internal class BoxBucketInternal<T> : IBoxBucketInternal<T> where T : IBox
    {
        public const int MAX_GROUP = 16;
        public const double MIN_LENGTH = 1e-6;

        public BoxBucketInternal(IEnumerable<T> boxNodes)
        {
            BoxNodes = boxNodes.ToList();
            Box = Rectangle3D.CubeContaining(Rectangle3D.Containing(boxNodes.Select(b => b.Box).ToArray()), 2);
            CenterBox = new Rectangle3D(Box.CenterPoint, Box.LengthX * 0.25, Box.LengthY * 0.25, Box.LengthZ * 0.25);

        }

        protected BoxBucketInternal(Rectangle3D box, IEnumerable<T> boxNodes)
        {
            Box = box;
            BoxNodes = boxNodes.Where(b => Rectangle3D.Overlaps(Box, b.Box)).ToList();
            CenterBox = new Rectangle3D(Box.CenterPoint, Box.LengthX * 0.25, Box.LengthY * 0.25, Box.LengthZ * 0.25);
        }

        public virtual IBoxBucketInternal<T> CreateInstance(Rectangle3D box, IEnumerable<T> boxNodes)
        {
            return new BoxBucketInternal<T>(box, boxNodes);
        }

        private object locker = new object();

        public List<T> Fetch(Rectangle3D box)
        {
            if (addedBoxNodes.Any())
            {
                lock (locker)
                {
                    foreach (var bucket in GetSubBoxBuckets())
                    {
                        bucket.AddRange(addedBoxNodes);
                    }

                    BoxNodes.AddRange(addedBoxNodes);
                    addedBoxNodes.Clear();
                }
            }

            if (BoxNodes.Count < MAX_GROUP || Box.LengthX < MIN_LENGTH)
            {
                return BoxNodes;
            }

            bool ok = Shared.GetXYZindicies8(Box, box, out int index8);

            if (ok)
            {
                BuildParts8();
                return _parts8[index8].Fetch(box);
            }

            var XYZgroupings8 = ReturnXYZgroupings8(box);
            if (XYZgroupings8 != null) { return XYZgroupings8; }

            return BoxNodes;
        }

        private List<T> addedBoxNodes;

        public void Add(T box)
        {
            if (addedBoxNodes is null) addedBoxNodes = new List<T>();
            if (Rectangle3D.Overlaps(Box, box.Box))
            {
                addedBoxNodes.Add(box);
            }
        }
        public void AddRange(IEnumerable<T> boxes)
        {
            if (addedBoxNodes is null) addedBoxNodes = new List<T>();
            addedBoxNodes.AddRange(boxes.Where(b => Rectangle3D.Overlaps(Box, b.Box)).ToList());
        }

        private List<T>? ReturnXYZgroupings8(Rectangle3D box)
        {
            BuildXYZgroupings8();
            return _groupings8?.Fetch(box);
        }
        public List<T> BoxNodes { get; }
        public Rectangle3D Box { get; private set; }
        public Rectangle3D CenterBox { get; private set; }

        private IBoxBucketInternal<T>[]? _parts8 = null;

        private CubeXYZgroupings8<T>? _groupings8 = null;//level of parts8

        private IEnumerable<IBoxBucketInternal<T>> GetSubBoxBuckets()
        {
            if (_parts8 is not null)
            {
                foreach (var element in _parts8) { yield return element; }
            }
            if (_groupings8 is not null)
            {
                foreach (var element in _groupings8.GetSubBoxBuckets()) { yield return element; }
            }
        }

        private void BuildParts8()
        {
            if (_parts8 is not null) { return; }
            lock (locker)
            {
                _parts8 = Shared.XYZsplit8(this);
            }
        }
        private void BuildXYZgroupings8()
        {
            if (_groupings8 is not null) { return; }
            lock (locker)
            {
                _groupings8 = new CubeXYZgroupings8<T>(this);
            }
        }
    }
}
