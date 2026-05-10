using BasicObjects.MathExtensions;
using Operations.Intermesh.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Interfaces
{
    internal interface IIntermeshEdge
    {
        Combination2 OriginalKey { get; }
        List<IntermeshSegment> Segments { get; set; }
        IIntermeshEdge Switch();
    }
}
