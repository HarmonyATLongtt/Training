using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp7
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;
            Console.Title = "Trò chơi KÉO BÚA BAO";
            Console.WriteLine("Chào mừng bạn đến với trò chơi");
            Console.WriteLine("***Luật chơi như sau KEO = 1, BUA = 2, BAO = 3; KEO < BUA, BUA < BAO, KEO > BAO");
            Console.WriteLine("Bạn có 3 lượt chơi");
        sansang:
            Console.WriteLine("Bạn đã sẵn sàng chưa y/n");
            string input1 = Console.ReadLine();
            if (input1 == "y")
            {
                goto choi;
            }
            else if (input1 == "n")
            {
                goto sansang;
            }
            else
            {
                Console.WriteLine("Sai cú pháp mời bạn nhập lại");
                goto sansang;
            }
        choi:
            Console.WriteLine("Ready......");
            int count1 = 0;
            int count2 = 0;
            int count3 = 0;
            for (; count1 < 3; count1++)
            {
                Console.WriteLine("Lượt thứ {0}", count1 + 1);
                Console.Write("Lựa chọn của bạn là  ");
                int nguoichon = int.Parse(Console.ReadLine());
                Random rd = new Random();
                int maychon = rd.Next(1, 4);
                Console.WriteLine("Lựa chọn của máy là {0}", maychon);
                int broken = nguoichon - maychon;
                if (broken == 1 || broken == 2 || broken == -2)
                {
                    Console.WriteLine("Bạn đã chiến thắng lượt {0}", count1 + 1);
                    count2 += 1;
                }
                else if (broken == 0)
                {
                    Console.WriteLine("Hòa!!!");
                }
                else
                {
                    Console.WriteLine("Bạn đã thua lượt {0}, hãy cố gắng hơn.", count1 + 1);
                    count3 += 1;
                }
            }
            if (count2 > count3)
            {
                Console.WriteLine("Chung cuộc bạn đã thắng");
            }
            else if (count2 == count3)
            {
                Console.WriteLine("Chung cuộc bạn đã thua");
            }
            else
            {
                Console.WriteLine("Hòa!!!");
            }
            Console.WriteLine("Trò chơi kết thúc!!!");

            Console.ReadKey();
        }
    }
}