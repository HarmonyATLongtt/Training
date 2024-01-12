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

        private static bool CanWin(int[] game, int n, int leap, int i, bool[] visited)
        {
            if (i < 0 || game[i] == 1 || visited[i]) return false;
            if (i >= n || i + leap >= n) return true;
            visited[i] = true;
            return CanWin(game, n, leap, i + 1, visited) || CanWin(game, n, leap, i + leap, visited) || CanWin(game, n, leap, i - 1, visited);
        }

        private static void Array_Game(string inputFile, string outputFile)
        {
            int q = 0, n = 0, leap = 0;
            int[] game = null;
            List<string> result = new List<string>();
            try
            {
                using (StreamReader reader = new StreamReader(inputFile))
                {
                    q = int.Parse(reader.ReadLine());
                    for (int line = 0; line < q; line++)
                    {
                        string[] part = reader.ReadLine().Split(' ');
                        n = int.Parse(part[0]);
                        leap = int.Parse(part[1]);

                        string[] arrGame = reader.ReadLine().Split(' ');
                        game = new int[n];
                        for (int j = 0; j < n; j++)
                        {
                            game[j] = int.Parse(arrGame[j]);
                        }
                        if (ValidCheck(q, n, leap, game))
                        {
                            bool[] visited = new bool[n];
                            if (CanWin(game, n, leap, 0, visited))
                                result.Add("YES");
                            else result.Add("NO");
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