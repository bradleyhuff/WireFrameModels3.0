using BasicObjects.MathExtensions;
using Operations.Intermesh.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Intermesh.Basics
{
    internal class IntermeshEdge : IIntermeshEdge
    {
        public List<IntermeshSegment> Segments { get; set; } = new List<IntermeshSegment>();

        public Combination2 Key
        {
            get
            {
                if ((Segments ?? new List<IntermeshSegment>()).Any())
                {
                    return new Combination2(Segments.First().A.Id, Segments.Last().B.Id);
                }
                throw new InvalidDataException("No segments are provided to get a key.");
            }
        }

        public IIntermeshEdge Switch()
        {
            return this;
        }
    }
}
