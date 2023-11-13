using Microsoft.Extensions.Primitives;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    class TaxData
    {
        public int Age { get; set; }
        public float Income { get; set; }

        public float GetTaxCoe()
        {
            if (Age < 18)
            {
                return 0;
            }
            else
            {
                switch (Income)
                {
                    case (<= 9000000):
                        return 0.05F;
                    case (> 9000000 and <= 15000000):
                        return 0.10F;
                    case (> 15000000 and <= 20000000):
                        return 0.15F;
                    case (> 20000000 and <= 30000000):
                        return 0.20F;
                    default: return 0.50F;
                }

            }
        }
    }

    interface IPerson
    {
        bool Equals(object x);
        float GetTax();
        void GetInfo();
    }

  

    class Student: IPerson
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public float Income { get; set; }

        public float Tax_coe { get; set; }

        public string Class { get; set; }
        public string School { get; set; }
        public Student(string id, string name, string age, string income, string @class, string school, TaxData mytax)
        {
            Id = id;
            Name = name;
            Age = Convert.ToInt32(age);
            Income = float.Parse(income);
            mytax.Age = Convert.ToInt32(age);
            mytax.Income = float.Parse(income);
            Tax_coe = mytax.GetTaxCoe();
            Class = @class;
            School = school;
        }

        public bool Equals(Student s)
        {
            if (s.Id == Id)
            {
                Console.WriteLine("Already available with {0}", s.Name);
                return false;
            }
            else
            {
                return true;
            }

        }
        public float GetTax()
        {
            float tax = Tax_coe * Income;
            return tax;
        }
        
        public void GetInfo()
        {
            Console.WriteLine("-----------------------");
            Console.WriteLine("Id: {0}",Id);
            Console.WriteLine("Name: {0}", Name);
            Console.WriteLine("School: {0}", School);
            Console.WriteLine("Class: {0}", Class);

        }
    }
    class Teacher : IPerson
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public float Income { get; set; }

        public float Tax_coe { get; set; }
        public float Tax { get; set; }
        public string School { get; set; }

        public Teacher(string id, string name, string age, string income, string school, TaxData mytax) 
        {
            Id = id;
            Name = name;
            Age = Convert.ToInt32 (age);
            Income = float.Parse(income);
            mytax.Age = Convert.ToInt32 (age);
            mytax.Income= float.Parse(income);
            Tax_coe = mytax.GetTaxCoe();
            Tax = GetTax();
            School = school;
        }
        public bool Equals(Teacher t)
        {
            if (t.Id == Id)
            {
                Console.WriteLine("Already available with {0}", t.Name);
                return false;
            }
            else
            {
                return true;
            }

        }
        public float GetTax()
        {
            float tax = Tax_coe * Income;
            return tax;
        }
        public void GetInfo()
        {
            Console.WriteLine("-----------------------");
            Console.WriteLine("Id: {0}", Id);
            Console.WriteLine("Name: {0}", Name);
            Console.WriteLine("School: {0}", School);
            Console.WriteLine("Income: {0}", Income);
            Console.WriteLine("Tax: {0}", Tax);
        }
    }
    class Employee : IPerson
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public float Income { get; set; }

        public float Tax_coe { get; set; }
        public float Tax { get; set; }
        public string Jobtitle { get; set; }
        public string Company { get; set; }
        

        public Employee(string id, string name, string age, string income, string jobtitle, string company, TaxData mytax )
        {
            Id = id;
            Name = name;
            Age = Convert.ToInt32(age);
            Income = float.Parse( income );
            mytax.Age = Age;
            mytax.Income= Income;
            Tax_coe = mytax.GetTaxCoe();
            Tax = GetTax();
            Jobtitle = jobtitle;
            Company = company;
        }


        public bool Equals(Employee e)
        {
            if (e.Id == Id)
            {
                Console.WriteLine("Already available with {0}", e.Name);
                return false;
            }
            else
            {
                return true;
            }

        }
        public float GetTax()
        {
            float tax = Tax_coe * Income;
            return tax;
        }
        
        public void GetInfo()
        {
            Console.WriteLine("-----------------------");
            Console.WriteLine("Id: {0}", Id);
            Console.WriteLine("Name: {0}", Name);
            Console.WriteLine("Company: {0}", Company);
            Console.WriteLine("Job Title: {0}", Jobtitle);
            Console.WriteLine("Income: {0}", Income);
            Console.WriteLine("Tax: {0}", Tax);
        }
    }
}
