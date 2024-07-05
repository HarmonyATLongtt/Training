using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Bai1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            List<Person> lst = Init();
            
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "infor1.txt");

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;          
                while ((line = reader.ReadLine()) != null)
                {
                    try {
                        Person person = new Person();
                        string[] data = line.Trim().Split(new char[] { ';' });
                        person.Id = Convert.ToInt32(data[0].Trim());
                        person.Name = data[1].Trim();
                        person.Age = Convert.ToInt32(data[2].Trim());
                        person.Income = Convert.ToDouble(data[3].Trim());
                        person.Taxcoe = Convert.ToDouble(data[4].Trim());
                        lst.Add(person);
                    } catch (Exception ex) { }
                    
                }
            }
            foreach (Person p in lst) {
                Output(p);
                Console.WriteLine("----------------");
            }

            Person other = new Person(4, "Trần Huy Văn", 40, 4000, 0.3);
            bool check = false;
            foreach (Person p in lst)
            {
               if(p.Equals(other))
                {
                    Console.WriteLine($"Người này đã tồn tại trong danh là:");
                    Output(p);
                    check = true;
                    break;
                }                
            }
            if (!check)
            {
                Console.WriteLine("Người này không tồn tại trong danh");
            }
            Console.ReadKey();
        }

        public static List<Person> Init() { return new List<Person>(); }
        public static void Output(Person p)
        {
            Console.WriteLine($"ID: {p.Id}");
            Console.WriteLine($"Tên: {p.Name}");          
            Console.WriteLine($"Tax: {p.GetTax()}");
        }
    }
}
