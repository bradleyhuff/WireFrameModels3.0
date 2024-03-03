using Collections.Buckets.Interfaces;
using BasicObjects.GeometricObjects;
using System.Linq.Expressions;

namespace Collections.Buckets
{
    internal class CubeXYZgroupings8<T> where T : IBox
    {
        internal CubeXYZgroupings8(IBoxBucketInternal<T> bucket)
        {
            Bucket = bucket;
        }

        internal IBoxBucketInternal<T> Bucket { get; private set; }

        internal List<T> Fetch(Rectangle3D box)
        {
            if (Bucket is null || !Bucket.Box.Contains(box)) { return null; }

            int caseNumber = GetCase(box);

            switch (caseNumber)
            {
                case 1: return ReturnX4(box);
                case 2: return ReturnY4(box);
                case 3: return ReturnXY2(box);
                case 4: return ReturnZ4(box);
                case 5: return ReturnXZ2(box);
                case 6: return ReturnYZ2(box);
                case 7: return ReturnXYZ1(box);
            }

            return null;
        }

        private int GetCase(Rectangle3D box)
        {
            var boxMaxPoint = box.MaxPoint;
            var boxMinPoint = box.MinPoint;
            var bucketCenterBoxMinPoint = Bucket.CenterBox.MinPoint;
            var bucketCenterBoxMaxPoint = Bucket.CenterBox.MaxPoint;

            bool centerX = bucketCenterBoxMinPoint.X < boxMinPoint.X && boxMaxPoint.X < bucketCenterBoxMaxPoint.X;
            bool centerY = bucketCenterBoxMinPoint.Y < boxMinPoint.Y && boxMaxPoint.Y < bucketCenterBoxMaxPoint.Y;
            bool centerZ = bucketCenterBoxMinPoint.Z < boxMinPoint.Z && boxMaxPoint.Z < bucketCenterBoxMaxPoint.Z;

            int digitX = centerX ? 1 : 0;
            int digitY = centerY ? 1 : 0;
            int digitZ = centerZ ? 1 : 0;

            return digitZ << 2 | digitY << 1 | digitX;
        }

        private IBoxBucketInternal<T>[] _partsX4 = null;
        private IBoxBucketInternal<T>[] _partsY4 = null;
        private IBoxBucketInternal<T>[] _partsZ4 = null;
        private IBoxBucketInternal<T>[] _partsXY2 = null;
        private IBoxBucketInternal<T>[] _partsXZ2 = null;
        private IBoxBucketInternal<T>[] _partsYZ2 = null;
        private IBoxBucketInternal<T> _partsXYZ1 = null;

        internal IEnumerable<IBoxBucketInternal<T>> GetSubBoxBuckets()
        {
            if (_partsX4 is not null) { foreach (var element in _partsX4){ yield return element; } }
            if (_partsY4 is not null) { foreach (var element in _partsY4) { yield return element; } }
            if (_partsZ4 is not null) { foreach (var element in _partsZ4) { yield return element; } }

            if (_partsXY2 is not null) { foreach (var element in _partsXY2) { yield return element; } }
            if (_partsXZ2 is not null) { foreach (var element in _partsXZ2) { yield return element; } }
            if (_partsYZ2 is not null) { foreach (var element in _partsYZ2) { yield return element; } }

            if(_partsXYZ1 is not null) { yield return _partsXYZ1; }
        }

        private List<T> ReturnX4(Rectangle3D box)
        {
            bool ok = Shared.GetYZindicies4(Bucket.Box, box, out int index4);
            if (!ok) { return null; }
            BuildPartsX4();
            return _partsX4[index4].Fetch(box);
        }

        private List<T> ReturnY4(Rectangle3D box)
        {
            bool ok = Shared.GetXZindicies4(Bucket.Box, box, out int index4);
            if (!ok) { return null; }
            BuildPartsY4();
            return _partsY4[index4].Fetch(box);
        }

        private List<T> ReturnZ4(Rectangle3D box)
        {
            bool ok = Shared.GetXYindicies4(Bucket.Box, box, out int index4);
            if (!ok) { return null; }
            BuildPartsZ4();
            return _partsZ4[index4].Fetch(box);
        }

        private List<T> ReturnXY2(Rectangle3D box)
        {
            bool ok = Shared.GetZindicies2(Bucket.Box, box, out int index2);
            if (!ok) { return null; }
            BuildPartsXY2();
            return _partsXY2[index2].Fetch(box);
        }

        private List<T> ReturnXZ2(Rectangle3D box)
        {
            bool ok = Shared.GetYindicies2(Bucket.Box, box, out int index2);
            if (!ok) { return null; }
            BuildPartsXZ2();
            return _partsXZ2[index2].Fetch(box);
        }

        private List<T> ReturnYZ2(Rectangle3D box)
        {
            bool ok = Shared.GetXindicies2(Bucket.Box, box, out int index2);
            if (!ok) { return null; }
            BuildPartsYZ2();
            return _partsYZ2[index2].Fetch(box);
        }

        private List<T> ReturnXYZ1(Rectangle3D box)
        {
            BuildPartsXYZ1();
            return _partsXYZ1.Fetch(box);
        }

        private void BuildPartsX4()
        {
            if (_partsX4 is not null) { return; }
            _partsX4 = Shared.CenterYZsplit4(Bucket);
        }

        private void BuildPartsY4()
        {
            if (_partsY4 is not null) { return; }
            _partsY4 = Shared.CenterXZsplit4(Bucket);
        }

        private void BuildPartsZ4()
        {
            if (_partsZ4 is not null) { return; }
            _partsZ4 = Shared.CenterXYsplit4(Bucket);

        }

        private void BuildPartsXY2()
        {
            if (_partsXY2 is not null) { return; }
            _partsXY2 = Shared.CenterZsplit2(Bucket);
        }

        private void BuildPartsXZ2()
        {
            if (_partsXZ2 is not null) { return; }
            _partsXZ2 = Shared.CenterYsplit2(Bucket);
        }

        private void BuildPartsYZ2()
        {
            if (_partsYZ2 is not null) { return; }
            _partsYZ2 = Shared.CenterXsplit2(Bucket);
        }

        private void BuildPartsXYZ1()
        {
            if (_partsXYZ1 is not null) { return; }
            _partsXYZ1 = Shared.CenterFilter(Bucket);
        }
    }
}
