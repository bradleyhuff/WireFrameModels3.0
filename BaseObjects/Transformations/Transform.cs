using BaseObjects.Transformations.Interfaces;
using BasicObjects.GeometricObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BaseObjects.Transformations
{
    public class Transform : ITransform
    {
        private double p00;
        private double p01;
        private double p02;
        private double p03;

        private double p10;
        private double p11;
        private double p12;
        private double p13;

        private double p20;
        private double p21;
        private double p22;
        private double p23;

        private double p30;
        private double p31;
        private double p32;
        private double p33;


        private double n00;
        private double n01;
        private double n02;

        private double n10;
        private double n11;
        private double n12;

        private double n20;
        private double n21;
        private double n22;

        public Point3D Apply(Point3D point)
        {
            double x = p00 * point.X + p01 * point.Y + p02 * point.Z + p03;
            double y = p10 * point.X + p11 * point.Y + p12 * point.Z + p13;
            double z = p20 * point.X + p21 * point.Y + p22 * point.Z + p23;

            return new Point3D(x, y, z);
        }

        public Vector3D Apply(Point3D point, Vector3D normal)
        {
            double x = n00 * normal.X + n01 * normal.Y + n02 * normal.Z;
            double y = n10 * normal.X + n11 * normal.Y + n12 * normal.Z;
            double z = n20 * normal.X + n21 * normal.Y + n22 * normal.Z;

            return new Vector3D(x, y, z).Direction;
        }

        public Transform Rotate(Vector3D axis, double angle)
        {
            return Rotation(axis, angle);
        }
        public Transform Reflect(Vector3D n)
        {
            return Reflection(n) * this;
        }
        public Transform ShearAtXY(double x, double y)
        {
            return ShearXY(x, y) * this;
        }
        public Transform ShearAtYZ(double y, double z)
        {
            return ShearYZ(y, z) * this;
        }
        public Transform ShearAtXZ(double x, double z)
        {
            return ShearXZ(x, z) * this;
        }
        public Transform Translate(Point3D point)
        {
            return Translation(point) * this;
        }
        public Transform AtScale(double size)
        {
            return Scale(size) * this;
        }
        public Transform AtScale(double x, double y, double z)
        {
            return Scale(x, y, z) * this;
        }

        public Transform AtPoint(Point3D point)
        {
            return Translation(point) * this * Translation(-1 * point);
        }
        public Transform AtAxis(Vector3D axis, double angle)
        {
            return Rotation(axis, angle) * this * Rotation(axis, -angle);
        }

        public static Transform operator *(Transform a, Transform b)
        {
            Transform c = new Transform();

            c.p00 = a.p00 * b.p00 + a.p01 * b.p10 + a.p02 * b.p20 + a.p03 * b.p30;
            c.p01 = a.p00 * b.p01 + a.p01 * b.p11 + a.p02 * b.p21 + a.p03 * b.p31;
            c.p02 = a.p00 * b.p02 + a.p01 * b.p12 + a.p02 * b.p22 + a.p03 * b.p32;
            c.p03 = a.p00 * b.p03 + a.p01 * b.p13 + a.p02 * b.p23 + a.p03 * b.p33;

            c.p10 = a.p10 * b.p00 + a.p11 * b.p10 + a.p12 * b.p20 + a.p13 * b.p30;
            c.p11 = a.p10 * b.p01 + a.p11 * b.p11 + a.p12 * b.p21 + a.p13 * b.p31;
            c.p12 = a.p10 * b.p02 + a.p11 * b.p12 + a.p12 * b.p22 + a.p13 * b.p32;
            c.p13 = a.p10 * b.p03 + a.p11 * b.p13 + a.p12 * b.p23 + a.p13 * b.p33;

            c.p20 = a.p20 * b.p00 + a.p21 * b.p10 + a.p22 * b.p20 + a.p23 * b.p30;
            c.p21 = a.p20 * b.p01 + a.p21 * b.p11 + a.p22 * b.p21 + a.p23 * b.p31;
            c.p22 = a.p20 * b.p02 + a.p21 * b.p12 + a.p22 * b.p22 + a.p23 * b.p32;
            c.p23 = a.p20 * b.p03 + a.p21 * b.p13 + a.p22 * b.p23 + a.p23 * b.p33;

            c.p30 = a.p30 * b.p00 + a.p31 * b.p10 + a.p32 * b.p20 + a.p33 * b.p30;
            c.p31 = a.p30 * b.p01 + a.p31 * b.p11 + a.p32 * b.p21 + a.p33 * b.p31;
            c.p32 = a.p30 * b.p02 + a.p31 * b.p12 + a.p32 * b.p22 + a.p33 * b.p32;
            c.p33 = a.p30 * b.p03 + a.p31 * b.p13 + a.p32 * b.p23 + a.p33 * b.p33;


            c.n00 = a.n00 * b.n00 + a.n01 * b.n10 + a.n02 * b.n20;
            c.n01 = a.n00 * b.n01 + a.n01 * b.n11 + a.n02 * b.n21;
            c.n02 = a.n00 * b.n02 + a.n01 * b.n12 + a.n02 * b.n22;

            c.n10 = a.n10 * b.n00 + a.n11 * b.n10 + a.n12 * b.n20;
            c.n11 = a.n10 * b.n01 + a.n11 * b.n11 + a.n12 * b.n21;
            c.n12 = a.n10 * b.n02 + a.n11 * b.n12 + a.n12 * b.n22;

            c.n20 = a.n20 * b.n00 + a.n21 * b.n10 + a.n22 * b.n20;
            c.n21 = a.n20 * b.n01 + a.n21 * b.n11 + a.n22 * b.n21;
            c.n22 = a.n20 * b.n02 + a.n21 * b.n12 + a.n22 * b.n22;

            return c;
        }

        public static Transform Reflection(Vector3D n)
        {
            n = n.Direction;

            Transform t = new Transform();
            t.p00 = 1 - 2 * n.X * n.X;
            t.p01 = -2 * n.X * n.Y;
            t.p02 = -2 * n.X * n.Z;
            t.p03 = 0;

            t.p10 = -2 * n.X * n.Y;
            t.p11 = 1 - 2 * n.Y * n.Y;
            t.p12 = -2 * n.Y * n.Z;
            t.p13 = 0;

            t.p20 = -2 * n.X * n.Z;
            t.p21 = -2 * n.Y * n.Z;
            t.p22 = 1 - 2 * n.Z * n.Z;
            t.p23 = 0;

            t.p30 = 0;
            t.p31 = 0;
            t.p32 = 0;
            t.p33 = 1;

            t.n00 = t.p00;
            t.n01 = t.p01;
            t.n02 = t.p02;

            t.n10 = t.p10;
            t.n11 = t.p11;
            t.n12 = t.p12;

            t.n20 = t.p20;
            t.n21 = t.p21;
            t.n22 = t.p22;

            return t;

        }

        public static Transform Rotation(Vector3D axis, double angle)
        {
            axis = axis.Direction;
            var cc = 1 - Math.Cos(angle);
            var s = Math.Sin(angle);
            var c = Math.Cos(angle);

            Transform t = new Transform();
            t.p00 = axis.X * axis.X * cc + c;
            t.p01 = axis.Y * axis.X * cc - axis.Z * s;
            t.p02 = axis.Z * axis.X * cc + axis.Y * s;
            t.p03 = 0;

            t.p10 = axis.X * axis.Y * cc + axis.Z * s;
            t.p11 = axis.Y * axis.Y * cc + c;
            t.p12 = axis.Z * axis.Y * cc - axis.X * s;
            t.p13 = 0;

            t.p20 = axis.X * axis.Z * cc - axis.Y * s;
            t.p21 = axis.Y * axis.Z * cc + axis.X * s;
            t.p22 = axis.Z * axis.Z * cc + c;
            t.p23 = 0;

            t.p30 = 0;
            t.p31 = 0;
            t.p32 = 0;
            t.p33 = 1;

            t.n00 = t.p00;
            t.n01 = t.p01;
            t.n02 = t.p02;

            t.n10 = t.p10;
            t.n11 = t.p11;
            t.n12 = t.p12;

            t.n20 = t.p20;
            t.n21 = t.p21;
            t.n22 = t.p22;

            return t;
        }

        public static Transform Scale(double size)
        {
            return Scale(size, size, size);
        }

        public static Transform Scale(double x, double y, double z)
        {
            Transform t = new Transform();
            t.p00 = x;
            t.p01 = 0;
            t.p02 = 0;
            t.p03 = 0;

            t.p10 = 0;
            t.p11 = y;
            t.p12 = 0;
            t.p13 = 0;

            t.p20 = 0;
            t.p21 = 0;
            t.p22 = z;
            t.p23 = 0;

            t.p30 = 0;
            t.p31 = 0;
            t.p32 = 0;
            t.p33 = 1;

            t.n00 = 1 / t.p00;
            t.n01 = t.p01;
            t.n02 = t.p02;

            t.n10 = t.p10;
            t.n11 = 1 / t.p11;
            t.n12 = t.p12;

            t.n20 = t.p20;
            t.n21 = t.p21;
            t.n22 = 1 / t.p22;

            return t;
        }

        public static Transform ShearXY(double x, double y)
        {
            Transform t = new Transform();
            t.p00 = 1;
            t.p01 = 0;
            t.p02 = x;
            t.p03 = 0;

            t.p10 = 0;
            t.p11 = 1;
            t.p12 = y;
            t.p13 = 0;

            t.p20 = 0;
            t.p21 = 0;
            t.p22 = 1;
            t.p23 = 0;

            t.p30 = 0;
            t.p31 = 0;
            t.p32 = 0;
            t.p33 = 1;

            t.n00 = 1;
            t.n01 = 0;
            t.n02 = 0;

            t.n10 = 0;
            t.n11 = 1;
            t.n12 = 0;

            t.n20 = -x;
            t.n21 = -y;
            t.n22 = 1;

            return t;
        }

        public static Transform ShearXZ(double x, double z)
        {
            Transform t = new Transform();
            t.p00 = 1;
            t.p01 = x;
            t.p02 = 0;
            t.p03 = 0;

            t.p10 = 0;
            t.p11 = 1;
            t.p12 = 0;
            t.p13 = 0;

            t.p20 = 0;
            t.p21 = z;
            t.p22 = 1;
            t.p23 = 0;

            t.p30 = 0;
            t.p31 = 0;
            t.p32 = 0;
            t.p33 = 1;

            t.n00 = 1;
            t.n01 = 0;
            t.n02 = 0;

            t.n10 = -x;
            t.n11 = 1;
            t.n12 = -z;

            t.n20 = 0;
            t.n21 = 0;
            t.n22 = 1;

            return t;
        }

        public static Transform ShearYZ(double y, double z)
        {
            Transform t = new Transform();
            t.p00 = 1;
            t.p01 = 0;
            t.p02 = 0;
            t.p03 = 0;

            t.p10 = y;
            t.p11 = 1;
            t.p12 = 0;
            t.p13 = 0;

            t.p20 = z;
            t.p21 = 0;
            t.p22 = 1;
            t.p23 = 0;

            t.p30 = 0;
            t.p31 = 0;
            t.p32 = 0;
            t.p33 = 1;

            t.n00 = 1;
            t.n01 = -y;
            t.n02 = -z;

            t.n10 = 0;
            t.n11 = 1;
            t.n12 = 0;

            t.n20 = 0;
            t.n21 = 0;
            t.n22 = 1;

            return t;
        }

        public static Transform Translation(Point3D point)
        {
            Transform t = new Transform();
            t.p00 = 1;
            t.p01 = 0;
            t.p02 = 0;
            t.p03 = point.X;

            t.p10 = 0;
            t.p11 = 1;
            t.p12 = 0;
            t.p13 = point.Y;

            t.p20 = 0;
            t.p21 = 0;
            t.p22 = 1;
            t.p23 = point.Z;

            t.p30 = 0;
            t.p31 = 0;
            t.p32 = 0;
            t.p33 = 1;

            t.n00 = 1;
            t.n01 = 0;
            t.n02 = 0;

            t.n10 = 0;
            t.n11 = 1;
            t.n12 = 0;

            t.n20 = 0;
            t.n21 = 0;
            t.n22 = 1;

            t.n20 = t.p20;
            t.n21 = t.p21;
            t.n22 = t.p22;

            return t;
        }
    }
}
