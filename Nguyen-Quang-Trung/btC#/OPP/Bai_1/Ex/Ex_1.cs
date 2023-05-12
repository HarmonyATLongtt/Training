using System;
using System.Collections.Generic;

namespace Bai_1.Ex
{
    public class Ex_1
    {
        public static void Execution()
        {
            List<Person> listInfor = new List<Person>();
            while (true)
            {
                Console.WriteLine("*****Menu tuy chon*****");
                Console.WriteLine("1. Nhap thong tin");
                Console.WriteLine("2. Hien thi thong tin");
                Console.WriteLine("3. Kiem tra trung lap");
                Console.WriteLine("0. Thoat");
            nhapLai:
                Console.Write("Lua chon cua ban la: ");
                if (int.TryParse(Console.ReadLine(), out int luaChon))
                {
                    switch (luaChon)
                    {
                        case 0:
                            return;

                        case 1:
                            Init(listInfor);
                            break;

                        case 2:
                            Output(listInfor);
                            break;

                        case 3:
                            Console.WriteLine("Tinh nang nay chua hoan thien.....");
                            break;

                        default:
                            Console.WriteLine("Lua chon cua ban hien gio khong co. Moi lua chon lai voi 0/1/2/3.....");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Nhap sai cu phap.Hay nhap 1 so.....");
                    goto nhapLai;
                }
            }
        }

        public static void Init(List<Person> listInfor)
        {
            Person infor = new Person();
            Console.WriteLine("Moi nhap thong tin.....");
            bool inputvalue1, inputvalue2, inputvalue3, inputvalue4;
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
            infor.Name = Console.ReadLine();
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

            do
            {
                Console.Write("Nhap Taxcoe: ");
                try
                {
                    infor.TaxCoe = Convert.ToDouble(Console.ReadLine());
                    inputvalue4 = false;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Xay ra loi: Format Exception");
                    inputvalue4 = true;
                }
                finally
                {
                    Console.Write("");
                }
            } while (inputvalue4);
            listInfor.Add(infor);
        }

        public static void Output(List<Person> listInfor)
        {
            Console.WriteLine("{0, -5} {1, -20} {2, -5}",
               "ID", "Name", "Income");
            foreach (Person infor in listInfor)
            {
                Console.WriteLine("{0, -5} {1, -20} {2, -5}", infor.ID, infor.Name, infor.Income);
            }
        }
    }
}