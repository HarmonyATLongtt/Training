using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;



namespace Vi_Du_OOP
{
    class Animal
    {
        public double weight;
        public double height;

        public void showInfo()
        {
            Console.WriteLine("Height: " + height + " weight: " + weight);
        }
    }

    class Program
    {
        
        static void Main(string[] args)
        {
            Animal Dog = new Animal();
            Dog.weight = 15;
            Dog.height = 30;

            Animal Cat = new Animal();
            Cat.weight = 10;
            Cat.height = 20;


            Dog.showInfo();
            Cat.showInfo();

            Console.ReadKey();

        }
    }
}
