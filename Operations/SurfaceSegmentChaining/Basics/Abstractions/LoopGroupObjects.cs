using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.SurfaceSegmentChaining.Basics.Abstractions
{
    public abstract class LoopGroupObjects
    {
        static int _id = 0;
        protected LoopGroupObjects() {
            Id = _id++;
        }

        public int Id { get; }
        public bool Disabled { get; set; }

        public abstract LoopGroupObjects Clone();
    }
}
