﻿using Collections.WireFrameMesh.Basics;
using Operations.PositionRemovals.Interfaces;

namespace Operations.PositionRemovals.Conditionals
{
    internal class SurfaceEdgeConditionals : ISharedFillConditionals
    {
        public bool AllowFill(PositionNormal a, PositionNormal b, PositionNormal c)
        {
            return a.PositionObject.Cardinality < 2 || b.PositionObject.Cardinality < 2 || c.PositionObject.Cardinality < 2;
        }
    }
}