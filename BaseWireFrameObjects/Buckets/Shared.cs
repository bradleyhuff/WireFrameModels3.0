using BaseWireFrameObjects.Buckets.Interfaces;
using BasicObjects.GeometricObjects;

namespace BaseWireFrameObjects.Buckets
{
    internal static class Shared
    {
        internal static bool GetXYZindicies8(Rectangle3D containingBox, Rectangle3D box, out int index)
        {
            index = 0;

            var boxMaxPoint = box.MaxPoint;
            var boxMinPoint = box.MinPoint;
            var containingBoxCenterPoint = containingBox.CenterPoint;

            int digit0 = boxMaxPoint.X < containingBoxCenterPoint.X ? 1 : 0;
            int digit1 = containingBoxCenterPoint.X < boxMinPoint.X ? 1 : 0;
            int digit2 = boxMaxPoint.Y < containingBoxCenterPoint.Y ? 1 : 0;
            int digit3 = containingBoxCenterPoint.Y < boxMinPoint.Y ? 1 : 0;
            int digit4 = boxMaxPoint.Z < containingBoxCenterPoint.Z ? 1 : 0;
            int digit5 = containingBoxCenterPoint.Z < boxMinPoint.Z ? 1 : 0;

            int caseNumber = digit5 << 5 | digit4 << 4 | digit3 << 3 | digit2 << 2 | digit1 << 1 | digit0;

            switch (caseNumber)
            {
                case 21: return true;
                case 22: index = 1; return true;
                case 25: index = 2; return true;
                case 26: index = 3; return true;
                case 37: index = 4; return true;
                case 38: index = 5; return true;
                case 41: index = 6; return true;
                case 42: index = 7; return true;
            }
            return false;
        }

        internal static bool GetXYindicies4(Rectangle3D containingBox, Rectangle3D box, out int index)
        {
            index = 0;

            var boxMaxPoint = box.MaxPoint;
            var boxMinPoint = box.MinPoint;
            var containingBoxCenterPoint = containingBox.CenterPoint;

            int digit0 = boxMaxPoint.X < containingBoxCenterPoint.X ? 1 : 0;
            int digit1 = containingBoxCenterPoint.X < boxMinPoint.X ? 1 : 0;
            int digit2 = boxMaxPoint.Y < containingBoxCenterPoint.Y ? 1 : 0;
            int digit3 = containingBoxCenterPoint.Y < boxMinPoint.Y ? 1 : 0;

            int caseNumber = digit3 << 3 | digit2 << 2 | digit1 << 1 | digit0;

            switch (caseNumber)
            {
                case 5: return true;
                case 6: index = 1; return true;
                case 9: index = 2; return true;
                case 10: index = 3; return true;
            }
            return false;
        }

        internal static bool GetXZindicies4(Rectangle3D containingBox, Rectangle3D box, out int index)
        {
            index = 0;

            var boxMaxPoint = box.MaxPoint;
            var boxMinPoint = box.MinPoint;
            var containingBoxCenterPoint = containingBox.CenterPoint;

            int digit0 = boxMaxPoint.X < containingBoxCenterPoint.X ? 1 : 0;
            int digit1 = containingBoxCenterPoint.X < boxMinPoint.X ? 1 : 0;
            int digit2 = boxMaxPoint.Z < containingBoxCenterPoint.Z ? 1 : 0;
            int digit3 = containingBoxCenterPoint.Z < boxMinPoint.Z ? 1 : 0;

            int caseNumber = digit3 << 3 | digit2 << 2 | digit1 << 1 | digit0;

            switch (caseNumber)
            {
                case 5: return true;
                case 6: index = 1; return true;
                case 9: index = 2; return true;
                case 10: index = 3; return true;
            }
            return false;
        }

