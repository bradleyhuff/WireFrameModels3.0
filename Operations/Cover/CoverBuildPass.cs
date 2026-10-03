using Operations.Intermesh.Basics;
using Operations.PlanarFilling.Basics;
using Operations.SurfaceSegmentChaining.Basics;
using Operations.SurfaceSegmentChaining.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Cover
{
    internal class CoverBuildPass
    {
        public CoverBuildPass(ISurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint> chain,
            (SurfaceRayContainer<IntermeshPoint>[] Loop, Rank Rank) coverLoop,
            IntermeshTriangle triangle,
            CoverSet[] coveredLoops
            )
        {
            Chain = chain;
            CoverLoop = coverLoop;
            Triangle = triangle;
            CoveredLoops = coveredLoops;
        }
        public ISurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint> Chain { get; } 
        public (SurfaceRayContainer<IntermeshPoint>[] Loop, Rank Rank) CoverLoop { get; } 
        public IntermeshTriangle Triangle { get; }
        public CoverSet[] CoveredLoops { get; }
    }
}
