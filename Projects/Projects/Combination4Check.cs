using BasicObjects.MathExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireFrameModels3._0;

namespace Projects.Projects
{
    public class Combination4Check : ProjectBase
    {
        protected override void RunProject()
        {
            var key = new Combination4(4, 3, 2, 1);
            Console.WriteLine(key);
            key = new Combination4(4, 3, 1, 2);
            Console.WriteLine(key);
            key = new Combination4(4, 2, 3, 1);
            Console.WriteLine(key);
            key = new Combination4(4, 2, 1, 3);
            Console.WriteLine(key);
            key = new Combination4(4, 1, 2, 3);
            Console.WriteLine(key);
            key = new Combination4(4, 1, 3, 2);
            Console.WriteLine(key);

            key = new Combination4(3, 4, 2, 1);
            Console.WriteLine(key);
            key = new Combination4(3, 4, 1, 2);
            Console.WriteLine(key);
            key = new Combination4(3, 2, 4, 1);
            Console.WriteLine(key);
            key = new Combination4(3, 2, 1, 4);
            Console.WriteLine(key);
            key = new Combination4(3, 1, 2, 4);
            Console.WriteLine(key);
            key = new Combination4(3, 1, 4, 2);
            Console.WriteLine(key);

            key = new Combination4(2, 3, 4, 1);
            Console.WriteLine(key);
            key = new Combination4(2, 3, 1, 4);
            Console.WriteLine(key);
            key = new Combination4(2, 4, 3, 1);
            Console.WriteLine(key);
            key = new Combination4(2, 4, 1, 3);
            Console.WriteLine(key);
            key = new Combination4(2, 1, 4, 3);
            Console.WriteLine(key);
            key = new Combination4(2, 1, 3, 4);
            Console.WriteLine(key);

            key = new Combination4(1, 3, 2, 4);
            Console.WriteLine(key);
            key = new Combination4(1, 3, 4, 2);
            Console.WriteLine(key);
            key = new Combination4(1, 2, 3, 4);
            Console.WriteLine(key);
            key = new Combination4(1, 2, 4, 3);
            Console.WriteLine(key);
            key = new Combination4(1, 4, 2, 3);
            Console.WriteLine(key);
            key = new Combination4(1, 4, 3, 2);
            Console.WriteLine(key);
        }
    }
}
