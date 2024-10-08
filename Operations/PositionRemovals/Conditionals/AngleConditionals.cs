﻿using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Basics;
using Operations.PositionRemovals.Interfaces;

namespace Operations.PositionRemovals.Conditionals
{
    internal class AngleConditionals : IFillConditionals
    {
        public bool Unconditional { get; set; }
        public double MaxAngle { get; set; }

        public bool AllowFill(PositionNormal a, PositionNormal b, PositionNormal c)
        {
            if (Unconditional) return true;
            if (a.PositionObject.Cardinality > 2) return true;
            if (b.PositionObject.Cardinality > 2) return true;
            if (c.PositionObject.Cardinality > 2) return true;

            var vectorA = (a.Position - b.Position).Direction;
            var vectorB = (c.Position - b.Position).Direction;

            var angle = Vector3D.Angle(vectorA, vectorB);

            return angle < MaxAngle;
        }

        public bool AllowFill(Position a, Position b, Position c)
        {
            if (Unconditional) return true;
            if (a.Cardinality > 2) return true;
            if (b.Cardinality > 2) return true;
            if (c.Cardinality > 2) return true;

            var vectorA = (a.Point - b.Point).Direction;
            var vectorB = (c.Point - b.Point).Direction;

            var angle = Vector3D.Angle(vectorA, vectorB);

            return angle < MaxAngle;
        }
    }
}
