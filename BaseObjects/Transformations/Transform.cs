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
    public class Transform
    {
        private double m00;
        private double m01;
        private double m02;
        private double m03;

        private double m10;
        private double m11;
        private double m12;
        private double m13;

        private double m20;
        private double m21;
        private double m22;
        private double m23;

        private double m30;
        private double m31;
        private double m32;
        private double m33;

        public Point3D Apply(Point3D point)
        {
            double x = m00 * point.X + m01 * point.Y + m02 * point.Z + m03;
            double y = m10 * point.X + m11 * point.Y + m12 * point.Z + m13;
            double z = m20 * point.X + m21 * point.Y + m22 * point.Z + m23;

            return new Point3D(x, y, z);
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

            c.m00 = a.m00 * b.m00 + a.m01 * b.m10 + a.m02 * b.m20 + a.m03 * b.m30;
            c.m01 = a.m00 * b.m01 + a.m01 * b.m11 + a.m02 * b.m21 + a.m03 * b.m31;
            c.m02 = a.m00 * b.m02 + a.m01 * b.m12 + a.m02 * b.m22 + a.m03 * b.m32;
            c.m03 = a.m00 * b.m03 + a.m01 * b.m13 + a.m02 * b.m23 + a.m03 * b.m33;

            c.m10 = a.m10 * b.m00 + a.m11 * b.m10 + a.m12 * b.m20 + a.m13 * b.m30;
            c.m11 = a.m10 * b.m01 + a.m11 * b.m11 + a.m12 * b.m21 + a.m13 * b.m31;
            c.m12 = a.m10 * b.m02 + a.m11 * b.m12 + a.m12 * b.m22 + a.m13 * b.m32;
            c.m13 = a.m10 * b.m03 + a.m11 * b.m13 + a.m12 * b.m23 + a.m13 * b.m33;

            c.m20 = a.m20 * b.m00 + a.m21 * b.m10 + a.m22 * b.m20 + a.m23 * b.m30;
            c.m21 = a.m20 * b.m01 + a.m21 * b.m11 + a.m22 * b.m21 + a.m23 * b.m31;
            c.m22 = a.m20 * b.m02 + a.m21 * b.m12 + a.m22 * b.m22 + a.m23 * b.m32;
            c.m23 = a.m20 * b.m03 + a.m21 * b.m13 + a.m22 * b.m23 + a.m23 * b.m33;

            c.m30 = a.m30 * b.m00 + a.m31 * b.m10 + a.m32 * b.m20 + a.m33 * b.m30;
            c.m31 = a.m30 * b.m01 + a.m31 * b.m11 + a.m32 * b.m21 + a.m33 * b.m31;
            c.m32 = a.m30 * b.m02 + a.m31 * b.m12 + a.m32 * b.m22 + a.m33 * b.m32;
            c.m33 = a.m30 * b.m03 + a.m31 * b.m13 + a.m32 * b.m23 + a.m33 * b.m33;

            return c;
        }

        public static Transform Reflection(Vector3D n)
        {
            n = n.Direction;

            Transform t = new Transform();
            t.m00 = 1 - 2 * n.X * n.X;
            t.m01 = -2 * n.X * n.Y;
            t.m02 = -2 * n.X * n.Z;
            t.m03 = 0;

            t.m10 = -2 * n.X * n.Y;
            t.m11 = 1 - 2 * n.Y * n.Y;
            t.m12 = -2 * n.Y * n.Z;
            t.m13 = 0;

            t.m20 = -2 * n.X * n.Z;
            t.m21 = -2 * n.Y * n.Z;
            t.m22 = 1 - 2 * n.Z * n.Z;
            t.m23 = 0;

            t.m30 = 0;
            t.m31 = 0;
            t.m32 = 0;
            t.m33 = 1;

            return t;

        }

        public static Transform Rotation(Vector3D axis, double angle)
        {
            axis = axis.Direction;
            var cc = 1 - Math.Cos(angle);
            var s = Math.Sin(angle);
            var c = Math.Cos(angle);

            Transform t = new Transform();
            t.m00 = axis.X * axis.X * cc + c;
            t.m01 = axis.Y * axis.X * cc - axis.Z * s;
            t.m02 = axis.Z * axis.X * cc + axis.Y * s;
            t.m03 = 0;

            t.m10 = axis.X * axis.Y * cc + axis.Z * s;
            t.m11 = axis.Y * axis.Y * cc + c;
            t.m12 = axis.Z * axis.Y * cc - axis.X * s;
            t.m13 = 0;

            t.m20 = axis.X * axis.Z * cc - axis.Y * s;
            t.m21 = axis.Y * axis.Z * cc + axis.X * s;
            t.m22 = axis.Z * axis.Z * cc + c;
            t.m23 = 0;

            t.m30 = 0;
            t.m31 = 0;
            t.m32 = 0;
            t.m33 = 1;

            return t;
        }

        public static Transform Scale(double size)
        {
            return Scale(size, size, size);
        }

        public static Transform Scale(double x, double y, double z)
        {
            Transform t = new Transform();
            t.m00 = x;
            t.m01 = 0;
            t.m02 = 0;
            t.m03 = 0;
            t.m10 = 0;
            t.m11 = y;
            t.m12 = 0;
            t.m13 = 0;
            t.m20 = 0;
            t.m21 = 0;
            t.m22 = z;
            t.m23 = 0;
            t.m30 = 0;
            t.m31 = 0;
            t.m32 = 0;
            t.m33 = 1;

            return t;
        }

        public static Transform ShearXY(double x, double y)
        {
            Transform t = new Transform();
            t.m00 = 1;
            t.m01 = 0;
            t.m02 = x;
            t.m03 = 0;
            t.m10 = 0;
            t.m11 = 1;
            t.m12 = y;
            t.m13 = 0;
            t.m20 = 0;
            t.m21 = 0;
            t.m22 = 1;
            t.m23 = 0;
            t.m30 = 0;
            t.m31 = 0;
            t.m32 = 0;
            t.m33 = 1;

            return t;
        }

        public static Transform ShearXZ(double x, double z)
        {
            Transform t = new Transform();
            t.m00 = 1;
            t.m01 = x;
            t.m02 = 0;
            t.m03 = 0;
            t.m10 = 0;
            t.m11 = 1;
            t.m12 = 0;
            t.m13 = 0;
            t.m20 = 0;
            t.m21 = z;
            t.m22 = 1;
            t.m23 = 0;
            t.m30 = 0;
            t.m31 = 0;
            t.m32 = 0;
            t.m33 = 1;

            return t;
        }

        public static Transform ShearYZ(double y, double z)
        {
            Transform t = new Transform();
            t.m00 = 1;
            t.m01 = 0;
            t.m02 = 0;
            t.m03 = 0;
            t.m10 = y;
            t.m11 = 1;
            t.m12 = 0;
            t.m13 = 0;
            t.m20 = z;
            t.m21 = 0;
            t.m22 = 1;
            t.m23 = 0;
            t.m30 = 0;
            t.m31 = 0;
            t.m32 = 0;
            t.m33 = 1;

            return t;
        }

        public static Transform Translation(Point3D point)
        {
            Transform t = new Transform();
            t.m00 = 1;
            t.m01 = 0;
            t.m02 = 0;
            t.m03 = point.X;
            t.m10 = 0;
            t.m11 = 1;
            t.m12 = 0;
            t.m13 = point.Y;
            t.m20 = 0;
            t.m21 = 0;
            t.m22 = 1;
            t.m23 = point.Z;
            t.m30 = 0;
            t.m31 = 0;
            t.m32 = 0;
            t.m33 = 1;

            return t;
        }
    }
}
