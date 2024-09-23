using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai2_OOP
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Write("Nhập số lượng người: ");
            int a=int.Parse(Console.ReadLine());
            Console.WriteLine();
            for(int i = 0; i < a; i++)
            {
                string id=Console.ReadLine();
                string name=Console.ReadLine();
                int age=int.Parse(Console.ReadLine());
                double income=  int.Parse(Console.ReadLine());
                Init(id, name, age, income);
            }
            Output();
            Console.ReadKey();
        }
        static List<Person> peopleList = new List<Person>();
        static void Init(string id, string name, int age, double income)
        {
            TaxData taxData = new TaxData();
            peopleList.Add(new Person(id, name, age, income, taxData));
        }
        static void Output()
        {
            foreach (Person person in peopleList)
            {
                Console.WriteLine($"ID: {person.Id}, Name: {person.Name}, Tax: {person.GetTax()}");
            }
        }
    }
}
