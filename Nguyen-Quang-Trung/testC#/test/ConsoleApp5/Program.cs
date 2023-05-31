using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp5
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;
            int a, b, c, broken;
            Console.WriteLine("Mời nhập 3 số.....");
            Console.WriteLine("Số thứ 1: ");
            a = int.Parse(Console.ReadLine());
            Console.WriteLine("Số thứ 2: ");
            b = int.Parse(Console.ReadLine());
            Console.WriteLine("Số thứ 3: ");
            c = int.Parse(Console.ReadLine());
            if (a > b)
            {
                broken = a;
            }
            else
            {
                broken = b;
            }
            if (broken > c)
            {
                Console.WriteLine("Số max là {0}", broken);
            }
            else
            {
                Console.WriteLine("Số max là {0}", c);
            }
            Console.ReadKey();
        }
    }
}