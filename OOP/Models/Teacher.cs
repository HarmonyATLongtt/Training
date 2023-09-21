using NguyenVanViet.Core;

namespace NguyenVanViet.Models
{
    public class Teacher : Person, IPerson
    {
        public string School { get; set; }

        public Teacher(int id, string name, int age, float income, string school, TaxData taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            School = school;
            TaxCoe = taxData.GetTaxCoe(age, income);
        }

        public void GetInfo()
        {
            Console.WriteLine($"ID: {Id}, Name: {Name}, Age: {Age}, School: {School}, Income: {Income}, Tax: {TaxCoe}");
        }
    }
}