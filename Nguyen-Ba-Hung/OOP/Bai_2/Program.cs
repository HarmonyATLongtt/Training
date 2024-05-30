using Bai_2;

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
        people.Add(new Person ( "A1", "Bang Qua", 21, 10000000 ));
        people.Add(new Person ("A2", "La Luot", 24, 5000000));

        return people;
    }

    public void Output(List<Person> p)
    {
        foreach (Person person in p)
        {
            Console.WriteLine("ID: " + person.id);
            Console.WriteLine("Name: " + person.name);
            Console.WriteLine("Tax: " + person.GetTax() + "\n");
        }
    }
}