using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                    case 3:
                        Console.WriteLine("Tinh nang nay chua hoan thien.....");
                        break;

                    case 4:
                        //Interface_IPerson timkiem = new Interface_IPerson();
                        //string keyword;
                        //foreach (var item in listInfor)
                        //{
                        //    keyword = item.ToString();
                        //    timkiem.GetInfor(keyword);
                        //}
                        break;

                    default:
                        Console.WriteLine("Nhap sai cu phap. Xin moi nhap lai.....");
                        break;
                }
            }
            Console.ReadKey();
        }

        public static void Init(List<Interface_IPerson> listInfor)
        {
            // Interface_IPerson infor = new Interface_IPerson();
            Student hs = new Student();
            Teacher gv = new Teacher();
            Employee nv = new Employee();
            TaxData hesothue = new TaxData();
            Console.WriteLine("Moi nhap thong tin.....");
            bool inputvalue1, inputvalue2, inputvalue3;
            Console.WriteLine("Ban muon nhap thong tin cua ai");
            Console.WriteLine("1. Student");
            Console.WriteLine("2. Teacher");
            Console.WriteLine("3. Employee");
            Console.Write("Lua chon cua ban: ");
            int doituong_input = int.Parse(Console.ReadLine());
            switch (doituong_input)
            {
                case 1:
                    do
                    {
                        Console.Write("Nhap ID: ");
                        try
                        {
                            hs.ID = Convert.ToInt32(Console.ReadLine());
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
                    hs.Name = Convert.ToString(Console.ReadLine());
                    do
                    {
                        Console.Write("Nhap Age: ");
                        try
                        {
                            hs.Age = Convert.ToInt32(Console.ReadLine());
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
                    Console.Write("Nhap School: ");
                    hs.School = Convert.ToString(Console.ReadLine());
                    Console.Write("Nhap Class: ");
                    hs.Class = Convert.ToString(Console.ReadLine());
                    do
                    {
                        Console.Write("Nhap Income: ");
                        try
                        {
                            hs.Income = Convert.ToDouble(Console.ReadLine());
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
                    hs.TaxCoe = hesothue.GetTaxCoe(hs.Age, hs.Income);
                    listInfor.Add(hs);
                    break;

                case 2:
                    do
                    {
                        Console.Write("Nhap ID: ");
                        try
                        {
                            gv.ID = Convert.ToInt32(Console.ReadLine());
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
                    gv.Name = Convert.ToString(Console.ReadLine());
                    do
                    {
                        Console.Write("Nhap Age: ");
                        try
                        {
                            hs.Age = Convert.ToInt32(Console.ReadLine());
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
                    Console.Write("Nhap School: ");
                    gv.School = Convert.ToString(Console.ReadLine());
                    do
                    {
                        Console.Write("Nhap Income: ");
                        try
                        {
                            gv.Income = Convert.ToDouble(Console.ReadLine());
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
                    gv.TaxCoe = hesothue.GetTaxCoe(hs.Age, hs.Income);
                    listInfor.Add(gv);
                    break;

                case 3:
                    do
                    {
                        Console.Write("Nhap ID: ");
                        try
                        {
                            nv.ID = Convert.ToInt32(Console.ReadLine());
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
                    nv.Name = Convert.ToString(Console.ReadLine());
                    do
                    {
                        Console.Write("Nhap Age: ");
                        try
                        {
                            hs.Age = Convert.ToInt32(Console.ReadLine());
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
                    Console.Write("Nhap Company: ");
                    nv.Company = Convert.ToString(Console.ReadLine());
                    Console.Write("Nhap JobTitle: ");
                    nv.JobTitle = Convert.ToString(Console.ReadLine());
                    do
                    {
                        Console.Write("Nhap Income: ");
                        try
                        {
                            nv.Income = Convert.ToDouble(Console.ReadLine());
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
                    nv.TaxCoe = hesothue.GetTaxCoe(hs.Age, hs.Income);
                    listInfor.Add(hs);
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
            foreach (Student hs in listInfor)
            {
                Console.WriteLine("{0, -5} {1, -20} {2, -5} {3, -20} {4, -10}", hs.ID, hs.Name, hs.Age, hs.School, hs.Class);
            }

            var count_gv = listInfor.OfType<Teacher>().ToList();
            Console.WriteLine("Teacher: " + count_gv.Count);
            Console.WriteLine("{0, -5} {1, -20} {2, -5} {3, -20} {4, -10} {5, -5}",
               "ID", "Name", "Age", "School", "Income", "TaxCoe");
            foreach (Teacher gv in listInfor)
            {
                Console.WriteLine("{0, -5} {1, -20} {2, -5} {3, -20} {4, -10} {5, -5}", gv.ID, gv.Name, gv.Age, gv.School, gv.Income, gv.TaxCoe);
            }

            var count_nv = listInfor.OfType<Employee>().ToList();
            Console.WriteLine("Employee: " + count_nv.Count);
            Console.WriteLine("{0, -5} {1, -20} {2, -5} {3, -20} {4, -20} {5, -10} {6, -5}",
               "ID", "Name", "Age", "Company", "JobTitle", "Income", "TaxCoe");
            foreach (Employee nv in listInfor)
            {
                Console.WriteLine("{0, -5} {1, -20} {2, -5} {3, -20} {4, -20} {5, -10} {6, -5}", nv.ID, nv.Name, nv.Age, nv.Company, nv.JobTitle, nv.Income, nv.TaxCoe);
            }
        }
    }
}