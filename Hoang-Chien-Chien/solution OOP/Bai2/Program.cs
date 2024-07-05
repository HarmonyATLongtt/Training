using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai2
{

    internal class Program
    {

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            List<Person> lst = Init();
            //lst.Add(new Person(1, "A", 20, 10000000));
            //lst.Add(new Person(2, "B", 23, 20000000));
            //lst.Add(new Person(3, "C", 25, 15000000));
            //lst.Add(new Person(4, "D", 16, 1000000));
            //lst.Add(new Person(5, "E", 18, 9000000));
            //lst.Add(new Person(6, "F", 25, 25000000));

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "infor2.txt");

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        Person person = new Person();
                        string[] data = line.Trim().Split(new char[] { ';' });
                        person.Id = Convert.ToInt32(data[0].Trim());
                        person.Name = data[1].Trim();
                        person.Age = Convert.ToInt32(data[2].Trim());
                        person.Income = Convert.ToDouble(data[3].Trim());
                        person.Taxcoe = Convert.ToDouble(TaxData.GetTaxCoe(person.Age, person.Taxcoe));
                        lst.Add(person);
                    }
                    catch (Exception ex) { }

                }
            }
            foreach (Person p in lst)
            {
                Output(p);
                Console.WriteLine("-------------------");
            }
            
            Console.ReadKey();
        }

        public static List<Person> Init() { return new List<Person>(); }
        public static void Output(Person p)
        {
            CultureInfo cultureInfo = new CultureInfo("vi-VN");
            Console.WriteLine($"ID: {p.Id}");
            Console.WriteLine($"Tên: {p.Name}");           
            Console.WriteLine("Tax: " + string.Format(cultureInfo, "{0:C}", p.GetTax()));
        }
    }
}
