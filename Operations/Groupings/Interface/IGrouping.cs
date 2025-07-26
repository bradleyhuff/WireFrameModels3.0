using Operations.Groupings.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Groupings.Interface
{
    internal interface IGrouping
    {
        public GroupingTriangle FirstTriangle { get; set; }
        public bool EdgeFilter(GroupingTriangle t, IEnumerable<GroupingTriangle> triangles);
        public bool TriangleFilter(GroupingTriangle a, GroupingTriangle b);
        public void SeedSet(GroupingTriangle triangle);
    }
}
