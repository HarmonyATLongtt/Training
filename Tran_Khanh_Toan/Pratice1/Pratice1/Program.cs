//xóa các thư viện không dùng đến
namespace Pratice1
{
    internal class Program
    {
        public static bool canWin(int[] game, int leap, int i)
        {
            if (i < 0 || game[i] == 1) return false;
            if (i + leap >= game.Length - 1) return true; //điều kiện nên ngắn gọn dễ hiểu ,tránh làm rối logic
            game[i] = 1;
            return canWin(game, leap, i + leap) || canWin(game, leap, i + 1) || canWin(game, leap, i - 1);
        }

        private static void Main(string[] args)
        {
            string[] lines = File.ReadAllLines("../input.txt");
            int q = int.Parse(lines[0]);
            int currentLine = 1;
            string output = "";
            for (int j = 0; j < q; j++)
            {
                string[] arrLeap = lines[currentLine++].Split(" ");
                int leap = int.Parse(arrLeap[1]);
                int lg = int.Parse(arrLeap[0]);
                string[] gameStrings = lines[currentLine++].Split(" ");
                int[] game = Array.ConvertAll(gameStrings, int.Parse);

                bool result = canWin(game, 0, 0);
                output += result ? "YES\n" : "NO\n";
            }
            File.WriteAllLines("../output.txt", new string[] { output });
        }
    }
}