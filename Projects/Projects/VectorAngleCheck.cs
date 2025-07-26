using BaseObjects.Transformations;
using BasicObjects.GeometricObjects;
using BasicObjects.MathExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireFrameModels3._0;

namespace Projects.Projects
{
    public class VectorAngleCheck : ProjectBase
    {
        protected override void RunProject()
        {
            var vectorA = new Vector3D(0,0,1);
            var vectorB = vectorA.Transform(Transform.Rotation(Vector3D.BasisX, 1e-15));

            Console.WriteLine($"Angle {Vector3D.Angle(vectorA, vectorB)}");
        }
    }
}
