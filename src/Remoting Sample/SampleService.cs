using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class SampleService : SampleInterface
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

        public void LogOnServer()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Write on server...");
        }
    }
}
