using Bai_1.Ex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_1
{
    internal class MainEx
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("********************** BT OPP **********************");
            while (true)
            {
                Console.WriteLine("Ban muon chon khoi chay bai tap nao?");
                Console.WriteLine("1. Bai 1");
                Console.WriteLine("2. Bai 2");
                Console.WriteLine("3. Bai 3");
                Console.WriteLine("0. Thoat");
                Console.Write("Lua chon cua ban la: ");
                int luachon = int.Parse(Console.ReadLine());
                switch (luachon)
                {
                    case 0:
                        Console.WriteLine("Chuong trinh se thoat.....");
                        return;

                    case 1:
                        Console.WriteLine("Ban da chon bai 1.....");
                        Ex_1.Execution();
                        break;

                    case 2:
                        Console.WriteLine("Ban da chon bai 2.....");
                        Ex_2.Execution();
                        break;

                    case 3:
                        Console.WriteLine("Ban da chon bai 3.....");
                        Ex_3.Execution();
                        break;
                }
            }
            Console.ReadKey();
        }
    }
}