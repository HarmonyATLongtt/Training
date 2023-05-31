using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;
            int a, b;
        Nhap:
            Console.WriteLine("Nhập số a, b lần lượt là: ");
            if (int.TryParse(Console.ReadLine(), out a) == true && int.TryParse(Console.ReadLine(), out b) == true)
            {
                Console.WriteLine("Phương trình bậc nhất là {0}x + {1} = 0", a, b);
            }
            else
            {
                Console.WriteLine("Mời nhập lại số.....");
                goto Nhap;
            }

        Luachon:
            Console.WriteLine("Hay chọn 1 hoặc 2 để xem lời giải bằng if-else hoặc switch...");
            int value = int.Parse(Console.ReadLine());
            if (value == 1)
            {
                goto Cach1;
            }
            else if (value == 2)
            {
                goto Cach2;
            }
            else
            {
                Console.WriteLine("Bạn đang gõ sai, hãy gõ 1 hoặc 2.....");
                goto Luachon;
            }

        // giải bằng if - else
        Cach1:
            if (a == 0)
            {
                if (b == 0)
                {
                    Console.WriteLine("Phương trình có vô số nghiệm");
                }
                else
                {
                    Console.WriteLine("Phương trình vô nghiệm");
                }
            }
            else
            {
                Console.WriteLine("Phương trình có nghiệm là {0}", -b * 0.1 / a);
            }
            goto End;

        // giải bằng swtich
        Cach2:
            switch (a == 0 && b == 0)
            {
                case true:
                    Console.WriteLine("Phương trình có vô số nghiệm");
                    break;
            }
            switch (a == 0 && b != 0)
            {
                case true:
                    Console.WriteLine("Phương trình vô nghiệm");
                    break;
            }
            switch (a != 0)
            {
                case true:
                    Console.WriteLine("Phương trình có nghiệm là {0}", -b * 0.1 / a);
                    break;
            }
        End:
            Console.ReadKey();
        }
    }
}