        internal static bool GetYZindicies4(Rectangle3D containingBox, Rectangle3D box, out int index)
        {
            index = 0;

            var boxMaxPoint = box.MaxPoint;
            var boxMinPoint = box.MinPoint;
            var containingBoxCenterPoint = containingBox.CenterPoint;

            int digit0 = boxMaxPoint.Y < containingBoxCenterPoint.Y ? 1 : 0;
            int digit1 = containingBoxCenterPoint.Y < boxMinPoint.Y ? 1 : 0;
            int digit2 = boxMaxPoint.Z < containingBoxCenterPoint.Z ? 1 : 0;
            int digit3 = containingBoxCenterPoint.Z < boxMinPoint.Z ? 1 : 0;

            int caseNumber = digit3 << 3 | digit2 << 2 | digit1 << 1 | digit0;

            switch (caseNumber)
            {
                case 5: return true;
                case 6: index = 1; return true;
                case 9: index = 2; return true;
                case 10: index = 3; return true;
            }
            return false;
        }

        internal static bool GetXindicies2(Rectangle3D containingBox, Rectangle3D box, out int index)
        {
            index = 0;

            var boxMaxPoint = box.MaxPoint;
            var boxMinPoint = box.MinPoint;
            var containingBoxCenterPoint = containingBox.CenterPoint;

            int digit0 = boxMaxPoint.X < containingBoxCenterPoint.X ? 1 : 0;
            int digit1 = containingBoxCenterPoint.X < boxMinPoint.X ? 1 : 0;

            int caseNumber = digit1 << 1 | digit0;

            switch (caseNumber)
            {
                case 1: return true;
                case 2: index = 1; return true;
            }
            return false;
        }

        internal static bool GetYindicies2(Rectangle3D containingBox, Rectangle3D box, out int index)
        {
            index = 0;

            var boxMaxPoint = box.MaxPoint;
            var boxMinPoint = box.MinPoint;
            var containingBoxCenterPoint = containingBox.CenterPoint;

            int digit0 = boxMaxPoint.Y < containingBoxCenterPoint.Y ? 1 : 0;
            int digit1 = containingBoxCenterPoint.Y < boxMinPoint.Y ? 1 : 0;

            int caseNumber = digit1 << 1 | digit0;

            switch (caseNumber)
            {
                case 1: return true;
                case 2: index = 1; return true;
            }
            return false;
        }

        internal static bool GetZindicies2(Rectangle3D containingBox, Rectangle3D box, out int index)
        {
            index = 0;

            var boxMaxPoint = box.MaxPoint;
            var boxMinPoint = box.MinPoint;
            var containingBoxCenterPoint = containingBox.CenterPoint;

            int digit0 = boxMaxPoint.Z < containingBoxCenterPoint.Z ? 1 : 0;
            int digit1 = containingBoxCenterPoint.Z < boxMinPoint.Z ? 1 : 0;

            int caseNumber = digit1 << 1 | digit0;

            switch (caseNumber)
            {
                case 1: return true;
                case 2: index = 1; return true;
            }
            return false;
        }

