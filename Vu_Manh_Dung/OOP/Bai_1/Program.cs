using System;

namespace Bai_1
{
    public class Program
    {
        public static void Init(List<Person> list)
        {
            list.Add(new Person("P01","Nguyen Van A",21,20000000,0.05f));
            list.Add(new Person("P02", "Nguyen Van B", 22, 22000000, 0.05f));
            list.Add(new Person("P03", "Nguyen Van C", 23, 25000000, 0.05f));
        }
        public static void Output(List<Person> list)
        {
            foreach (Person p in list)
            {
                Console.WriteLine($"{p.Id}\t{p.Name}\t{p.Taxcoe}");
            }
        }
        public static void Main(string[] args)
        {
            List<Person> list = new List<Person>();
            Init(list);
            Output(list);
            
        }
        
    }
}
