
using Double = BasicObjects.Math.Double;

namespace BasicObjects.Transformations
{
    internal static class Matricies
    {
        /// <summary>
        /// | m00  m01 |<br/>
        /// | m10  m11 |<br/>
        /// </summary>
        /// <param name="m00"></param>
        /// <param name="m01"></param>
        /// <param name="m10"></param>
        /// <param name="m11"></param>
        /// <returns></returns>
        internal static double Determinant(double m00, double m01, double m10, double m11)
        {
            return m00 * m11 - m10 * m01;
        }
        /// <summary>
        /// | m00  m01 m02 |<br/>
        /// | m10  m11 m12 |<br/>
        /// | m20  m21 m22 |<br/>
        /// </summary>
        /// <param name="m00"></param>
        /// <param name="m01"></param>
        /// <param name="m02"></param>
        /// <param name="m10"></param>
        /// <param name="m11"></param>
        /// <param name="m12"></param>
        /// <param name="m20"></param>
        /// <param name="m21"></param>
        /// <param name="m22"></param>
        /// <returns></returns>
        internal static double Determinant(double m00, double m01, double m02, double m10, double m11, double m12, double m20, double m21, double m22)
        {
            return m00 * Determinant(m11, m12, m21, m22) - m01 * Determinant(m10, m12, m20, m22) + m02 * Determinant(m10, m11, m20, m21);
        }

        internal static double[] Multiply4X4(double[] a, double[] b)
        {
            double[] dst = new double[16];

            dst[0] = b[0] * a[0] + b[1] * a[4] + b[2] * a[8] + b[3] * a[12];
            dst[1] = b[0] * a[1] + b[1] * a[5] + b[2] * a[9] + b[3] * a[13];
            dst[2] = b[0] * a[2] + b[1] * a[6] + b[2] * a[10] + b[3] * a[14];
            dst[3] = b[0] * a[3] + b[1] * a[7] + b[2] * a[11] + b[3] * a[15];
            dst[4] = b[4] * a[0] + b[5] * a[4] + b[6] * a[8] + b[7] * a[12];
            dst[5] = b[4] * a[1] + b[5] * a[5] + b[6] * a[9] + b[7] * a[13];
            dst[6] = b[4] * a[2] + b[5] * a[6] + b[6] * a[10] + b[7] * a[14];
            dst[7] = b[4] * a[3] + b[5] * a[7] + b[6] * a[11] + b[7] * a[15];
            dst[8] = b[8] * a[0] + b[9] * a[4] + b[10] * a[8] + b[11] * a[12];
            dst[9] = b[8] * a[1] + b[9] * a[5] + b[10] * a[9] + b[11] * a[13];
            dst[10] = b[8] * a[2] + b[9] * a[6] + b[10] * a[10] + b[11] * a[14];
            dst[11] = b[8] * a[3] + b[9] * a[7] + b[10] * a[11] + b[11] * a[15];
            dst[12] = b[12] * a[0] + b[13] * a[4] + b[14] * a[8] + b[15] * a[12];
            dst[13] = b[12] * a[1] + b[13] * a[5] + b[14] * a[9] + b[15] * a[13];
            dst[14] = b[12] * a[2] + b[13] * a[6] + b[14] * a[10] + b[15] * a[14];
            dst[15] = b[12] * a[3] + b[13] * a[7] + b[14] * a[11] + b[15] * a[15];
            return dst;
        }

        internal static void Normalize3D(double v0, double v1, double v2, out double n0, out double n1, out double n2)
        {
            double length = (double)System.Math.Sqrt(v0 * v0 + v1 * v1 + v2 * v2);
            // make sure we don't divide by 0.
            if (length > Double.DifferenceError)
            {
                n0 = v0 / length;
                n1 = v1 / length;
                n2 = v2 / length;
            }
            else
            {
                n0 = double.NaN;
                n1 = double.NaN;
                n2 = double.NaN;
            }
        }

        internal static void Cross3D(double a0, double a1, double a2, double b0, double b1, double b2, out double c0, out double c1, out double c2)
        {
            c0 = a1 * b2 - a2 * b1;
            c1 = a2 * b0 - a0 * b2;
            c2 = a0 * b1 - a1 * b0;
        }

        internal static double Dot3D(double a0, double a1, double a2, double b0, double b1, double b2)
        {
            return a0 * b0 + a1 * b1 + a2 * b2;
        }