        internal static IBoxBucket<T>[] xyzSplit8<T>(IBoxBucket<T> bucket) where T : IBox
        {
            var output = new IBoxBucket<T>[8];

            var splits = new List<T>[8];
            splits[0] = new List<T>();
            splits[1] = new List<T>();
            splits[2] = new List<T>();
            splits[3] = new List<T>();
            splits[4] = new List<T>();
            splits[5] = new List<T>();
            splits[6] = new List<T>();
            splits[7] = new List<T>();

            var bucketBoxMinPoint = bucket.Box.MinPoint;
            var bucketBoxMaxPoint = bucket.Box.MaxPoint;
            var bucketBoxCenterPoint = bucket.Box.CenterPoint;

            foreach (var node in bucket.BoxNodes)
            {
                var box = node.Box;
                var boxMinPoint = box.MinPoint;
                var boxMaxPoint = box.MaxPoint;

                var boxMinMaxMinCenterX = bucketBoxMinPoint.X < boxMaxPoint.X && boxMinPoint.X < bucketBoxCenterPoint.X;
                var boxMinMaxMinCenterY = bucketBoxMinPoint.Y < boxMaxPoint.Y && boxMinPoint.Y < bucketBoxCenterPoint.Y;
                var boxMinMaxMinCenterZ = bucketBoxMinPoint.Z < boxMaxPoint.Z && boxMinPoint.Z < bucketBoxCenterPoint.Z;

                var boxCenterMaxMinMaxX = bucketBoxCenterPoint.X < boxMaxPoint.X && boxMinPoint.X < bucketBoxMaxPoint.X;
                var boxCenterMaxMinMaxY = bucketBoxCenterPoint.Y < boxMaxPoint.Y && boxMinPoint.Y < bucketBoxMaxPoint.Y;
                var boxCenterMaxMinMaxZ = bucketBoxCenterPoint.Z < boxMaxPoint.Z && boxMinPoint.Z < bucketBoxMaxPoint.Z;


                if (boxMinMaxMinCenterX && boxMinMaxMinCenterY && boxMinMaxMinCenterZ)
                {
                    splits[0].Add(node);
                }

                if (boxCenterMaxMinMaxX && boxMinMaxMinCenterY && boxMinMaxMinCenterZ)
                {
                    splits[1].Add(node);
                }

                if (boxMinMaxMinCenterX && boxCenterMaxMinMaxY && boxMinMaxMinCenterZ)
                {
                    splits[2].Add(node);
                }

                if (boxCenterMaxMinMaxX && boxCenterMaxMinMaxY && boxMinMaxMinCenterZ)
                {
                    splits[3].Add(node);
                }

                if (boxMinMaxMinCenterX && boxMinMaxMinCenterY && boxCenterMaxMinMaxZ)
                {
                    splits[4].Add(node);
                }

                if (boxCenterMaxMinMaxX && boxMinMaxMinCenterY && boxCenterMaxMinMaxZ)
                {
                    splits[5].Add(node);
                }

                if (boxMinMaxMinCenterX && boxCenterMaxMinMaxY && boxCenterMaxMinMaxZ)
                {
                    splits[6].Add(node);
                }

                if (boxCenterMaxMinMaxX && boxCenterMaxMinMaxY && boxCenterMaxMinMaxZ)
                {
                    splits[7].Add(node);
                }
            }

            output[0] = bucket.CreateInstance(
                splits[0].ToArray(),
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                    bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                    bucket.Level + 1
                );

            output[1] = bucket.CreateInstance(
                splits[1].ToArray(),
                new Rectangle3D(
                bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                bucket.Level + 1
            );

            output[2] = bucket.CreateInstance(
                splits[2].ToArray(),
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                    bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                    bucket.Level + 1
            );

            output[3] = bucket.CreateInstance(
                 splits[3].ToArray(),
                 new Rectangle3D(
                     bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                     bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                     bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                     bucket.Level + 1
             );

            output[4] = bucket.CreateInstance(
                splits[4].ToArray(),
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z),
                    bucket.Level + 1
                );

            output[5] = bucket.CreateInstance(
                splits[5].ToArray(),
                new Rectangle3D(
                    bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                    bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z),
                    bucket.Level + 1
            );

            output[6] = bucket.CreateInstance(
                splits[6].ToArray(),
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z),
                    bucket.Level + 1
                );

            output[7] = bucket.CreateInstance(
                splits[7].ToArray(),
                new Rectangle3D(
                    bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                    bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z),
                    bucket.Level + 1
            );

            return output;
        }

