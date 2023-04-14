using System;
using System.Collections.Generic;
using System.Linq;

namespace Bai_1.Ex
{
    public class Ex_3
    {
        public static void Execution()
        {
            List<Interface_IPerson> listInfor = new List<Interface_IPerson>();
            while (true)
            {
                Console.WriteLine("*****Menu tuy chon*****");
                Console.WriteLine("1. Nhap thong tin");
                Console.WriteLine("2. Hien thi thong tin");
                Console.WriteLine("3. Kiem tra trung lap");
                Console.WriteLine("4. Tim kiem thong tin");
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

                        case 4:
                            Console.Write("Nhap ten nguoi muon truy van thong tin: ");
                            string key = Convert.ToString(Console.ReadLine());
                            Interface_IPerson find = new Interface_IPerson();
                            var names = listInfor.Select(p => p.Name).ToArray();

                            foreach (string item in names)
                            {
                                key = item;
                                find.GetInfor(key, item);
                            }
                            break;

                        default:
                            Console.WriteLine("Lua chon cua ban hien gio khong co. Moi lua chon lai voi 0/1/2/3/4.....");
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

        public static void Init(List<Interface_IPerson> listInfor)
        {
            Student hs = new Student();
            Teacher gv = new Teacher();
            Employee nv = new Employee();
            TaxData heSoThue = new TaxData();
            Console.WriteLine("Moi nhap thong tin.....");
            bool checkId, checkAge, checkIncome;
            Console.WriteLine("Ban muon nhap thong tin cua ai");
            Console.WriteLine("1. Student");
            Console.WriteLine("2. Teacher");
            Console.WriteLine("3. Employee");
            Console.Write("Lua chon cua ban: ");
            int doiTuongInput = int.Parse(Console.ReadLine());
            switch (doiTuongInput)
            {
                case 1:
                    do
                    {
                        Console.Write("Nhap ID: ");
                        try
                        {
                            hs.ID = Convert.ToInt32(Console.ReadLine());
                            checkId = false;
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Xay ra loi: Format Exception");
                            checkId = true;
                        }
                        finally
                        {
                            Console.Write("");
                        }
                    } while (checkId);
                    Console.Write("Nhap Name: ");
                    hs.Name = Console.ReadLine();
                    do
                    {
                        Console.Write("Nhap Age: ");
                        try
                        {
                            hs.Age = Convert.ToInt32(Console.ReadLine());
                            checkAge = false;
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Xay ra loi: Format Exception");
                            checkAge = true;
                        }
                        finally
                        {
                            Console.Write("");
                        }
                    } while (checkAge);
                    Console.Write("Nhap School: ");
                    hs.School = Console.ReadLine();
                    Console.Write("Nhap Class: ");
                    hs.Class = Console.ReadLine();
                    do
                    {
                        Console.Write("Nhap Income: ");
                        try
                        {
                            hs.Income = Convert.ToDouble(Console.ReadLine());
                            checkIncome = false;
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Xay ra loi: Format Exception");
                            checkIncome = true;
                        }
                        finally
                        {
                            Console.Write("");
                        }
                    } while (checkIncome);
                    hs.TaxCoe = heSoThue.GetTaxCoe(hs.Age, hs.Income);
                    listInfor.Add(hs);
                    break;

                case 2:
                    do
                    {
                        Console.Write("Nhap ID: ");
                        try
                        {
                            gv.ID = Convert.ToInt32(Console.ReadLine());
                            checkId = false;
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Xay ra loi: Format Exception");
                            checkId = true;
                        }
                        finally
                        {
                            Console.Write("");
                        }
                    } while (checkId);
                    Console.Write("Nhap Name: ");
                    gv.Name = Console.ReadLine();
                    do
                    {
                        Console.Write("Nhap Age: ");
                        try
                        {
                            hs.Age = Convert.ToInt32(Console.ReadLine());
                            checkAge = false;
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Xay ra loi: Format Exception");
                            checkAge = true;
                        }
                        finally
                        {
                            Console.Write("");
                        }
                    } while (checkAge);
                    Console.Write("Nhap School: ");
                    gv.School = Console.ReadLine();
                    do
                    {
                        Console.Write("Nhap Income: ");
                        try
                        {
                            gv.Income = Convert.ToDouble(Console.ReadLine());
                            checkIncome = false;
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Xay ra loi: Format Exception");
                            checkIncome = true;
                        }
                        finally
                        {
                            Console.Write("");
                        }
                    } while (checkIncome);
                    gv.TaxCoe = heSoThue.GetTaxCoe(hs.Age, hs.Income);
                    listInfor.Add(gv);
                    break;

                case 3:
                    do
                    {
                        Console.Write("Nhap ID: ");
                        try
                        {
                            nv.ID = Convert.ToInt32(Console.ReadLine());
                            checkId = false;
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Xay ra loi: Format Exception");
                            checkId = true;
                        }
                        finally
                        {
                            Console.Write("");
                        }
                    } while (checkId);
                    Console.Write("Nhap Name: ");
                    nv.Name = Console.ReadLine();
                    do
                    {
                        Console.Write("Nhap Age: ");
                        try
                        {
                            hs.Age = Convert.ToInt32(Console.ReadLine());
                            checkAge = false;
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Xay ra loi: Format Exception");
                            checkAge = true;
                        }
                        finally
                        {
                            Console.Write("");
                        }
                    } while (checkAge);
                    Console.Write("Nhap Company: ");
                    nv.Company = Console.ReadLine();
                    Console.Write("Nhap JobTitle: ");
                    nv.JobTitle = Console.ReadLine();
                    do
                    {
                        Console.Write("Nhap Income: ");
                        try
                        {
                            nv.Income = Convert.ToDouble(Console.ReadLine());
                            checkIncome = false;
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Xay ra loi: Format Exception");
                            checkIncome = true;
                        }
                        finally
                        {
                            Console.Write("");
                        }
                    } while (checkIncome);
                    nv.TaxCoe = heSoThue.GetTaxCoe(nv.Age, nv.Income);
                    listInfor.Add(nv);
                    break;

                default:
                    Console.WriteLine("Nhap sai cu phap. Xin moi nhap lai.....");
                    break;
            }
        }

        public static void Output(List<Interface_IPerson> listInfor)
        {
            var count_hs = listInfor.OfType<Student>().ToList();
            Console.WriteLine("Student: " + count_hs.Count);
            Console.WriteLine("{0, -5} {1, -20} {2, -5} {3, -20} {4, -10}",
                "ID", "Name", "Age", "School", "Class");
            foreach (Interface_IPerson infor in listInfor)
            {
                if (infor is Student hs)
                {
                    Console.WriteLine(hs.ToString());
                }
            }
            var count_gv = listInfor.OfType<Teacher>().ToList();
            Console.WriteLine("Teacher: " + count_gv.Count);
            Console.WriteLine("{0, -5} {1, -20} {2, -5} {3, -20} {4, -10} {5, -5}",
                               "ID", "Name", "Age", "School", "Income", "TaxCoe");
            foreach (Interface_IPerson infor in listInfor)
            {
                if (infor is Teacher gv)
                {
                    Console.WriteLine(gv.ToString());
                }
            }
            var count_nv = listInfor.OfType<Employee>().ToList();
            Console.WriteLine("Employee: " + count_nv.Count);
            Console.WriteLine("{0, -5} {1, -20} {2, -5} {3, -20} {4, -20} {5, -10} {6, -5}",
                               "ID", "Name", "Age", "Company", "JobTitle", "Income", "TaxCoe");
            foreach (Interface_IPerson infor in listInfor)
            {
                if (infor is Employee nv)
                {
                    Console.WriteLine(nv.ToString());
                }
            }
        }
    }
}