        internal static double[] Identity4X4()
        {
            double[] dst = new double[16];

            dst[0] = 1;
            dst[1] = 0;
            dst[2] = 0;
            dst[3] = 0;
            dst[4] = 0;
            dst[5] = 1;
            dst[6] = 0;
            dst[7] = 0;
            dst[8] = 0;
            dst[9] = 0;
            dst[10] = 1;
            dst[11] = 0;
            dst[12] = 0;
            dst[13] = 0;
            dst[14] = 0;
            dst[15] = 1;

            return dst;
        }

        internal static double[] Transpose4X4(double[] m)
        {
            double[] dst = new double[16];

            dst[0] = m[0];
            dst[1] = m[4];
            dst[2] = m[8];
            dst[3] = m[12];
            dst[4] = m[1];
            dst[5] = m[5];
            dst[6] = m[9];
            dst[7] = m[13];
            dst[8] = m[2];
            dst[9] = m[6];
            dst[10] = m[10];
            dst[11] = m[14];
            dst[12] = m[3];
            dst[13] = m[7];
            dst[14] = m[11];
            dst[15] = m[15];

            return dst;
        }

        internal static double[] Perspective4X4(double fieldOfViewInRadians, double aspect, double near, double far)
        {
            double[] dst = new double[16];
            double f = (double)System.Math.Tan(System.Math.PI * 0.5 - 0.5 * fieldOfViewInRadians);
            double rangeInv = (double)(1.0 / (near - far));

            dst[0] = f / aspect;
            dst[1] = 0;
            dst[2] = 0;
            dst[3] = 0;
            dst[4] = 0;
            dst[5] = f;
            dst[6] = 0;
            dst[7] = 0;
            dst[8] = 0;
            dst[9] = 0;
            dst[10] = (near + far) * rangeInv;
            dst[11] = -1;
            dst[12] = 0;
            dst[13] = 0;
            dst[14] = near * far * rangeInv * 2;
            dst[15] = 0;

            return dst;
        }

        internal static double[] Orthographic4X4(double left, double right, double bottom, double top, double near, double far)
        {
            double[] dst = new double[16];

            dst[0] = 2 / (right - left);
            dst[1] = 0;
            dst[2] = 0;
            dst[3] = 0;
            dst[4] = 0;
            dst[5] = 2 / (top - bottom);
            dst[6] = 0;
            dst[7] = 0;
            dst[8] = 0;
            dst[9] = 0;
            dst[10] = 2 / (near - far);
            dst[11] = 0;
            dst[12] = (left + right) / (left - right);
            dst[13] = (bottom + top) / (bottom - top);
            dst[14] = (near + far) / (near - far);
            dst[15] = 1;

            return dst;
        }

        internal static double[] Frustum4X4(double left, double right, double bottom, double top, double near, double far)
        {
            double dx = right - left;
            double dy = top - bottom;
            double dz = far - near;

            double[] dst = new double[16];

            dst[0] = 2 * near / dx;
            dst[1] = 0;
            dst[2] = 0;
            dst[3] = 0;
            dst[4] = 0;
            dst[5] = 2 * near / dy;
            dst[6] = 0;
            dst[7] = 0;
            dst[8] = (left + right) / dx;
            dst[9] = (top + bottom) / dy;
            dst[10] = -(far + near) / dz;
            dst[11] = -1;
            dst[12] = 0;
            dst[13] = 0;
            dst[14] = -2 * near * far / dz;
            dst[15] = 0;

            return dst;
        }

        internal static double[] Translation4X4(double tx, double ty, double tz)
        {
            double[] dst = new double[16];

            dst[0] = 1;
            dst[1] = 0;
            dst[2] = 0;
            dst[3] = 0;
            dst[4] = 0;
            dst[5] = 1;
            dst[6] = 0;
            dst[7] = 0;
            dst[8] = 0;
            dst[9] = 0;
            dst[10] = 1;
            dst[11] = 0;
            dst[12] = tx;
            dst[13] = ty;
            dst[14] = tz;
            dst[15] = 1;

            return dst;
        }

