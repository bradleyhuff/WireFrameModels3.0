using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireFrameModels3._0;
using Console = BaseObjects.Console;

namespace Projects.Projects
{
    public class ShellTest : ProjectBase
    {
        protected override void RunProject()
        {
            Console.WriteLine("This is a shell test", ConsoleColor.Blue);

            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    Console.Write($"{i} ", (ConsoleColor)j);
                }
                Console.WriteLine();
            }
        }
    }
}
