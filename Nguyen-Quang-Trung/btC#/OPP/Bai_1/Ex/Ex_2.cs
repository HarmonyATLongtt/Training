using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_1.Ex
{
    public class Ex_2
    {
        public static void Execution()
        {
            List<Interface_IPerson> listInfor = new List<Interface_IPerson>();
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

        public static void Init(List<Interface_IPerson> listInfor)
        {
            Interface_IPerson infor = new Interface_IPerson();
            Console.WriteLine("Moi nhap thong tin.....");
            bool inputvalue1, inputvalue2, inputvalue3; //inputvalue4;
            do
            {
                Console.Write("Nhap ID: ");
                try
                {
                    infor.ID = Convert.ToInt32(Console.ReadLine());
                    inputvalue1 = false;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Xay ra loi: Format Exception");
                    inputvalue1 = true;
                }
                finally
                {
                    Console.Write("");
                }
            } while (inputvalue1);
            Console.Write("Nhap Name: ");
            infor.Name = Convert.ToString(Console.ReadLine());
            do
            {
                Console.Write("Nhap Age: ");
                try
                {
                    infor.Age = Convert.ToInt32(Console.ReadLine());
                    inputvalue2 = false;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Xay ra loi: Format Exception");
                    inputvalue2 = true;
                }
                finally
                {
                    Console.Write("");
                }
            } while (inputvalue2);
            do
            {
                Console.Write("Nhap Income: ");
                try
                {
                    infor.Income = Convert.ToDouble(Console.ReadLine());
                    inputvalue3 = false;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Xay ra loi: Format Exception");
                    inputvalue3 = true;
                }
                finally
                {
                    Console.Write("");
                }
            } while (inputvalue3);
            TaxData hesothue = new TaxData();
            infor.TaxCoe = hesothue.GetTaxCoe(infor.Age, infor.Income);
            listInfor.Add(infor);
        }

        public static void Output(List<Interface_IPerson> listInfor)
        {
            Console.WriteLine("{0, -5} {1, -20} {2, -10} {3, -5}",
               "ID", "Name", "Income", "TaxCoe");
            foreach (Interface_IPerson infor in listInfor)
            {
                Console.WriteLine("{0, -5} {1, -20} {2, -10} {3, -5}", infor.ID, infor.Name, infor.Income, infor.TaxCoe);
            }
        }
    }
}