        internal static double[] Translate4X4(double[] m, double tx, double ty, double tz)
        {
            // This is the optimized version of
            // return multiply(m, translation(tx, ty, tz), dst);
            double[] dst = new double[16];

            dst[0] = m[0];
            dst[1] = m[1];
            dst[2] = m[2];
            dst[3] = m[3];
            dst[4] = m[4];
            dst[5] = m[5];
            dst[6] = m[6];
            dst[7] = m[7];
            dst[8] = m[8];
            dst[9] = m[9];
            dst[10] = m[10];
            dst[11] = m[11];

            dst[12] = m[0] * tx + m[4] * ty + m[8] * tz + m[12];
            dst[13] = m[1] * tx + m[5] * ty + m[9] * tz + m[13];
            dst[14] = m[2] * tx + m[6] * ty + m[10] * tz + m[14];
            dst[15] = m[3] * tx + m[7] * ty + m[11] * tz + m[15];

            return dst;
        }

        internal static void AxisRotation3x3(double x, double y, double z, double angle,
            out double m00, out double m01, out double m02, out double m10, out double m11, out double m12, out double m20, out double m21, out double m22)
        {
            double n = (double)System.Math.Sqrt(x * x + y * y + z * z);
            x /= n;
            y /= n;
            z /= n;

            double xx = x * x;
            double yy = y * y;
            double zz = z * z;
            double c = (double)System.Math.Cos(angle);
            double s = (double)System.Math.Sin(angle);
            double oneMinusCosine = 1 - c;

            m00 = xx + (1 - xx) * c;
            m01 = x * y * oneMinusCosine + z * s;
            m02 = x * z * oneMinusCosine - y * s;

            m10 = x * y * oneMinusCosine - z * s;
            m11 = yy + (1 - yy) * c;
            m12 = y * z * oneMinusCosine + x * s;

            m20 = x * z * oneMinusCosine + y * s;
            m21 = y * z * oneMinusCosine - x * s;
            m22 = zz + (1 - zz) * c;
        }

        internal static double[] AxisRotation(double[] axis, double angleInRadians)
        {
            return AxisRotationOfCosine(axis, System.Math.Cos(angleInRadians));
        }

        internal static double[] AxisRotationOfCosine(double[] axis, double angleCosine)
        {
            double[] dst = new double[16];

            double x = axis[0];
            double y = axis[1];
            double z = axis[2];
            double n = System.Math.Sqrt(x * x + y * y + z * z);
            x /= n;
            y /= n;
            z /= n;
            double xx = x * x;
            double yy = y * y;
            double zz = z * z;
            double c = angleCosine;
            double s = System.Math.Sqrt(1 - c * c);
            double oneMinusCosine = 1 - c;

            dst[0] = xx + (1 - xx) * c;
            dst[1] = x * y * oneMinusCosine + z * s;
            dst[2] = x * z * oneMinusCosine - y * s;
            dst[3] = 0;
            dst[4] = x * y * oneMinusCosine - z * s;
            dst[5] = yy + (1 - yy) * c;
            dst[6] = y * z * oneMinusCosine + x * s;
            dst[7] = 0;
            dst[8] = x * z * oneMinusCosine + y * s;
            dst[9] = y * z * oneMinusCosine - x * s;
            dst[10] = zz + (1 - zz) * c;
            dst[11] = 0;
            dst[12] = 0;
            dst[13] = 0;
            dst[14] = 0;
            dst[15] = 1;

            return dst;
        }

        internal static double[] TransformVector(double[] m, double[] v)
        {
            double[] dst = new double[4];
            for (int i = 0; i < 4; ++i)
            {
                dst[i] = 0;
                for (int j = 0; j < 4; ++j)
                {
                    dst[i] += v[j] * m[j * 4 + i];
                }
            }
            return dst;
        }

        internal static double[] AxisRotation4X4(double[] axis, double angleInRadians)
        {
            double[] dst = new double[16];

            double x = axis[0];
            double y = axis[1];
            double z = axis[2];
            double n = System.Math.Sqrt(x * x + y * y + z * z);
            x /= n;
            y /= n;
            z /= n;
            double xx = x * x;
            double yy = y * y;
            double zz = z * z;
            double c = System.Math.Cos(angleInRadians);
            double s = System.Math.Sin(angleInRadians);
            double oneMinusCosine = 1 - c;

            dst[0] = xx + (1 - xx) * c;
            dst[1] = x * y * oneMinusCosine + z * s;
            dst[2] = x * z * oneMinusCosine - y * s;
            dst[3] = 0;
            dst[4] = x * y * oneMinusCosine - z * s;
            dst[5] = yy + (1 - yy) * c;
            dst[6] = y * z * oneMinusCosine + x * s;
            dst[7] = 0;
            dst[8] = x * z * oneMinusCosine + y * s;
            dst[9] = y * z * oneMinusCosine - x * s;
            dst[10] = zz + (1 - zz) * c;
            dst[11] = 0;
            dst[12] = 0;
            dst[13] = 0;
            dst[14] = 0;
            dst[15] = 1;

            return dst;
        }

