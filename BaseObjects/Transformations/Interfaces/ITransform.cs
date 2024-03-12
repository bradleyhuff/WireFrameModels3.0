using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseObjects.Transformations.Interfaces
{
    public interface ITransform
    {
        public Point3D Apply(Point3D point);
        public Vector3D Apply(Point3D point, Vector3D normal);
    }
}
