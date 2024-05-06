using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp8
{
    internal class Animal
    {
        public string name;
        public int height;
        public int weight;

        public virtual void Sound()
        {
            Console.WriteLine("Animal keu...");
        }
    }
}