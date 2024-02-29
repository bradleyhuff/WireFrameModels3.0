using Collections.Buckets.Interfaces;
using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collections.WireFrameBuilder.Basics
{
    public class PositionNormal : IBox
    {
        private static int _id = 0;

        private PositionNormal()
        {
            Id = _id++;
        }

        public PositionNormal(int index) : this()
        {
            Index = index;
        }

        public static PositionNormal CreateAndAdd(List<PositionNormal> list)
        {
            var node = new PositionNormal(list.Count);
            list.Add(node);
            return node;
        }

        public int Id { get; }
        public int Index { get; }
        public Position Position { get; set; }

        public Rectangle3D Box
        {
            get
            {
                return Position.Box;
            }
        }

        private Vector3D _normal = null;
        private Ray3D? _ray = null;

        public Vector3D Normal
        {
            get { return _normal; }
            set
            {
                _normal = value;
                _wasInverted = false;
            }
        }

        public Ray3D Ray
        {
            get
            {
                if (_ray is null && Position?.Point is not null && _normal is not null)
                {
                    _ray = new Ray3D(Position.Point, _normal);
                }
                return _ray;
            }
        }

        private bool _wasInverted = false;

        public void InvertNormal()
        {
            if (_wasInverted || _normal is null) { return; }
            Normal = -Normal;
            _wasInverted = true;
            _ray = null;
        }

        public void CommitInvert()
        {
            _wasInverted = false;
        }
    }
}