        internal static double[] AxisRotate4X4(double[] m, double[] axis, double angleInRadians)
        {
            // This is the optimized verison of
            // return multiply(m, axisRotation(axis, angleInRadians), dst);
            double[] dst = new double[16];

            double x = axis[0];
            double y = axis[1];
            double z = axis[2];
            double n = System.Math.Sqrt(x * x + y * y + z * z);
            x /= n;
            y /= n;
            z /= n;
            double xx = x * x;
            double yy = y * y;
            double zz = z * z;
            double c = System.Math.Cos(angleInRadians);
            double s = System.Math.Sin(angleInRadians);
            double oneMinusCosine = 1 - c;

            double r00 = xx + (1 - xx) * c;
            double r01 = x * y * oneMinusCosine + z * s;
            double r02 = x * z * oneMinusCosine - y * s;
            double r10 = x * y * oneMinusCosine - z * s;
            double r11 = yy + (1 - yy) * c;
            double r12 = y * z * oneMinusCosine + x * s;
            double r20 = x * z * oneMinusCosine + y * s;
            double r21 = y * z * oneMinusCosine - x * s;
            double r22 = zz + (1 - zz) * c;

            dst[0] = r00 * m[0] + r01 * m[4] + r02 * m[8];
            dst[1] = r00 * m[1] + r01 * m[5] + r02 * m[9];
            dst[2] = r00 * m[2] + r01 * m[6] + r02 * m[10];
            dst[3] = r00 * m[3] + r01 * m[7] + r02 * m[11];
            dst[4] = r10 * m[0] + r11 * m[4] + r12 * m[8];
            dst[5] = r10 * m[1] + r11 * m[5] + r12 * m[9];
            dst[6] = r10 * m[2] + r11 * m[6] + r12 * m[10];
            dst[7] = r10 * m[3] + r11 * m[7] + r12 * m[11];
            dst[8] = r20 * m[0] + r21 * m[4] + r22 * m[8];
            dst[9] = r20 * m[1] + r21 * m[5] + r22 * m[9];
            dst[10] = r20 * m[2] + r21 * m[6] + r22 * m[10];
            dst[11] = r20 * m[3] + r21 * m[7] + r22 * m[11];

            dst[12] = m[12];
            dst[13] = m[13];
            dst[14] = m[14];
            dst[15] = m[15];

            return dst;
        }

        internal static double[] Scaling4X4(double sx, double sy, double sz)
        {
            double[] dst = new double[16];

            dst[0] = sx;
            dst[1] = 0;
            dst[2] = 0;
            dst[3] = 0;
            dst[4] = 0;
            dst[5] = sy;
            dst[6] = 0;
            dst[7] = 0;
            dst[8] = 0;
            dst[9] = 0;
            dst[10] = sz;
            dst[11] = 0;
            dst[12] = 0;
            dst[13] = 0;
            dst[14] = 0;
            dst[15] = 1;

            return dst;
        }

        internal static double[] Scale4X4(double[] m, double sx, double sy, double sz)
        {
            // This is the optimized verison of
            // return multiply(m, scaling(sx, sy, sz), dst);
            double[] dst = new double[16];

            dst[0] = sx * m[0];
            dst[1] = sx * m[1];
            dst[2] = sx * m[2];
            dst[3] = sx * m[3];
            dst[4] = sy * m[4];
            dst[5] = sy * m[5];
            dst[6] = sy * m[6];
            dst[7] = sy * m[7];
            dst[8] = sz * m[8];
            dst[9] = sz * m[9];
            dst[10] = sz * m[10];
            dst[11] = sz * m[11];

            dst[12] = m[12];
            dst[13] = m[13];
            dst[14] = m[14];
            dst[15] = m[15];

            return dst;
        }

