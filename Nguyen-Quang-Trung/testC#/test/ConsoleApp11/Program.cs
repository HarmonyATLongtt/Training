using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ConsoleApp11
{
    internal class Program
    {
        private static void Main()
        {
            //using (FileStream file = new FileStream("vidu1.txt", FileMode.Create, FileAccess.Write))
            //{
            //    BinaryWriter bw = new BinaryWriter(file);
            //    bw.Write(123);
            //    bw.Flush();
            //    StreamWriter sw = new StreamWriter(file);
            //    //sw.WriteLine("\n");
            //    sw.WriteLine("abc hello");
            //    sw.WriteLine("123456789");
            //    sw.WriteLine("nguyen van a");
            //    sw.Flush();
            //}
            //using (FileStream file = new FileStream("vidu1.txt", FileMode.Open, FileAccess.Read))
            //{
            //    BinaryReader br = new BinaryReader(file);
            //    var nd1 = br.ReadInt16();
            //    StreamReader sr = new StreamReader(file);
            //    var nd2 = sr.ReadToEnd();
            //    Console.WriteLine(nd1);
            //    Console.WriteLine(nd2);
            //}
            //StreamReader sreader = new StreamReader(file);
            //string nd = sreader.ReadToEnd();
            //Console.WriteLine("" + nd);
            //BinaryReader breader = new BinaryReader(file);
            //var nd = breader.ReadInt32();
            //Console.WriteLine(nd);

            //file.Close();

            //string duongdan = "vidu2.txt";
            //string noidung = "xin chao 123456789 qưertyuiopsdfghjklxcvbnm";
            //File.WriteAllText(duongdan, noidung);
            //Console.ReadKey();

            //string duongdan = "vidu3.txt";
            //string[] nd = new string[] { "aaa", "bbb", "ccc" };
            //File.WriteAllLines(duongdan, nd);
            //string[] ndmoi = new string[] { "1113333", "66666666", "1211111" };
            //File.AppendAllLines(duongdan, ndmoi);
        }
    }
}