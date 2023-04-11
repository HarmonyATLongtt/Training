using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vi_Du_For
{
    class Program
    {
        static void Main(string[] args)
        {
            int N = 10;
            int M = 20;

            char drawChar = '*';
            char insideChar = '.';

            // Vẽ từ trên xuống
            for (int i = 0; i < N; i++)
            {
                // Vẽ từ trái sang
                for (int j = 0; j < M; j++)
                {
                    if (i % (N - 1) == 0 || ((i % (N - 1) != 0) && (j % (M - 1) == 0)))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(drawChar);    
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write(insideChar);
                    }
                }
                //mỗi lần vẽ xong một hàng thì xuống dòng
                Console.WriteLine();
            }
            Console.ReadKey();
        }
    }
}
