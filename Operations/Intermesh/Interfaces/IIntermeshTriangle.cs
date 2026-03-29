using BasicObjects.GeometricObjects;
using Collections.Buckets.Interfaces;
using Operations.Intermesh.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Interfaces
{
    internal interface IIntermeshTriangle: IBox
    {
        int Id { get; }
        Triangle3D Triangle { get; }
        List<IIntermeshTriangle> Gathering { get; }
        List<IIntermeshTriangle> IntersectingTriangles { get; }
        Dictionary<int, IntermeshIntersection> GatheringSets { get; }
    }
}
