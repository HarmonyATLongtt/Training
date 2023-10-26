using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai2 
{
    class Program
    {
        public static List<Person> DS = new List<Person>();
        static void Main(string[] args)
        {
            Init();
            Output();
        }
        public static void Init()
        {
            TaxData t = new TaxData();
            Person p = new Person(1, "duc", 17, 100, t);
            Person p1 = new Person(2, "duc1", 18, 10000000, t);
            Person p2 = new Person(3, "duc2", 19, 17000000, t);
            DS.Add(p);
            DS.Add(p1);
            DS.Add(p2);
        }
        public static void Output()
        {
            Console.WriteLine("{0,-10}{1,-20}{2,-10}{3,-10}", "Id", "Name", "TaxCoe", "Tax");
            foreach (var p in DS)
            {
                Console.WriteLine("{0,-10}{1,-20}{2,-10}{3,-10}", p.Id, p.Name, p.TaxCoe, p.GetTax());
            }
        }
    }
}