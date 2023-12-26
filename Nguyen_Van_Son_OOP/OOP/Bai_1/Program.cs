using Bai_1;

List<Person>? li;

void Init()
{
    li = new List<Person>
            {
                new Person(1,"Nguyen Van A",21,12000000,0.3f),
                new Person(2,"Nguyen Van B",20,10000000,0.3f),
                new Person(3,"Nguyen Van C",19,8000000,0.2f),
                new Person(4,"Nguyen Van D",22,14000000,0.3f),
                new Person(5,"Nguyen Van E",31,25000000,0.4f)
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