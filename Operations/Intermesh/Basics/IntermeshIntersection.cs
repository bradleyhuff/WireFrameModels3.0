using BasicObjects.GeometricObjects;
using Collections.Buckets.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    public class IntermeshIntersection : IBox
    {
        private static int _id = 0;

        public IntermeshIntersection()
        {
            Id = _id++;
        }
        public int Id { get; }
        public bool IsSet { get; set; }
        public bool Disabled { get; set; }

        private LineSegment3D _intersection;
        public LineSegment3D? Intersection
        {
            get { return _intersection; }
            set { _intersection = value; }
        }

        private Rectangle3D? _box;
        public Rectangle3D? Box
        {
            get
            {
                if (_box is null)
                {
                    _box = Rectangle3D.Containing(Intersection).Margin(1e-3);
                }
                return _box;
            }
        }

        private List<IntermeshDivision> _divisions = new List<IntermeshDivision>();
        public IReadOnlyList<IntermeshDivision> Divisions
        {
            get { return _divisions; }
        }
    }
}
