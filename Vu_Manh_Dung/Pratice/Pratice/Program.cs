using System;
using System.Collections.Generic;
using System.IO;

namespace Pratice
{
    public class Program
    {
        private static bool ValidCheck(int q, int n, int leap, int[] game)
        {
            if (!(q >= 1 && q <= 5000)) return false;
            if (!(n >= 1 && n <= 100)) return false;
            if (!(leap >= 0 && leap <= 100)) return false;
            if (game[0] != 0) return false;
            return true;
        }

        private static bool CanWin(int[] game, int n, int leap, int i)
        {
            if (i < 0 || game[i] == 1) return false;
            if (i >= n || i + leap >= n) return true;
            game[i] = 1;
            return CanWin(game, n, leap, i + 1) || CanWin(game, n, leap, i + leap) || CanWin(game, n, leap, i - 1);
        }

        private static void Array_Game(string inputFile, string outputFile)
        {
            List<string> result = new List<string>();
            try
            {
                using (StreamReader reader = new StreamReader(inputFile))
                {
                    int q = int.Parse(reader.ReadLine());
                    for (int line = 0; line < q; line++)
                    {
                        string[] part = reader.ReadLine().Split(' ');
                        int n = int.Parse(part[0]);
                        int leap = int.Parse(part[1]);

                        string[] arrGame = reader.ReadLine().Split(' ');
                        int[] game = new int[n];
                        for (int j = 0; j < n; j++)
                        {
                            game[j] = int.Parse(arrGame[j]);
                        }
                        if (ValidCheck(q, n, leap, game))
                        {
                            if (CanWin(game, n, leap, 0))
                                result.Add("YES");
                            else
                                result.Add("NO");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(outputFile))
                {
                    foreach (string item in result)
                        writer.WriteLine(item);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void Main(string[] args)
        {
            string inputFile = "input.txt";
            string outputFile = "output.txt";
            Array_Game(inputFile, outputFile);
        }
    }
}