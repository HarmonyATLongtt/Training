using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Vi_Du_Exception
{
    
    public class DataTooLongExeption : Exception
    {

        const string erroMessage = "Dữ liệu quá dài";
        public DataTooLongExeption() : base(erroMessage)
        {
        }
    }
    class Program
    {
        public static void UserInput(string s)
        {
            if (s.Length > 10)
            {
                Exception e = new DataTooLongExeption();
                throw e;    // lỗi văng ra
            }
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            //int a = 5;
            //int b = 0;

            //try
            //{
            //    var c = a / b;
            //    Console.WriteLine(c);
            //}
            //catch
            //{
            //    Console.WriteLine("Phep tinh khong thuc hien duoc");

            //}
            //Console.ReadLine();

            try
            {
                UserInput("Đây là một chuỗi rất dài");
            }
            catch (DataTooLongExeption e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception otherExeption)
            {
                Console.WriteLine(otherExeption.Message);
            }
            Console.ReadLine();
        }
    }

}
