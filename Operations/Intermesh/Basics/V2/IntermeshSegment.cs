using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using Collections.Buckets;
using Collections.Buckets.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics.V2
{
    internal class IntermeshSegment : IBox
    {
        private static int _id = 0;
        public IntermeshSegment(IntermeshPoint a, IntermeshPoint b)
        {
            A = a;
            B = b;
            Id = _id++;
            Key = new Combination2(a.Id, b.Id);
            _divisionPoints.Add(A);
            _divisionPoints.Add(B);
            Segment = new LineSegment3D(a.Point, b.Point);
        }

        public int Id { get; }
        public Combination2 Key { get; }

        public IntermeshPoint A { get; }
        public IntermeshPoint B { get; }
        public LineSegment3D Segment { get; }

        private List<IntermeshPoint> _divisionPoints = new List<IntermeshPoint>();

        public IReadOnlyList<IntermeshPoint> DivisionPoints
        {
            get { return _divisionPoints; }
        }

        public bool HasDivisionPoints { get { return InternalDivisions > 0; } }
        public int InternalDivisions { get { return _divisionPoints.Count() - 2; } }

        public bool Add(IntermeshPoint division)
        {
            if (_divisionPoints.Any(t => t.Id == division.Id)) { return false; }
            _divisionPoints.Add(division);
            return true;
        }

        private Rectangle3D _box;
        public Rectangle3D Box
        {
            get
            {
                if (_box is null && A is not null && B is not null)
                {
                    _box = Rectangle3D.Containing(A.Point, B.Point).Margin(BoxBucket.MARGINS);
                }
                return _box;
            }
        }

        public override int GetHashCode()
        {
            return Id;
        }

        private List<IntermeshTriangle> _triangles = new List<IntermeshTriangle>();
        public IReadOnlyList<IntermeshTriangle> Triangles
        {
            get { return _triangles; }
        }
        public bool Add(IntermeshTriangle triangle)
        {
            if (_triangles.Any(t => t.Id == triangle.Id)) { return false; }
            _triangles.Add(triangle);
            return true;
        }

        public IEnumerable<IntermeshDivision> BuildDivisions()
        {
            var orderedPoints = _divisionPoints.OrderBy(p => Point3D.Distance(p.Point, A.Point)).ToArray();

            for (int i = 0; i < orderedPoints.Length - 1; i++)
            {
                yield return new IntermeshDivision(orderedPoints[i], orderedPoints[i + 1]);
            }
        }
    }
}
