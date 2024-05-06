using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;


namespace Stack_List
{
    class Program
    {
        static void VDStack()
        {
            Stack MyStack = new Stack();

            MyStack.Push("Stack 1");
            MyStack.Push("Stack 2");
            MyStack.Push("Stack 3");
            MyStack.Push("Stack 4");

            Console.WriteLine("Tong so stack la: {0}", MyStack.Count);

            Console.WriteLine("Stack dau tien la: {0}", MyStack.Peek());

            int Length = MyStack.Count;
            for (int i = 0; i < Length; i++)
            {
                Console.WriteLine(" " + MyStack.Pop());
            }

            Console.WriteLine("So phan tu Stack con lai: {0}", MyStack.Count);
        }

        static void VDList()
        {
            List<string> MyList = new List<string>();
            MyList.Add("Hoc C#");
            MyList.Add("Hoc Revit");
            MyList.Add("Hoc API");

            Console.WriteLine("So luong phan tu trong List la: {0}", MyList.Count);

            foreach (string item in MyList)
            {
                Console.WriteLine(" " + item);
            }
            Console.WriteLine();

            MyList.Insert(1, "Hoc JS");

            Console.WriteLine("List sau khi insert");
            foreach (string item in MyList)
            {
                Console.WriteLine(" " + item);
            }
            
        }

        static void Main(string[] args)
        {
            VDStack();
            

            VDList();

            Console.ReadLine();
        }
    }
}
