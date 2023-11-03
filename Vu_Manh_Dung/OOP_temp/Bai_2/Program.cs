using System;

namespace Bai_2
{
    public class Program
    {

        static void Init(List<Person> list, TaxData tax)
        {
            //hardcode
            list.Add(new Person("P01","Nguyen Van A",15,10000000,tax));
            list.Add(new Person("P02", "Nguyen Van B", 18, 10000000, tax));
            list.Add(new Person("P03", "Nguyen Van C", 20, 20000000, tax));

            //softcode
            Console.Write("Number of Person: ");
            int n = int.Parse(Console.ReadLine());
            for(int i=0; i<n; i++)
            {
                Console.Write("Add person "+ (i+1));
                Person p = new Person();
                p.Init();
                list.Add(p);
            }
        }

        static void Output(List<Person> list)
        {
            Person.Title();
            foreach(Person p in list)
            {
                p.Output();
            }
        }
        public static void Main(string[] args) 
        { 
            List<Person> list = new List<Person>();
            TaxData tax = new TaxData();
            Init(list, tax);
            Output(list);
        }
    }
}