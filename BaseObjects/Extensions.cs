using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseObjects
{
    public static class Extensions
    {
        public static string Repeat(this string s, int times, string separator = "")
        {
            return string.Join(separator, Enumerable.Repeat(s, times));
        }
    }
}
