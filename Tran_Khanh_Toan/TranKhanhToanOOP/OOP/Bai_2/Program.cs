using System;

namespace Bai_2 // Note: actual namespace depends on the project name.
{
    public class Program
    {
        public static List<Person> Init(TaxData taxData)
        {
            
            return new List<Person>
            {
                new Person(1, "Tran Khanh Toan", 21, 9000000, taxData),
                new Person(2, "Tran Khanh Toan1", 22, 18000000, taxData),
                new Person(3, "Tran Khanh Toan2", 23, 25000000, taxData)
            };
        }

        public static void Output(List<Person> people)
        {
            foreach(var person in people)
            {
                Console.WriteLine(person.Name + " taxCoe: " + person.GetTax());
            }
        }

        static void Main(string[] args)
        {
            TaxData taxData = new TaxData();
            List<Person> people = Init(taxData);
            Output(people);
        }
    }
}