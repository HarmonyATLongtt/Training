using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp9
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("*****Chuong trinh quan ly sinh vien*****");
            while (true)
            {
                Console.WriteLine("Tuy chon......");
                Console.WriteLine("1. Them sinh vien");
                Console.WriteLine("2. Cap nhap thong tin sinh vien boi ID");
                Console.WriteLine("3. Xoa sinh vien boi ID");
                Console.WriteLine("4. Tim kiem sinh vien theo ten");
                Console.WriteLine("5. Sap xep sinh vien theo diem trung binh (GPA)");
                Console.WriteLine("6. Sap xep sinh vien theo ten");
                Console.WriteLine("7. Sap xep sinh vien theo ID");
                Console.WriteLine("8. Hien thi danh sach sinh vien");
                Console.WriteLine("0. Thoat");
                Console.Write("Lua chon cua ban: ");
                int k = int.Parse(Console.ReadLine());
                switch (k)
                {
                    case 1:
                        Console.WriteLine("ban chon 1");
                        break;

                    case 2:
                        Console.WriteLine("ban chon 2");
                        break;

                    case 3:
                        Console.WriteLine("ban chon 3");
                        break;

                    case 4:
                        Console.WriteLine("ban chon 4");
                        break;

                    case 5:
                        Console.WriteLine("ban chon 5");
                        break;

                    case 6:
                        Console.WriteLine("ban chon 6");
                        break;

                    case 7:
                        Console.WriteLine("ban chon 7");
                        break;

                    case 8:
                        Console.WriteLine("ban chon 8");
                        break;

                    case 0:
                        Console.WriteLine("Ban da thoat chuong trinh");
                        goto thoat;
                        break;

                    default:
                        Console.WriteLine("Khong co chu nang nay");
                        Console.WriteLine("Hay chon lai.....");
                        break;
                }
            }
        thoat:
            Console.ReadKey();
        }
    }
}