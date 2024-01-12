using System.IO;


bool canWin(int index, int leap, int[] game)
{
   if (index < 0 || game[index] == 1)
    {
        return false;
    }
    else if (index == game.Length - 1 || index + leap >= game.Length)
    {
        return true;
    }

    game[index] = 1;

    return canWin(index + 1, leap, game)
        || canWin(index - 1, leap, game)
        || canWin(index + leap, leap, game);
}


void main()
{
    string inputFilePath = "../../../input.txt";
    string outputFilePath = "../../../output.txt";

    if (!File.Exists(inputFilePath))
    {
        Console.WriteLine("Input file does not exist.");
        return;
    }

    FileStream fs = new FileStream(inputFilePath, FileMode.Open);

    StreamReader sr = new StreamReader(fs);

    List<string> lines = new List<string>();
    string line = "";
    while ((line = sr.ReadLine()) != null)
    {
        lines.Add(line);
    }

    int n = int.Parse(lines[0]);
    int[] N = new int[lines.Count / 2];
    int[] leaps = new int[lines.Count / 2];
    List<int[]> arr = new List<int[]>();

    for (int i = 1; i < lines.Count; i += 2)
    {
        int[] array1 = lines[i].Split(" ").Select(int.Parse).ToArray();
        int[] array2 = lines[i + 1].Split(" ").Select(int.Parse).ToArray();
        N[i / 2] = array1[0];
        leaps[i / 2] = array1[1];
        int[] res = new int[N[i / 2]];
        for (int j = 0; j < array2.Length; j++)
        {
            res[j] = array2[j];
        }
        arr.Add(res);
    }

    File.Delete(outputFilePath);
    for (int i = 0; i < n; i++)
    {
        using (StreamWriter sw = new StreamWriter(outputFilePath, true))
        {
            if (canWin(0,leaps[i], arr[i]))
            {
                Console.WriteLine("YES");
                sw.WriteLine("YES");
            }
            else
            {
                Console.WriteLine("NO");
                sw.WriteLine("NO");
            }
            
        }
    }
    return;
}

main();