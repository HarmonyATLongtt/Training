using OOP.Programs;
using System;

namespace OOP.Views
{
    public class MainView
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("======== Menu ========");
                Console.WriteLine("1. Bai 1");
                Console.WriteLine("2. Bai 2");
                Console.WriteLine("3. Bai 3");
                Console.WriteLine("0. Exit");

                int choose;
                Console.Write("Enter your choose: ");

                if (int.TryParse(Console.ReadLine(), out choose))
                {
                    Console.WriteLine();
                    switch (choose)
                    {
                        case 1:
                            Bai_1 b1 = new Bai_1();
                            b1.Init();
                            b1.Output();
                            Console.WriteLine();
                            Console.Write("Enter any key to continue...");
                            Console.ReadKey();
                            break;

                        case 2:
                            Bai_2 b2 = new Bai_2();
                            b2.Init();
                            b2.Output();
                            Console.WriteLine();
                            Console.Write("Enter any key to continue...");
                            Console.ReadKey();
                            break;

                        case 3:
                            Bai_3 b3 = new Bai_3();
                            b3.Init();
                            b3.Output();
                            Console.WriteLine();
                            Console.Write("Enter any key to continue...");
                            Console.ReadKey();
                            break;

                        case 0:
                            return;

                        default:
                            Console.WriteLine("Your choose invalid!");
                            Console.WriteLine();
                            Console.Write("Enter any key to continue...");
                            Console.ReadKey();
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("\nYour choose invalid!");
                    Console.WriteLine();
                    Console.Write("Enter any key to continue...");
                    Console.ReadKey();
                }
            }
        }
    }
}