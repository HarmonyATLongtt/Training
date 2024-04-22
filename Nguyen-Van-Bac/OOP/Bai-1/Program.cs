// See https://aka.ms/new-console-template for more information
using Bai_1;
using System.Runtime.CompilerServices;

 static List<Person> Init()
{
    var ListPerson = new List<Person>()
    {
        new Person()
        {
            Id = 1,
            Name = "Nam",
            Age = 18,
            Income = 8000000,
            TaxCoe = 0.05
        },
        new Person()
        {
            Id = 2,
            Name = "Binh",
            Age = 19,
            Income = 9000000,
            TaxCoe = 0.05
        }
    };
    return ListPerson;

}
static void Output(List<Person> people)
{
    foreach (var item in people)
    {
         Console.WriteLine($"ID: {item.Id}, Name: {item.Name}, Tax: {item.TaxCoe}");
    }
}
Output(Init());
