using Bai_3;
using System;
using OOP.Bai_3;
namespace OOP.Bai_3
{
    public class TaxData : Person1
    {
        
        public double GetTaxCoe(int age, double income)
        {
            if (age < 18)
            {
                return 0;
            }
            else if (age >= 18)
            {
                if (income <= 9000000)
                {
                    return 0.05;
                }
                else if (income > 9000000 && income <= 15000000)
                {
                    return 0.1;
                }
                else if (income > 15000000 && income <= 20000000)
                {
                    return 0.15;
                }
                else if (income > 20000000 && income <= 30000000)
                {
                    return 0.2;
                }
            }
            return 0.25;
            
        }
    }
}
