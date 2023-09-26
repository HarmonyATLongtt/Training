using NguyenVanViet.Controls;
using System;

namespace Views
{
    public class Bai_2
    {
        public static void Main(string[] args)
        {
            Program p = new Program();
            p.Init(false, NguyenVanViet.Core.ExerciseEnum.Bai_2);
            p.Output(false);

            Console.Read();
        }
    }
}