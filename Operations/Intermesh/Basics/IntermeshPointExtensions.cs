using BasicObjects.GeometricObjects;
using Collections.Buckets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshPointExtensions
    {
        private static BoxBucket<IntermeshPoint> bucket = new BoxBucket<IntermeshPoint>();

        internal static IntermeshPoint Fetch(Point3D point)
        {
            var match = bucket.Fetch(new Rectangle3D(point, BoxBucket.MARGINS));
            var found = match.Where(m => Point3D.AreEqual(m.Point, point, 1e-15)).MinBy(p => Point3D.Distance(p.Point, point));
            if (found is not null)
            {
                return found;
            }
            var intermeshPoint = new IntermeshPoint(point);
            bucket.Add(intermeshPoint);
            return intermeshPoint;
        }
    }
}
