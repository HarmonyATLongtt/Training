using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp8
{
    public class Calculator
    {
        // public delegate void CalculationComplete(int result);

        public void Add(int x, int y) // , CalculationComplete callback
        {
            int result = x + y;
            //callback(result);
        }
    }

    public class Program
    {
        private static void Main(string[] args)
        {
            Calculator calc = new Calculator();
            calc.Add(3, 5; =>
            {
                Console.WriteLine("Result: " + result);
            });
        }
    }
}