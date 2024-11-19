using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai3_OOP
{
    public class Person: IPerson
    {
        protected string _id;
        protected string _name;
        protected int _age;
        public Person()
        {

        }
        public Person(string id, string name, int age)
        {
            _id = id;
            _name = name;
            _age = age;
        }
        protected static string GetInput(string tmp)
        {
            string input;
            Console.Write(tmp);
            do
            {
                input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("INVALID");
                }
                else
                    break;
            } while (true);
            return input;
        }
        public virtual void Init()
        {
            _id = GetInput("ID: ");
            _name = GetInput("Full name: ");
            do
            {
                string tmp = GetInput("Age: ");
                if (int.TryParse(tmp, out int age) && age > 0)
                {
                    _age = age;
                    break;
                }
                else
                {
                    Console.WriteLine("INVALID");
                }
            }while (true);
        }
        public virtual string GetInfo()
        {
            return $"ID: {_id}, Name: {_name}, Age: {_age}";
        }
    }
}