        internal static double[] Inverse4X4(double[] m)
        {
            double[] dst = new double[16];

            double tmp_0 = m[10] * m[15];
            double tmp_1 = m[14] * m[11];
            double tmp_2 = m[6] * m[15];
            double tmp_3 = m[14] * m[7];
            double tmp_4 = m[6] * m[11];
            double tmp_5 = m[10] * m[7];
            double tmp_6 = m[2] * m[15];
            double tmp_7 = m[14] * m[3];
            double tmp_8 = m[2] * m[11];
            double tmp_9 = m[10] * m[3];
            double tmp_10 = m[2] * m[7];
            double tmp_11 = m[6] * m[3];
            double tmp_12 = m[8] * m[13];
            double tmp_13 = m[12] * m[9];
            double tmp_14 = m[4] * m[13];
            double tmp_15 = m[12] * m[5];
            double tmp_16 = m[4] * m[9];
            double tmp_17 = m[8] * m[5];
            double tmp_18 = m[0] * m[13];
            double tmp_19 = m[12] * m[1];
            double tmp_20 = m[0] * m[9];
            double tmp_21 = m[8] * m[1];
            double tmp_22 = m[0] * m[5];
            double tmp_23 = m[4] * m[1];

            double t0 = tmp_0 * m[5] + tmp_3 * m[9] + tmp_4 * m[13] -
                    (tmp_1 * m[5] + tmp_2 * m[9] + tmp_5 * m[13]);
            double t1 = tmp_1 * m[1] + tmp_6 * m[9] + tmp_9 * m[13] -
                    (tmp_0 * m[1] + tmp_7 * m[9] + tmp_8 * m[13]);
            double t2 = tmp_2 * m[1] + tmp_7 * m[5] + tmp_10 * m[13] -
                    (tmp_3 * m[1] + tmp_6 * m[5] + tmp_11 * m[13]);
            double t3 = tmp_5 * m[1] + tmp_8 * m[5] + tmp_11 * m[9] -
                    (tmp_4 * m[1] + tmp_9 * m[5] + tmp_10 * m[9]);

            double d = (double)(1.0 / (m[0] * t0 + m[4] * t1 + m[8] * t2 + m[12] * t3));

            dst[0] = d * t0;
            dst[1] = d * t1;
            dst[2] = d * t2;
            dst[3] = d * t3;
            dst[4] = d * (tmp_1 * m[4] + tmp_2 * m[8] + tmp_5 * m[12] -
                    (tmp_0 * m[4] + tmp_3 * m[8] + tmp_4 * m[12]));
            dst[5] = d * (tmp_0 * m[0] + tmp_7 * m[8] + tmp_8 * m[12] -
                    (tmp_1 * m[0] + tmp_6 * m[8] + tmp_9 * m[12]));
            dst[6] = d * (tmp_3 * m[0] + tmp_6 * m[4] + tmp_11 * m[12] -
                    (tmp_2 * m[0] + tmp_7 * m[4] + tmp_10 * m[12]));
            dst[7] = d * (tmp_4 * m[0] + tmp_9 * m[4] + tmp_10 * m[8] -
                    (tmp_5 * m[0] + tmp_8 * m[4] + tmp_11 * m[8]));
            dst[8] = d * (tmp_12 * m[7] + tmp_15 * m[11] + tmp_16 * m[15] -
                    (tmp_13 * m[7] + tmp_14 * m[11] + tmp_17 * m[15]));
            dst[9] = d * (tmp_13 * m[3] + tmp_18 * m[11] + tmp_21 * m[15] -
                    (tmp_12 * m[3] + tmp_19 * m[11] + tmp_20 * m[15]));
            dst[10] = d * (tmp_14 * m[3] + tmp_19 * m[7] + tmp_22 * m[15] -
                    (tmp_15 * m[3] + tmp_18 * m[7] + tmp_23 * m[15]));
            dst[11] = d * (tmp_17 * m[3] + tmp_20 * m[7] + tmp_23 * m[11] -
                    (tmp_16 * m[3] + tmp_21 * m[7] + tmp_22 * m[11]));
            dst[12] = d * (tmp_14 * m[10] + tmp_17 * m[14] + tmp_13 * m[6] -
                    (tmp_16 * m[14] + tmp_12 * m[6] + tmp_15 * m[10]));
            dst[13] = d * (tmp_20 * m[14] + tmp_12 * m[2] + tmp_19 * m[10] -
                    (tmp_18 * m[10] + tmp_21 * m[14] + tmp_13 * m[2]));
            dst[14] = d * (tmp_18 * m[6] + tmp_23 * m[14] + tmp_15 * m[2] -
                    (tmp_22 * m[14] + tmp_14 * m[2] + tmp_19 * m[6]));
            dst[15] = d * (tmp_22 * m[10] + tmp_16 * m[2] + tmp_21 * m[6] -
                    (tmp_20 * m[6] + tmp_23 * m[10] + tmp_17 * m[2]));

            return dst;
        }

