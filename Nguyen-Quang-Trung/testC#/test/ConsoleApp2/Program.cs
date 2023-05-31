using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppConsole2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string ten, diachi;
            int tuoi;
            Console.OutputEncoding = Encoding.Unicode;
            Console.InputEncoding = Encoding.Unicode;
            Console.Write("Tên: ");
            ten = Console.ReadLine();
            Console.Write("Tuổi: ");
            tuoi = int.Parse(Console.ReadLine());
            Console.Write("Địa chỉ: ");
            diachi = Console.ReadLine();
            Console.WriteLine("--------------------------");
            Console.WriteLine("Tên của bạn là: {0}, tuổi của bạn là: {1}, địa chỉ của bạn là: {2}", ten, tuoi, diachi);

            Console.WriteLine("**************************");
            string value1, value2, value3;
            Console.WriteLine("Nhập vào 3 số bạn muốn:");
            value1 = Console.ReadLine();
            value2 = Console.ReadLine();
            value3 = Console.ReadLine();
            Console.WriteLine("--------------------------");
            Console.WriteLine(string.Format("Các số là {0}, {1}, {2}", value1, value2, value3));
            Console.ReadKey();
        }
    }
}