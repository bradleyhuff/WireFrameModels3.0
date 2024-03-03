using Collections.Buckets.Interfaces;
using BasicObjects.GeometricObjects;

namespace Collections.Buckets
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

        internal static IBoxBucketInternal<T>[] XYZsplit8<T>(IBoxBucketInternal<T> bucket) where T : IBox
        {
            var output = new IBoxBucketInternal<T>[8];

            var bucketBoxMinPoint = bucket.Box.MinPoint;
            var bucketBoxMaxPoint = bucket.Box.MaxPoint;
            var bucketBoxCenterPoint = bucket.Box.CenterPoint;

            output[0] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                    bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                bucket.BoxNodes
                );

            output[1] = bucket.CreateInstance(
                new Rectangle3D(
                bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                bucket.BoxNodes
            );

            output[2] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                    bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                bucket.BoxNodes
            );

            output[3] = bucket.CreateInstance(
                 new Rectangle3D(
                     bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                     bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                     bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                 bucket.BoxNodes
             );

            output[4] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z),
                bucket.BoxNodes
                );

            output[5] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                    bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z),
                bucket.BoxNodes
            );

            output[6] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z), 
                bucket.BoxNodes
                );

            output[7] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                    bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z),
                bucket.BoxNodes
            );

            return output;
        }

        internal static IBoxBucketInternal<T>[] CenterYZsplit4<T>(IBoxBucketInternal<T> bucket) where T : IBox
        {
            var output = new IBoxBucketInternal<T>[4];

            var bucketBoxMinPoint = bucket.Box.MinPoint;
            var bucketBoxMaxPoint = bucket.Box.MaxPoint;
            var bucketBoxCenterPoint = bucket.Box.CenterPoint;
            var bucketCenterBoxMinPoint = bucket.CenterBox.MinPoint;
            var bucketCenterBoxMaxPoint = bucket.CenterBox.MaxPoint;

            output[0] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                    bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                bucket.BoxNodes
            );

            output[1] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                    bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                bucket.BoxNodes
                );

            output[2] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z),
                bucket.BoxNodes
                );

            output[3] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z),
                bucket.BoxNodes
                );

            return output;
        }

        internal static IBoxBucketInternal<T>[] CenterXZsplit4<T>(IBoxBucketInternal<T> bucket) where T : IBox
        {
            var output = new IBoxBucketInternal<T>[4];

            var bucketBoxMinPoint = bucket.Box.MinPoint;
            var bucketBoxMaxPoint = bucket.Box.MaxPoint;
            var bucketCenterBoxMinPoint = bucket.CenterBox.MinPoint;
            var bucketCenterBoxMaxPoint = bucket.CenterBox.MaxPoint;
            var bucketBoxCenterPoint = bucket.Box.CenterPoint;

            output[0] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                bucket.BoxNodes
                );

            output[1] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                bucket.BoxNodes
                );

            output[2] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z),
                bucket.BoxNodes
                );

            output[3] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z),
                bucket.BoxNodes
                );

            return output;
        }

        internal static IBoxBucketInternal<T>[] CenterXYsplit4<T>(IBoxBucketInternal<T> bucket) where T : IBox
        {
            var output = new IBoxBucketInternal<T>[4];

            var bucketBoxMinPoint = bucket.Box.MinPoint;
            var bucketBoxMaxPoint = bucket.Box.MaxPoint;
            var bucketCenterBoxMinPoint = bucket.CenterBox.MinPoint;
            var bucketCenterBoxMaxPoint = bucket.CenterBox.MaxPoint;
            var bucketBoxCenterPoint = bucket.Box.CenterPoint;

            output[0] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                bucket.BoxNodes
                );

            output[1] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                    bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                bucket.BoxNodes
                );

            output[2] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                bucket.BoxNodes
                );

            output[3] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                    bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                bucket.BoxNodes
                );

            return output;
        }

        internal static IBoxBucketInternal<T>[] CenterXsplit2<T>(IBoxBucketInternal<T> bucket) where T : IBox
        {
            var output = new IBoxBucketInternal<T>[2];

            var bucketBoxMinPoint = bucket.Box.MinPoint;
            var bucketBoxMaxPoint = bucket.Box.MaxPoint;
            var bucketCenterBoxMinPoint = bucket.CenterBox.MinPoint;
            var bucketCenterBoxMaxPoint = bucket.CenterBox.MaxPoint;
            var bucketBoxCenterPoint = bucket.Box.CenterPoint;

            output[0] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketBoxMinPoint.X, bucketBoxCenterPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                bucket.BoxNodes
                );

            output[1] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketBoxCenterPoint.X, bucketBoxMaxPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                bucket.BoxNodes
                );

            return output;
        }

        internal static IBoxBucketInternal<T>[] CenterYsplit2<T>(IBoxBucketInternal<T> bucket) where T : IBox
        {
            var output = new IBoxBucketInternal<T>[2];

            var bucketBoxMinPoint = bucket.Box.MinPoint;
            var bucketBoxMaxPoint = bucket.Box.MaxPoint;
            var bucketCenterBoxMinPoint = bucket.CenterBox.MinPoint;
            var bucketCenterBoxMaxPoint = bucket.CenterBox.MaxPoint;
            var bucketBoxCenterPoint = bucket.Box.CenterPoint;

            output[0] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketBoxMinPoint.Y, bucketBoxCenterPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                bucket.BoxNodes
                );

            output[1] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketBoxCenterPoint.Y, bucketBoxMaxPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                bucket.BoxNodes
                );

            return output;
        }

        internal static IBoxBucketInternal<T>[] CenterZsplit2<T>(IBoxBucketInternal<T> bucket) where T : IBox
        {
            var output = new IBoxBucketInternal<T>[2];

            var bucketBoxMinPoint = bucket.Box.MinPoint;
            var bucketBoxMaxPoint = bucket.Box.MaxPoint;
            var bucketCenterBoxMinPoint = bucket.CenterBox.MinPoint;
            var bucketCenterBoxMaxPoint = bucket.CenterBox.MaxPoint;
            var bucketBoxCenterPoint = bucket.Box.CenterPoint;

            output[0] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketBoxMinPoint.Z, bucketBoxCenterPoint.Z),
                bucket.BoxNodes
                );

            output[1] = bucket.CreateInstance(
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketBoxCenterPoint.Z, bucketBoxMaxPoint.Z),
                bucket.BoxNodes
                );

            return output;
        }

        internal static IBoxBucketInternal<T> CenterFilter<T>(IBoxBucketInternal<T> bucket) where T : IBox
        {
            var bucketCenterBoxMinPoint = bucket.CenterBox.MinPoint;
            var bucketCenterBoxMaxPoint = bucket.CenterBox.MaxPoint;

            var output = bucket.CreateInstance(
                new Rectangle3D(
                    bucketCenterBoxMinPoint.X, bucketCenterBoxMaxPoint.X,
                    bucketCenterBoxMinPoint.Y, bucketCenterBoxMaxPoint.Y,
                    bucketCenterBoxMinPoint.Z, bucketCenterBoxMaxPoint.Z),
                bucket.BoxNodes
                );

            return output;
        }
    }
}
