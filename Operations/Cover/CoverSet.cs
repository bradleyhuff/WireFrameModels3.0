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
    internal class CoverSet
    {
        public CoverSet(IntermeshTriangle triangle, ISurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint> chain,
            (PlanarFillingGroup FillGroup, SurfaceRayContainer<IntermeshPoint>[] Loop, Rank Rank) coverLoop,
            IEnumerable<(PlanarFillingGroup, SurfaceRayContainer<IntermeshPoint>[], Rank)> loops)
        {
            Triangle = triangle;
            Chain = chain;
            CoverLoop = coverLoop;
            Loops = loops;
        }
        public IntermeshTriangle Triangle { get; }
        public ISurfaceSegmentChaining<PlanarFillingGroup, IntermeshPoint> Chain { get; }
        public (PlanarFillingGroup FillGroup, SurfaceRayContainer<IntermeshPoint>[] Loop, Rank Rank) CoverLoop { get; }
        public IEnumerable<(PlanarFillingGroup FillGroup, SurfaceRayContainer<IntermeshPoint>[] Loop, Rank Rank)> Loops { get; }
    }
}