        internal static IBoxBucket<T>[] CenterYZsplit4<T>(IBoxBucket<T> bucket) where T : IBox
        {
            var output = new IBoxBucket<T>[4];

            var splits = new List<T>[4];
            splits[0] = new List<T>();
            splits[1] = new List<T>();
            splits[2] = new List<T>();
            splits[3] = new List<T>();

            var bucketBoxMinPoint = bucket.Box.MinPoint;
            var bucketBoxMaxPoint = bucket.Box.MaxPoint;
            var bucketBoxCenterPoint = bucket.Box.CenterPoint;
            var bucketCenterBoxMinPoint = bucket.CenterBox.MinPoint;
            var bucketCenterBoxMaxPoint = bucket.CenterBox.MaxPoint;

            foreach (var node in bucket.BoxNodes)
            {
                var box = node.Box;
                var boxMinPoint = box.MinPoint;
                var boxMaxPoint = box.MaxPoint;

                if (bucketCenterBoxMinPoint.X > boxMaxPoint.X || boxMinPoint.X > bucketCenterBoxMaxPoint.X) { continue; }

                if (bucketBoxMinPoint.Y < boxMaxPoint.Y && boxMinPoint.Y < bucketBoxCenterPoint.Y &&
                    bucketBoxMinPoint.Z < boxMaxPoint.Z && boxMinPoint.Z < bucketBoxCenterPoint.Z)
                {
                    splits[0].Add(node);
                }

                if (bucketBoxCenterPoint.Y < boxMaxPoint.Y && boxMinPoint.Y < bucketBoxMaxPoint.Y &&
                    bucketBoxMinPoint.Z < boxMaxPoint.Z && boxMinPoint.Z < bucketBoxCenterPoint.Z)
                {
                    splits[1].Add(node);
                }

                if (bucketBoxMinPoint.Y < boxMaxPoint.Y && boxMinPoint.Y < bucketBoxCenterPoint.Y &&
                    bucketBoxCenterPoint.Z < boxMaxPoint.Z && boxMinPoint.Z < bucketBoxMaxPoint.Z)
                {
                    splits[2].Add(node);
                }

                if (bucketBoxCenterPoint.Y < boxMaxPoint.Y && boxMinPoint.Y < bucketBoxMaxPoint.Y &&
                    bucketBoxCenterPoint.Z < boxMaxPoint.Z && boxMinPoint.Z < bucketBoxMaxPoint.Z)
                {
                    splits[3].Add(node);
                }
            }

            output[0] = bucket.CreateInstance(
                splits[0].ToArray(),
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                    bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                    bucket.Level + 1
            );

            output[1] = bucket.CreateInstance(
                splits[1].ToArray(),
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                    bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                    bucket.Level + 1
                );

            output[2] = bucket.CreateInstance(
                splits[2].ToArray(),
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z),
                    bucket.Level + 1
                );

            output[3] = bucket.CreateInstance(
                splits[3].ToArray(),
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z),
                    bucket.Level + 1
                );

