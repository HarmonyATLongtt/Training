using System;

namespace Bai_1
{
    public class Program
    {
        public static void Init(List<Person> list)
        {
            //hard code
            list.Add(new Person("P01","Nguyen Van A",21,20000000,0.05f));
            list.Add(new Person("P02", "Nguyen Van B", 22, 22000000, 0.05f));
            list.Add(new Person("P03", "Nguyen Van C", 23, 25000000, 0.05f));

            Console.Write("Number of person: ");
            int n = int.Parse(Console.ReadLine());
            for(int i = 0; i < n; i++)
            {
                Console.WriteLine("Add person "+ (i+1));
                Person p = new Person();
                p.Init();
                list.Add(p);
            }

        }
        public static void Output(List<Person> list)
        {
            Person.Title();
            foreach (Person p in list)
            {
                p.Output();
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
