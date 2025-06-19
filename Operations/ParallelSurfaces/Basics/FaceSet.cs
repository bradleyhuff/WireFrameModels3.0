using Collections.WireFrameMesh.Interfaces;
using Operations.Groupings.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.ParallelSurfaces.Basics
{
    public class FaceSet
    {
        private static int _id = 0;
        private static object lockObject = new object();
        public FaceSet(GroupingCollection face) 
        { 
            Face = face;
            lock (lockObject)
            {
                Id = _id++;
            }
        }
        public int Id { get; }
        public GroupingCollection Face { get; }

        public IWireFrameMesh FacePlate { get; set; }
    }
}
