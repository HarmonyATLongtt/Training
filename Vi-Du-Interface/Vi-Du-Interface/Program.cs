using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vi_Du_Interface
{
    interface Ispeak
    {
        void speak();
    }

    class Animal : Ispeak
    {
        public void speak()
        {
            Console.WriteLine("Animal is speaking. . .");
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            Animal animal = new Animal();

            animal.speak();

            Console.ReadKey();
        }
    }
}
