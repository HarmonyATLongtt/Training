using Bai_2;


namespace OOP.Bai_2
{
    class Program
    {
        static List<Person> Init()
        {
            TaxData taxData = new TaxData();
            List<Person> p = new List<Person>
            {
                new Person("1", "Nguyen Van A", 30, 50000, taxData),
                new Person("2", "Nguyen Van B", 45, 70000, taxData),
                new Person("3", "Nguyen Van C", 35, 63000, taxData),
                new Person("4", "Nguyen Van D", 17, 40000,taxData)
            };
            return p;
        }

        static void Output(List<Person> p)
        {
            foreach (var person in p)
            {
                // Corrected the interpolation syntax for printing
                Console.WriteLine($"ID: {person.ID}, Name: {person.Name}, Tax: {person.GetTax():0.00}");
            }
        }

        static void Main(string[] args)
        {
            List<Person> p = Init();
            Output(p);
        }
    }
}
