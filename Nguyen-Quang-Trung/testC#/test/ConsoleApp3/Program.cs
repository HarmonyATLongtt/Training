using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            Console.InputEncoding = Encoding.Unicode;
            int value1, value2;
            Console.WriteLine("2 số bạn muốn nhập vào để tính toán là.....");
        Nhap1:
            Console.Write("Số thứ nhất: ");
            if (int.TryParse(Console.ReadLine(), out value1) == true)
            {
                goto Nhap2;
            }
            else
            {
                Console.WriteLine("Mời nhập lại số ");
                goto Nhap1;
            }
        Nhap2:
            Console.Write("Số thứ hai: ");
            if (int.TryParse(Console.ReadLine(), out value2) == true)
            {
                goto Tinhtoan;
            }
            else
            {
                Console.WriteLine("Mời nhập lại số: ");
                goto Nhap2;
            }
        Tinhtoan:
            Console.WriteLine("Các biểu thức toán học là.....");
            tong(value1, value2);
            hieu(value1, value2);
            tich(value1, value2);
            thuong(value1, value2);
            sodu(value1, value2);
            gia_tri_nguyen_của_thuong(value1, value2);

            Console.ReadKey();
        }

        private static void tong(int a, int b)
        {
            Console.WriteLine("Tổng của 2 số là: {0}", a + b);
        }

        private static void hieu(int a, int b)
        {
            Console.WriteLine("Hiệu của 2 số là: {0}", a - b);
        }

        private static void tich(int a, int b)
        {
            Console.WriteLine("Tích của 2 số là: {0}", a * b);
        }

        private static void thuong(int a, int b)
        {
            Console.WriteLine("Thương của 2 số là: {0}", a * 0.1 / b);
        }

        private static void sodu(int a, int b)
        {
            Console.WriteLine("Số dư của 2 số là: {0}", a % b);
        }

        private static void gia_tri_nguyen_của_thuong(int a, int b)
        {
            Console.WriteLine("Giá trị nguyên của thương là: {0}", a / b);
        }
    }
}