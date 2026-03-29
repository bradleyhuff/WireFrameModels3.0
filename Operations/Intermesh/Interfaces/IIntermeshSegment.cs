using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Interfaces
{
    internal interface IIntermeshSegment
    {
        int Id { get; }
        Combination2 Key { get; }
        Rectangle3D Box { get; }
        LineSegment3D Segment { get; }
    }
}
