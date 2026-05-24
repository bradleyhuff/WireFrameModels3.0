using Operations.Intermesh.Basics;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Classes.Support.ExtractFillTriangles
{
    internal class SimpleFillStrategy : IFillStrategy
    {
        public void GetFillTriangles(IntermeshTriangle triangle)
        {
            throw new NotImplementedException();
        }

        public bool ShouldUseStrategy(IntermeshTriangle triangle)
        {
            throw new NotImplementedException();
        }
    }
}
