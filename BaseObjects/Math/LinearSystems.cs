
namespace BasicObjects.Math
{
    internal static class LinearSystems
    {
        /// | m0  m1  m2 ||r0||v0|<br/>
        /// | m3  m4  m5 ||r1||v1|<br/>
        /// | m6  m7  m8 ||r2||v2|<br/>
        internal static void Solve3x3(double m0, double m1, double m2, double m3, double m4, double m5, double m6, double m7, double m8, double v0, double v1, double v2, out double r0, out double r1, out double r2)
        {
            var index = MaxIndex(m0, m1, m2, m3, m4, m5, m6, m7, m8);

            switch (index)
            {
                case 0: Solve3x3Pivot(m0, m1, m2, m3, m4, m5, m6, m7, m8, v0, v1, v2, out r0, out r1, out r2); return;
                case 1: Solve3x3Pivot(m1, m0, m2, m4, m3, m5, m7, m6, m8, v0, v1, v2, out r1, out r0, out r2); return;
                case 2: Solve3x3Pivot(m2, m1, m0, m5, m4, m3, m8, m7, m6, v0, v1, v2, out r2, out r1, out r0); return;

                case 3: Solve3x3Pivot(m3, m4, m5, m0, m1, m2, m6, m7, m8, v1, v0, v2, out r0, out r1, out r2); return;
                case 4: Solve3x3Pivot(m4, m3, m5, m1, m0, m2, m7, m6, m8, v1, v0, v2, out r1, out r0, out r2); return;
                case 5: Solve3x3Pivot(m5, m4, m3, m2, m1, m0, m8, m7, m6, v1, v0, v2, out r2, out r1, out r0); return;

                case 6: Solve3x3Pivot(m6, m7, m8, m0, m1, m2, m3, m4, m5, v2, v0, v1, out r0, out r1, out r2); return;
                case 7: Solve3x3Pivot(m7, m6, m8, m1, m0, m2, m4, m3, m5, v2, v0, v1, out r1, out r0, out r2); return;
                default: Solve3x3Pivot(m8, m7, m6, m2, m1, m0, m5, m4, m3, v2, v0, v1, out r2, out r1, out r0); return;
            }
        }

        private static int MaxIndex(double m0, double m1, double m2, double m3, double m4, double m5, double m6, double m7, double m8)
        {
            var maxValue = System.Math.Abs(m0);
            var index = 0;

            if (System.Math.Abs(m1) > maxValue)
            {
                maxValue = System.Math.Abs(m1);
                index = 1;
            }

            if (System.Math.Abs(m2) > maxValue)
            {
                maxValue = System.Math.Abs(m2);
                index = 2;
            }

            if (System.Math.Abs(m3) > maxValue)
            {
                maxValue = System.Math.Abs(m3);
                index = 3;
            }

            if (System.Math.Abs(m4) > maxValue)
            {
                maxValue = System.Math.Abs(m4);
                index = 4;
            }

            if (System.Math.Abs(m5) > maxValue)
            {
                maxValue = System.Math.Abs(m5);
                index = 5;
            }

            if (System.Math.Abs(m6) > maxValue)
            {
                maxValue = System.Math.Abs(m6);
                index = 6;
            }

            if (System.Math.Abs(m7) > maxValue)
            {
                maxValue = System.Math.Abs(m7);
                index = 7;
            }

            if (System.Math.Abs(m8) > maxValue)
            {
                index = 8;
            }
            return index;
        }

        private static void Solve3x3Pivot(double m0, double m1, double m2, double m3, double m4, double m5, double m6, double m7, double m8, double v0, double v1, double v2, out double r0, out double r1, out double r2)
        {
            double m2_0 = m1 * m3 - m0 * m4;
            double m2_1 = m2 * m3 - m0 * m5;
            double m2_2 = m1 * m6 - m0 * m7;
            double m2_3 = m2 * m6 - m0 * m8;

            double v2_0 = v0 * m3 - v1 * m0;
            double v2_1 = v0 * m6 - v2 * m0;

            Solve2x2(m2_0, m2_1, m2_2, m2_3, v2_0, v2_1, out r1, out r2);
            r0 = (v0 - m1 * r1 - m2 * r2) / m0;
        }

        private static int MaxIndex(double m0, double m1, double m2, double m3)
        {
            var maxValue = System.Math.Abs(m0);
            var index = 0;

            if (System.Math.Abs(m1) > maxValue)
            {
                maxValue = System.Math.Abs(m1);
                index = 1;
            }

            if (System.Math.Abs(m2) > maxValue)
            {
                maxValue = System.Math.Abs(m2);
                index = 2;
            }

            if (System.Math.Abs(m3) > maxValue)
            {
                index = 3;
            }
            return index;
        }

        /// | m0  m1 ||r0||v0|<br/>
        /// | m2  m3 ||r1||v1|<br/>

        internal static void Solve2x2(double m0, double m1, double m2, double m3, double v0, double v1, out double r0, out double r1)
        {
            var index = MaxIndex(m0, m1, m2, m3);

            switch (index)
            {
                case 0: Solve2x2Pivot(m0, m1, m2, m3, v0, v1, out r0, out r1); return;
                case 1: Solve2x2Pivot(m1, m0, m3, m2, v0, v1, out r1, out r0); return;
                case 2: Solve2x2Pivot(m2, m3, m0, m1, v1, v0, out r0, out r1); return;
                default: Solve2x2Pivot(m3, m2, m1, m0, v1, v0, out r1, out r0); return;
            }
        }

        private static void Solve2x2Pivot(double m0, double m1, double m2, double m3, double v0, double v1, out double r0, out double r1)
        {
            r1 = (v0 * m2 - v1 * m0) / (m2 * m1 - m0 * m3);
            r0 = (v0 - m1 * r1) / m0;
        }
    }
}
