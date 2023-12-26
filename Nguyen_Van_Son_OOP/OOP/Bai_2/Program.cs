using Bai_2;

List<Person>? li;

void Init()
{
    TaxData taxData = new TaxData();
    li = new List<Person>
            {
                new Person(1,"Nguyen Van A",21,12000000,taxData),
                new Person(2,"Nguyen Van B",20,10000000,taxData),
                new Person(3,"Nguyen Van C",19,8000000,taxData),
                new Person(4,"Nguyen Van D",22,14000000,taxData),
                new Person(5,"Nguyen Van E",31,25000000,taxData)
            };
}

void Output()
{
    Console.WriteLine($"{"Id",-10}{"Income",-15}{"Tax"}");
    foreach (Person p in li)
    {
        Console.WriteLine($"{p.Id,-10}{p.Income,-15}{p.GetTax()}");
    }
}

Init();
Output();