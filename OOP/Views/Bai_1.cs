using NguyenVanViet.Controls;
using System;

namespace Views
{
    public class Bai_1
    {
        public static void Main(string[] args)
        {
            Program p = new Program();
            p.Init(false, NguyenVanViet.Core.ExerciseEnum.Bai_1);
            p.Output(false);

            Console.Read();
        }
    }
}