using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HasTableCoBan
{
    class Program
    {
        static void Main(string[] args)
        {
            Hashtable MyHash = new Hashtable();
            MyHash.Add("K", "Kteam");
            MyHash.Add("H", "HowKteam");
            MyHash.Add("FE", "Free Education");

            Console.WriteLine(MyHash["FE"]);

            Console.WriteLine("\nCount: " + MyHash.Count);

            foreach (DictionaryEntry item in MyHash)
            {
                Console.WriteLine(item.Key + "\t" + item.Value);
            }

            MyHash["TG"] = "TruongGiang";
            MyHash["LG"] = "LinhGiang";

            Console.WriteLine("\nCount: " + MyHash.Count);

            foreach (DictionaryEntry item in MyHash)
            {
                Console.WriteLine(item.Key + "\t" + item.Value);
            }

            Console.ReadLine();
        }
    }
}