        internal static void TransformVector3x3(
            double m00, double m01, double m02, double m10, double m11, double m12, double m20, double m21, double m22,
            double x, double y, double z, out double tx, out double ty, out double tz
            )
        {
            tx = m00 * x + m01 * y + m02 * z;
            ty = m10 * x + m11 * y + m12 * z;
            tz = m20 * x + m21 * y + m22 * z;
        }

        internal static double[] TransformPoint4X4(double[] m, double[] v)
        {
            double[] dst = new double[4];

            double d = v[0] * m[3] + v[1] * m[7] + v[2] * m[11] + m[15];

            dst[0] = (v[0] * m[0] + v[1] * m[4] + v[2] * m[8] + m[12]) / d;
            dst[1] = (v[0] * m[1] + v[1] * m[5] + v[2] * m[9] + m[13]) / d;
            dst[2] = (v[0] * m[2] + v[1] * m[6] + v[2] * m[10] + m[14]) / d;

            return dst;
        }

        internal static double[] TransformDirection4X4(double[] m, double[] v)
        {
            double[] dst = new double[3];

            dst[0] = v[0] * m[0] + v[1] * m[4] + v[2] * m[8];
            dst[1] = v[0] * m[1] + v[1] * m[5] + v[2] * m[9];
            dst[2] = v[0] * m[2] + v[1] * m[6] + v[2] * m[10];

            return dst;
        }

        internal static double[] TransformNormalWithInverse4X4(double[] mi, double[] v)
        {
            double[] dst = new double[3];

            dst[0] = v[0] * mi[0] + v[1] * mi[1] + v[2] * mi[2];
            dst[1] = v[0] * mi[4] + v[1] * mi[5] + v[2] * mi[6];
            dst[2] = v[0] * mi[8] + v[1] * mi[9] + v[2] * mi[10];

            return dst;
        }

        internal static double[] TransformNormal4X4(double[] m, double[] v)
        {
            double[] dst = new double[3];
            double[] mi = Inverse4X4(m);

            dst[0] = v[0] * mi[0] + v[1] * mi[1] + v[2] * mi[2];
            dst[1] = v[0] * mi[4] + v[1] * mi[5] + v[2] * mi[6];
            dst[2] = v[0] * mi[8] + v[1] * mi[9] + v[2] * mi[10];

            return dst;
        }

        internal static double[] Copy4X4(double[] src)
        {
            double[] dst = new double[16];

            dst[0] = src[0];
            dst[1] = src[1];
            dst[2] = src[2];
            dst[3] = src[3];
            dst[4] = src[4];
            dst[5] = src[5];
            dst[6] = src[6];
            dst[7] = src[7];
            dst[8] = src[8];
            dst[9] = src[9];
            dst[10] = src[10];
            dst[11] = src[11];
            dst[12] = src[12];
            dst[13] = src[13];
            dst[14] = src[14];
            dst[15] = src[15];

            return dst;
        }

        internal static double DotProduct(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                sum += a[i] * b[i];
            }

            return sum;
        }

        internal static double[] Subtract(double[] a, double[] b)
        {
            double[] output = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                output[i] = a[i] - b[i];
            }
            return output;
        }

        internal static double[] Add(double[] a, double[] b)
        {
            double[] output = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                output[i] = a[i] + b[i];
            }
            return output;
        }

        internal static double[] ScalarMultiply(double scalar, double[] a)
        {
            double[] output = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                output[i] = scalar * a[i];
            }
            return output;
        }

        internal static double[] VectorMultiply(double[] vector, double[][] matrix)
        {
            double[] output = new double[vector.Length];

            for (int i = 0; i < vector.Length; i++)
            {
                output[i] = DotProduct(vector, matrix[i]);
            }

            return output;
        }
    }
}
