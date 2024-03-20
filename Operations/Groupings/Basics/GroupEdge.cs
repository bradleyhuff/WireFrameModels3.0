using Collections.WireFrameMesh.Basics;
using Operations.Intermesh.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Groupings.Basics
{
    public class GroupEdge
    {
        internal GroupEdge(PositionNormal a, PositionNormal b)
        {
            A = a;
            B = b;
        }
        public PositionNormal A { get; private set; }
        public PositionNormal B { get; private set; }

        public bool IsDegenerate
        {
            get
            {
                return A.Id == B.Id;
            }
        }
    }
}
