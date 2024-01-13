using System;
using System.IO;

namespace Pratice1
{
    internal class Program
    {
        public bool canWin(int[] game, int leap, int i)
        {
            if (i < 0 || game[i] == 1) return false;
            if (i + leap >= game.Length || i == game.Length - 1) return true;
            game[i] = 1;
            return canWin(game, leap, i + leap) || canWin(game, leap, i + 1) || canWin(game, leap, i - 1);
        }
        static void Main(string[] args)
        {
            Program program = new Program();
            string[] lines = File.ReadAllLines("../input.txt");
            int q = int.Parse(lines[0]);
            int currentLine = 1;
            string output = "";
            for (int j = 0; j < q; j++)
            {
                string[] arrLeap = lines[currentLine].Split(" ");
                int leap = int.Parse(arrLeap[1]);
                int lg = int.Parse(arrLeap[0]);
                currentLine++;
                string[] gameStrings = lines[currentLine].Split(" ");
                int[] game = Array.ConvertAll(gameStrings, int.Parse);
                currentLine++;

                bool result = program.canWin(game, leap, 0);
                output += result ? "YES\n" : "NO\n";
            }
                File.WriteAllLines("../output.txt", new string[] { output });
        }
    }
}