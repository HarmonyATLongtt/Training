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
            PersonData.Init();
            PersonData.Output();
            Console.ReadLine();
        }
    }
}