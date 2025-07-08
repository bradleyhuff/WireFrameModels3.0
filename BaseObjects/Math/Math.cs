using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicObjects.Math
{
    public static class Math
    {
        public static double Square(double x)
        {
            return x * x;
        }

        public static double Cube(double x)
        {
            return x * x * x;
        }

        public static double Min(params double[] list)
        {
            double minValue = double.MaxValue;
            foreach (double element in list)
            {
                minValue = System.Math.Min(minValue, element);
            }
            return minValue;
        }

        public static IEnumerable<T> Min<T>(Func<T, double> compare, params T[] list)
        {
            double minValue = double.MaxValue;
            foreach (T element in list)
            {
                var newValue = compare(element);
                if (newValue < minValue)
                {
                    minValue = newValue;
                }
            }
            foreach (T element in list.Where(e => compare(e) == minValue))
            {
                yield return element;
            }
        }

        public static double Max(params double[] list)
        {
            double maxValue = double.MinValue;
            foreach (double element in list)
            {
                maxValue = System.Math.Max(maxValue, element);
            }
            return maxValue;
        }

        public static IEnumerable<T> Max<T>(Func<T, double> compare, params T[] list)
        {
            double maxValue = double.MinValue;
            foreach (T element in list)
            {
                var newValue = compare(element);
                if (newValue > maxValue)
                {
                    maxValue = newValue;
                }
            }
            foreach (T element in list.Where(e => compare(e) == maxValue))
            {
                yield return element;
            }
        }

        public static int Min(params int[] list)
        {
            int minValue = int.MaxValue;
            foreach (int element in list)
            {
                minValue = System.Math.Min(minValue, element);
            }
            return minValue;
        }

        public static int Max(params int[] list)
        {
            int maxValue = int.MinValue;
            foreach (int element in list)
            {
                maxValue = System.Math.Max(maxValue, element);
            }
            return maxValue;
        }

        public static double[] Abs(params double[] list)
        {
            double[] output = new double[list.Length];
            for (int i = 0; i < list.Length; i++)
            {
                output[i] = System.Math.Abs(list[i]);
            }
            return output;
        }

        public static int[] Abs(params int[] list)
        {
            int[] output = new int[list.Length];
            for (int i = 0; i < list.Length; i++)
            {
                output[i] = System.Math.Abs(list[i]);
            }
            return output;
        }

        public static double ConvertToDegrees(this double angle)
        {
            return angle * 180.0 / System.Math.PI;
        }
    }
}
