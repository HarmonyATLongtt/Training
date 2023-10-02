using OOP.Core;
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
                    IProgram exercise = null;
                    Console.WriteLine();
                    switch (choose)
                    {
                        case 1:
                            exercise = new Bai_1();
                            break;

                        case 2:
                            exercise = new Bai_2();
                            break;

                        case 3:
                            exercise = new Bai_3();
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
                    if (exercise != null)
                    {
                        exercise.Init();
                        exercise.Output();
                        Console.WriteLine();
                        Console.Write("Enter any key to continue...");
                        Console.ReadKey();
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