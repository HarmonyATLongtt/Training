using NguyenVanViet.Core;

namespace NguyenVanViet.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public float Income { get; set; }
        public float TaxCoe { get; set; }

        public Person()
        { }

        public Person(int id, string? name, int age, float income, float taxCoe)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = taxCoe;
        }

        public Person(int id, string? name, int age, float income, TaxData taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = taxData.GetTaxCoe(age, income);
        }

        public bool Equals(Person other)
        {
            return Id == other.Id && Name == other.Name && Age == other.Age && TaxCoe == other.TaxCoe;
        }

        public float GetTax()
        {
            return Income * TaxCoe;
        }
    }
}