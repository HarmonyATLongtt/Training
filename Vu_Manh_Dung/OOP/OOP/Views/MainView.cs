using OOP.Data;
using OOP.Models;
using System;
using System.Collections.Generic;

namespace OOP.Views
{
    public class MainView
    {
        public static void Main(string[] args)
        {
            List<Person> list = new List<Person>();
            PersonData.Init(list);
            PersonData.Output(list);
            Console.ReadLine();
        }
    }
}