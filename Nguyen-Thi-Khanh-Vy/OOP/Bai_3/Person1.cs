using System;
using System.Security.Claims;
using Bai_3;
using OOP.Bai_3;

public class Person1 : IPerson
{
	protected string ID;
	protected string Name;
	protected int Age;
	
	public Person1()
	{
	}
	public Person1 (string id, string name, int age)
	{
		ID = id;
		Name = name;
		Age = age;
		
	}
    protected static string GetInput(string prompt)
    {
        string input;
        do
        {
            Console.WriteLine(prompt);
            input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Gia tri nhap khong duoc de trong.Vui long nhap lai!");
            }
        } while (string.IsNullOrWhiteSpace(input));
        return input;
    }

    public void Nhap()
    {
        ID = GetInput("Nhap ID:");
        Name = GetInput("Nhap Ho ten: ");

        while (true)
        {
            string ageInput = GetInput("Nhap tuoi: ");
            if (int.TryParse(ageInput, out int age) && age > 0)
            {
                Age = age;
                break;
            }
            Console.WriteLine("Tuoi khong hop le vui lpng nhap lai");
        }
       
    }
    public string GetInfo()
    {
        return $"ID: {ID}, Name: {Name}, Age: {Age} ";
    }
}
