using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_1
{
    internal class Bai_1
    {
        private static void Main(string[] args)
        {
            List<Person> listInfor = new List<Person>();
            // Person tinhtoan = new Person();
            while (true)
            {
                Console.WriteLine("*****Menu tuy chon*****");
                Console.WriteLine("1. Nhap thong tin");
                Console.WriteLine("2. Hien thi thong tin");
                Console.WriteLine("3. Kiem tra trung lap");
                Console.WriteLine("0. Thoat");
                Console.Write("Nhap tuy chon: ");
                int luachon = Convert.ToInt16(Console.ReadLine());
                switch (luachon)
                {
                    case 0:
                        return;

                    case 1:
                        Init(listInfor);
                        break;

                    case 2:
                        Output(listInfor);
                        break;
                }
            }
            Console.ReadKey();
        }

        public static void Init(List<Person> listInfor)
        {
            Person Infor = new Person();
            Console.WriteLine("Moi nhap thong tin.....");
            Console.Write("Nhap ID: ");
            Infor.ID = Convert.ToInt32(Console.ReadLine());
            Console.Write("Nhap Name: ");
            Infor.Name = Convert.ToString(Console.ReadLine());
            Console.Write("Nhap Age: ");
            Infor.Age = Convert.ToInt32(Console.ReadLine());
            Console.Write("Nhap Income: ");
            Infor.Income = Convert.ToDecimal(Console.ReadLine());
            Console.Write("Nhap Taxcoe: ");
            Infor.TaxCoe = Convert.ToDecimal(Console.ReadLine());
            listInfor.Add(Infor);
        }

        public static void Output(List<Person> listInfor)
        {
            Console.WriteLine("{0, -5} {1, -20} {2, -5}",
               "ID", "Name", "Income");
            foreach (Person Infor in listInfor)
            {
                Console.WriteLine("{0, -5} {1, -20} {2, -5}", Infor.ID, Infor.Name, Infor.Income);
            }
        }
    }
}