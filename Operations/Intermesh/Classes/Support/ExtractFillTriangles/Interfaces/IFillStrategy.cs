using Operations.Intermesh.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Classes.Support.ExtractFillTriangles.Interfaces
{
    internal interface IFillStrategy
    {
        public void GetFillTriangles(IntermeshTriangle triangle);
        public bool ShouldUseStrategy(IntermeshTriangle triangle);
    }
}
