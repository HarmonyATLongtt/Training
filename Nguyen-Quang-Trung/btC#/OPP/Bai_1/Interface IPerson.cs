using System;
using System.Collections.Generic;
using System.Linq;

namespace Bai_1
{
    public class Interface_IPerson
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; } // hệ số thuế

        public bool Equals(Interface_IPerson p)
        {
            if (p == null)
            {
                return false;
            }

            return this == p; // ??? trùng hoàn toàn thì mới trả về true, nếu so sánh theo thuộc tính thì chỉ nên so sánh id
        }

        public double GetTax(double Income, double TaxCoe)
        {
            return Income * TaxCoe;
        }

        public string GetInfor(string key, string value)
        {
            if (key == value)
            {
                return $"ID: {ID}, Name: {Name}, Age: {Age}";
            }
            else
            {
                Console.WriteLine("Khong co thong tin trong co so du lieu");
                return null;
            }
        }

        public override string ToString()
        {
            return string.Format("{0, -5} {1, -20} {2, -5}", ID, Name, Age);
        }
    }
}