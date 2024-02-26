using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = BaseObjects.Console;

namespace WireFrameModels3._0
{
    public abstract class ProjectBase
    {
        DateTime startTime;

        public void Run()
        {
            Console.WriteLine($"## Run project {GetType().Name}. #####", ConsoleColor.Green);

            startTime = DateTime.Now;
            try
            {
                RunProject();

                Console.WriteLine($"## Total Elapsed time: {(DateTime.Now - startTime).TotalSeconds.ToString("#,##0.00")} seconds. #####", ConsoleColor.Green);
                Console.WriteLine("## Done  #####");
            }
            catch (Exception e)
            {
                Console.WriteLine($"## {e.Message} #####");
                Console.WriteLine(e.StackTrace);
                Console.WriteLine($"## Total Elapsed time: {(DateTime.Now - startTime).TotalSeconds.ToString("#,##0.00")} seconds. #####", ConsoleColor.Red);
                Console.WriteLine("## Failed  #####");
            }

            Console.WriteLine();
        }

        protected void Run(Action run)
        {
            try
            {
                run();
            }
            catch (Exception e)
            {
                Console.WriteLine($"## {e.Message} #####", ConsoleColor.Red);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("## Failed  #####");
            }
            Console.WriteLine();
        }

        protected abstract void RunProject();
    }
}
