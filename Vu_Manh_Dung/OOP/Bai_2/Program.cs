using System;

namespace Bai_2
{
    public class Program
    {
        public static void Main(string[] args) 
        { 
            List<Person> list = new List<Person>();
            TaxData tax = new TaxData();
            list.Add(new Person("P01","Nguyen Van A",15,10000000,tax));
            list.Add(new Person("P02", "Nguyen Van B", 18, 10000000, tax));
            list.Add(new Person("P03", "Nguyen Van C", 20, 20000000, tax));
            foreach (Person p in list)
            {
                Console.WriteLine($"{p.Id}\t{p.Name}\t{p.Taxcoe}");
            }
        }
    }
}