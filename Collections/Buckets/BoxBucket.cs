using Collections.Buckets.Interfaces;
using BasicObjects.GeometricObjects;

namespace Collections.Buckets
{
    public class BoxBucket<T> : IBoxBucket<T> where T : IBox
    {
        public const int MAX_GROUP = 16;
        public const int MAX_LEVEL = 24;

        public BoxBucket(T[] bucketNodes) : this(bucketNodes, GetContainingBox(bucketNodes), 0) { }

        private static Rectangle3D GetContainingBox(T[] bucketNodes)
        {
            var minX = Double.MaxValue;
            var maxX = Double.MinValue;
            var minY = Double.MaxValue;
            var maxY = Double.MinValue;
            var minZ = Double.MaxValue;
            var maxZ = Double.MinValue;

            foreach (var node in bucketNodes)
            {
                var minPoint = node.Box.MinPoint;
                var maxPoint = node.Box.MaxPoint;
                minX = Math.Min(minX, minPoint.X);
                maxX = Math.Max(maxX, maxPoint.X);
                minY = Math.Min(minY, minPoint.Y);
                maxY = Math.Max(maxY, maxPoint.Y);
                minZ = Math.Min(minZ, minPoint.Z);
                maxZ = Math.Max(maxZ, maxPoint.Z);
            }
            return new Rectangle3D(minX, maxX, minY, maxY, minZ, maxZ);
        }

        protected BoxBucket(T[] boxNodes, Rectangle3D box, int level)
        {
            BoxNodes = boxNodes;
            Box = box;
            Level = level;
            if (box is null) { return; }
            CenterBox = new Rectangle3D(
                Box.CenterPoint + new Vector3D(-Box.LengthX * 0.25, -Box.LengthY * 0.25, -Box.LengthZ * 0.25),
                Box.CenterPoint + new Vector3D(Box.LengthX * 0.25, Box.LengthY * 0.25, Box.LengthZ * 0.25));
        }

        public virtual IBoxBucket<T> CreateInstance(T[] boxNodes, Rectangle3D box, int level)
        {
            return new BoxBucket<T>(boxNodes, box, level);
        }
        private object locker = new object();

        public T[] Fetch(T input)
        {
            return RawFetch(input.Box).Where(n => Rectangle3D.Overlaps(input.Box, n.Box)).ToArray();
        }

        public T[] Fetch<G>(G input) where G : IBox
        {
            return RawFetch(input.Box).Where(n => Rectangle3D.Overlaps(input.Box, n.Box)).ToArray();
        }

        public T[] Fetch(Rectangle3D box)
        {
            return RawFetch(box).Where(n => Rectangle3D.Overlaps(box, n.Box)).ToArray();
        }

        public T[] RawFetch(Rectangle3D box)
        {
            if (BoxNodes.Length < MAX_GROUP || Level >= MAX_LEVEL)
            {
                return BoxNodes;
            }

            int index8;
            bool ok = Shared.GetXYZindicies8(Box, box, out index8);

            if (ok)
            {
                BuildParts8();
                return _parts8[index8].RawFetch(box);
            }

            var groupings = ReturnGroupings(box);
            if (groupings != null) { return groupings; }

            return BoxNodes;
        }

        public void Profile(string label = "")
        {
            if (Level > 3 && BoxNodes.Length < 3000) { return; }
            var containment = Rectangle3D.Containing(BoxNodes.Select(b => b.Box)?.ToArray());
            if (containment == null) { containment = new Rectangle3D(0, 0, 0, 0, 0, 0); }
            Console.WriteLine($"{new string(' ', Level)}{label}Level {Level}: {BoxNodes.Length} [{Box.LengthX.ToString("0.000000")}, {Box.LengthY.ToString("0.000000")}, {Box.LengthZ.ToString("0.000000")}] Containment: [{containment.LengthX.ToString("0.000000")}, {containment.LengthY.ToString("0.000000")}, {containment.LengthZ.ToString("0.000000")}]");
            if (_parts8 != null)
            {
                foreach (var part in _parts8)
                {
                    part?.Profile("Parts8 ");
                }
            }
            _groupings?.Profile();
        }

        private T[]? ReturnGroupings(Rectangle3D box)
        {
            BuildGroupings();
            return _groupings?.BasicFetch(box);
        }
        public T[] BoxNodes { get; } = new T[0];

        private T[] _containedBoxNodes = null;
        public T[] ContainedBoxNodes
        {
            get
            {
                if (_containedBoxNodes is null)
                {
                    _containedBoxNodes = BoxNodes.Where(b => Box.Contains(b.Box)).ToArray();
                }
                return _containedBoxNodes;
            }
        }
        public Rectangle3D Box { get; private set; }
        public Rectangle3D CenterBox { get; private set; }
        public int Level { get; private set; }

        private IBoxBucket<T>[]? _parts8 = null;

        private CubeXYZgroupings<T>? _groupings = null;//level of parts8

        private void BuildParts8()
        {
            if (_parts8 is not null) { return; }
            lock (locker)
            {
                _parts8 = Shared.xyzSplit8(this);
            }
        }
        private void BuildGroupings()
        {
            if (_groupings is not null) { return; }
            lock (locker)
            {
                _groupings = new CubeXYZgroupings<T>(this);
            }
        }
    }
}
