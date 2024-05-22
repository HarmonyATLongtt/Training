using Bai_1;

class Program
{
    static void Main(string[] args)
    {
        Program program = new Program();
        List<Person> lp = program.Init();
        program.Output(lp);

        return;
    }

    public List<Person> Init()
    {
        List<Person> people = new List<Person>();
        people.Add(new Person { id = "A1", name = "Bang Qua", age = 21, income = 10000000, taxCoe = 2 });
        people.Add(new Person { id = "A2", name = "La Luot", age = 24, income = 5000000, taxCoe = 1 });

        return people;
    }

    public void Output(List<Person> p)
    {
        foreach(Person person in p)
        {
            Console.WriteLine("ID: " + person.id);
            Console.WriteLine("Name: " + person.name);
            Console.WriteLine("Tax: " + person.GetTax() + "\n");
        }
    }
}