using NguyenVanViet.Core;

namespace NguyenVanViet.Models
{
    public class Student : Person, IPerson
    {
        public string Class { get; set; }
        public string School { get; set; }

        public Student(int id, string name, int age, float income, string clas, string school, TaxData taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            Class = clas;
            School = school;
            TaxCoe = taxData.GetTaxCoe(age, income);
        }

        public void GetInfo()
        {
            Console.WriteLine($"ID: {Id}, Name: {Name}, Age: {Age}, School: {School} , Class: {Class}");
        }
    }
}