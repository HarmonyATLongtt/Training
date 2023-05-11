namespace Ex_WPF
{
    public class Person
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
        public double TaxFactor { get; set; }

        public Person()
        {

        }

        public Person(string id, string name, int age, string address, double factor)
        {
            ID = id;
            Name = name;
            Age = age;
            Address = address;
            TaxFactor = factor;
        }
    }
}