            return output;
        }

        internal static IBoxBucket<T>[] CenterXZsplit4<T>(IBoxBucket<T> bucket) where T : IBox
        {
            var output = new IBoxBucket<T>[4];

            var splits = new List<T>[4];
            splits[0] = new List<T>();
            splits[1] = new List<T>();
            splits[2] = new List<T>();
            splits[3] = new List<T>();

            var bucketBoxMinPoint = bucket.Box.MinPoint;
            var bucketBoxMaxPoint = bucket.Box.MaxPoint;
            var bucketCenterBoxMinPoint = bucket.CenterBox.MinPoint;
            var bucketCenterBoxMaxPoint = bucket.CenterBox.MaxPoint;
            var bucketBoxCenterPoint = bucket.Box.CenterPoint;

            foreach (var node in bucket.BoxNodes)
            {
                var box = node.Box;
                var boxMinPoint = box.MinPoint;
                var boxMaxPoint = box.MaxPoint;

                if (bucketCenterBoxMinPoint.Y > boxMaxPoint.Y || boxMinPoint.Y > bucketCenterBoxMaxPoint.Y) { continue; }

                if (bucketBoxMinPoint.X < boxMaxPoint.X && boxMinPoint.X < bucketBoxCenterPoint.X &&
                    bucketBoxMinPoint.Z < boxMaxPoint.Z && boxMinPoint.Z < bucketBoxCenterPoint.Z)
                {
                    splits[0].Add(node);
                }

                if (bucketBoxCenterPoint.X < boxMaxPoint.X && boxMinPoint.X < bucketBoxMaxPoint.X &&
                    bucketBoxMinPoint.Z < boxMaxPoint.Z && boxMinPoint.Z < bucketBoxCenterPoint.Z)
                {
                    splits[1].Add(node);
                }

                if (bucketBoxMinPoint.X < boxMaxPoint.X && boxMinPoint.X < bucketBoxCenterPoint.X &&
                    bucketBoxCenterPoint.Z < boxMaxPoint.Z && boxMinPoint.Z < bucketBoxMaxPoint.Z)
                {
                    splits[2].Add(node);
                }

                if (bucketBoxCenterPoint.X < boxMaxPoint.X && boxMinPoint.X < bucketBoxMaxPoint.X &&
                    bucketBoxCenterPoint.Z < boxMaxPoint.Z && boxMinPoint.Z < bucketBoxMaxPoint.Z)
                {
                    splits[3].Add(node);
                }
            }

            output[0] = bucket.CreateInstance(
                splits[0].ToArray(),
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                    bucket.Level + 1
                );

            output[1] = bucket.CreateInstance(
                splits[1].ToArray(),
                new Rectangle3D(
                    bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                    bucket.Level + 1
                );

            output[2] = bucket.CreateInstance(
                splits[2].ToArray(),
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z),
                    bucket.Level + 1
                );

            output[3] = bucket.CreateInstance(
                splits[3].ToArray(),
                new Rectangle3D(
                    bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z),
                    bucket.Level + 1
                );

            return output;
        }

        internal static IBoxBucket<T>[] CenterXYsplit4<T>(IBoxBucket<T> bucket) where T : IBox
        {
            var output = new IBoxBucket<T>[4];

            var splits = new List<T>[4];
            splits[0] = new List<T>();
            splits[1] = new List<T>();
            splits[2] = new List<T>();
            splits[3] = new List<T>();

            var bucketBoxMinPoint = bucket.Box.MinPoint;
            var bucketBoxMaxPoint = bucket.Box.MaxPoint;
            var bucketCenterBoxMinPoint = bucket.CenterBox.MinPoint;
            var bucketCenterBoxMaxPoint = bucket.CenterBox.MaxPoint;
            var bucketBoxCenterPoint = bucket.Box.CenterPoint;

            foreach (var node in bucket.BoxNodes)
            {
                var box = node.Box;
                var boxMinPoint = box.MinPoint;
                var boxMaxPoint = box.MaxPoint;

                if (bucketCenterBoxMinPoint.Z > boxMaxPoint.Z || boxMinPoint.Z > bucketCenterBoxMaxPoint.Z) { continue; }

                if (bucketBoxMinPoint.X < boxMaxPoint.X && boxMinPoint.X < bucketBoxCenterPoint.X &&
                    bucketBoxMinPoint.Y < boxMaxPoint.Y && boxMinPoint.Y < bucketBoxCenterPoint.Y)
                {
                    splits[0].Add(node);
                }

                if (bucketBoxCenterPoint.X < boxMaxPoint.X && boxMinPoint.X < bucketBoxMaxPoint.X &&
                    bucketBoxMinPoint.Y < boxMaxPoint.Y && boxMinPoint.Y < bucketBoxCenterPoint.Y)
                {
                    splits[1].Add(node);
                }

                if (bucketBoxMinPoint.X < boxMaxPoint.X && boxMinPoint.X < bucketBoxCenterPoint.X &&
                    bucketBoxCenterPoint.Y < boxMaxPoint.Y && boxMinPoint.Y < bucketBoxMaxPoint.Y)
                {
                    splits[2].Add(node);
                }

                if (bucketBoxCenterPoint.X < boxMaxPoint.X && boxMinPoint.X < bucketBoxMaxPoint.X &&
                    bucketBoxCenterPoint.Y < boxMaxPoint.Y && boxMinPoint.Y < bucketBoxMaxPoint.Y)
                {
                    splits[3].Add(node);
                }
            }

            output[0] = bucket.CreateInstance(
                splits[0].ToArray(),
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                    bucket.Level + 1
                );

            output[1] = bucket.CreateInstance(
                splits[1].ToArray(),
                new Rectangle3D(
                    bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                    bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                    bucket.Level + 1
                );

            output[2] = bucket.CreateInstance(
                splits[2].ToArray(),
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                    bucket.Level + 1
                );

            output[3] = bucket.CreateInstance(
                splits[3].ToArray(),
                new Rectangle3D(
                    bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                    bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                    bucket.Level + 1
                );

            return output;
        }

        internal static IBoxBucket<T>[] CenterXsplit2<T>(IBoxBucket<T> bucket) where T : IBox
        {
            var output = new IBoxBucket<T>[2];

            var splits = new List<T>[2];
            splits[0] = new List<T>();
            splits[1] = new List<T>();

            var bucketBoxMinPoint = bucket.Box.MinPoint;
            var bucketBoxMaxPoint = bucket.Box.MaxPoint;
            var bucketCenterBoxMinPoint = bucket.CenterBox.MinPoint;
            var bucketCenterBoxMaxPoint = bucket.CenterBox.MaxPoint;
            var bucketBoxCenterPoint = bucket.Box.CenterPoint;

            foreach (var node in bucket.BoxNodes)
            {
                var box = node.Box;
                var boxMinPoint = box.MinPoint;
                var boxMaxPoint = box.MaxPoint;

                if (bucketCenterBoxMinPoint.Y > boxMaxPoint.Y || boxMinPoint.Y > bucketCenterBoxMaxPoint.Y ||
                    bucketCenterBoxMinPoint.Z > boxMaxPoint.Z || boxMinPoint.Z > bucketCenterBoxMaxPoint.Z) { continue; }

                if (bucketBoxMinPoint.X < boxMaxPoint.X && boxMinPoint.X < bucketBoxCenterPoint.X)
                {
                    splits[0].Add(node);
                }
                if (bucketBoxCenterPoint.X < boxMaxPoint.X && boxMinPoint.X < bucketBoxMaxPoint.X)
                {
                    splits[1].Add(node);
                }
            }

            output[0] = bucket.CreateInstance(
                splits[0].ToArray(),
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                    bucket.Level + 1
                );

            output[1] = bucket.CreateInstance(
                splits[1].ToArray(),
                new Rectangle3D(
                    bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                    bucket.Level + 1
                );

            return output;
        }

        internal static IBoxBucket<T>[] CenterYsplit2<T>(IBoxBucket<T> bucket) where T : IBox
        {
            var output = new IBoxBucket<T>[2];

            var splits = new List<T>[2];
            splits[0] = new List<T>();
            splits[1] = new List<T>();

            var bucketBoxMinPoint = bucket.Box.MinPoint;
            var bucketBoxMaxPoint = bucket.Box.MaxPoint;
            var bucketCenterBoxMinPoint = bucket.CenterBox.MinPoint;
            var bucketCenterBoxMaxPoint = bucket.CenterBox.MaxPoint;
            var bucketBoxCenterPoint = bucket.Box.CenterPoint;

            foreach (var node in bucket.BoxNodes)
            {
                var box = node.Box;
                var boxMinPoint = box.MinPoint;
                var boxMaxPoint = box.MaxPoint;

                if (bucketCenterBoxMinPoint.X > boxMaxPoint.X || boxMinPoint.X > bucketCenterBoxMaxPoint.X ||
                    bucketCenterBoxMinPoint.Z > boxMaxPoint.Z || boxMinPoint.Z > bucketCenterBoxMaxPoint.Z) { continue; }

                if (bucketBoxMinPoint.Y < boxMaxPoint.Y && boxMinPoint.Y < bucketBoxCenterPoint.Y)
                {
                    splits[0].Add(node);
                }
                if (bucketBoxCenterPoint.Y < boxMaxPoint.Y && boxMinPoint.Y < bucketBoxMaxPoint.Y)
                {
                    splits[1].Add(node);
                }
            }

            output[0] = bucket.CreateInstance(
                splits[0].ToArray(),
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                    bucket.Level + 1
                );

            output[1] = bucket.CreateInstance(
                splits[1].ToArray(),
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                    bucket.Level + 1
                );

            return output;
        }

        internal static IBoxBucket<T>[] CenterZsplit2<T>(IBoxBucket<T> bucket) where T : IBox
        {
            var output = new IBoxBucket<T>[2];

            var splits = new List<T>[2];
            splits[0] = new List<T>();
            splits[1] = new List<T>();

            var bucketBoxMinPoint = bucket.Box.MinPoint;
            var bucketBoxMaxPoint = bucket.Box.MaxPoint;
            var bucketCenterBoxMinPoint = bucket.CenterBox.MinPoint;
            var bucketCenterBoxMaxPoint = bucket.CenterBox.MaxPoint;
            var bucketBoxCenterPoint = bucket.Box.CenterPoint;

            foreach (var node in bucket.BoxNodes)
            {
                var box = node.Box;
                var boxMinPoint = box.MinPoint;
                var boxMaxPoint = box.MaxPoint;

                if (bucketCenterBoxMinPoint.X > boxMaxPoint.X || boxMinPoint.X > bucketCenterBoxMaxPoint.X ||
                    bucketCenterBoxMinPoint.Y > boxMaxPoint.Y || boxMinPoint.Y > bucketCenterBoxMaxPoint.Y) { continue; }

                if (bucketBoxMinPoint.Z < boxMaxPoint.Z && boxMinPoint.Z < bucketBoxCenterPoint.Z)
                {
                    splits[0].Add(node);
                }
                if (bucketBoxCenterPoint.Z < boxMaxPoint.Z&& boxMinPoint.Z < bucketBoxMaxPoint.Z)
                {
                    splits[1].Add(node);
                }
            }

            output[0] = bucket.CreateInstance(
                splits[0].ToArray(),
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                    bucket.Level + 1
                );

            output[1] = bucket.CreateInstance(
                splits[1].ToArray(),
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z),
                    bucket.Level + 1
                );

            return output;
        }

        internal static IBoxBucket<T> CenterFilter<T>(IBoxBucket<T> bucket) where T : IBox
        {
            var center = new List<T>();

            var bucketCenterBoxMinPoint = bucket.CenterBox.MinPoint;
            var bucketCenterBoxMaxPoint = bucket.CenterBox.MaxPoint;

            foreach (var node in bucket.BoxNodes)
            {
                var box = node.Box;
                var boxMinPoint = box.MinPoint;
                var boxMaxPoint = box.MaxPoint;

                if (bucketCenterBoxMinPoint.X < boxMaxPoint.X && boxMinPoint.X < bucketCenterBoxMaxPoint.X &&
                    bucketCenterBoxMinPoint.Y < boxMaxPoint.Y && boxMinPoint.Y < bucketCenterBoxMaxPoint.Y &&
                    bucketCenterBoxMinPoint.Z < boxMaxPoint.Z && boxMinPoint.Z < bucketCenterBoxMaxPoint.Z)
                {
                    center.Add(node);
                }
            }

            var output = bucket.CreateInstance(
                center.ToArray(),
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                    bucket.Level + 1
                );

            return output;
        }
    }
}
