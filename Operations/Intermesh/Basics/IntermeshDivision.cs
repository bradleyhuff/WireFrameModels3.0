using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    public class IntermeshDivision
    {
        private static int _id = 0;
        public IntermeshDivision()
        {
            Id = _id++;
        }

        public int Id { get; }
        public LineSegment3D? Division { get; set; }
    }
}
