using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP
{
    public class TaxData
    {
        public float GetTaxCoe(int age, float income)
        {
            float Taxcoe = 0.0f;
            if (age < 18)
            {
                Taxcoe = 0.0f;
                return Taxcoe;
            }
            if (income <= 9000000)
            {
                Taxcoe = 0.05f;
            }
            else if (income > 9000000 && income <= 15000000)
            {
                Taxcoe = 0.1f;
            }
            else if (income > 15000000 && income <= 20000000)
            {
                Taxcoe = 0.15f;
            }
            else if (income > 20000000 && income <= 30000000)
            {
                Taxcoe = 0.2f;
            }
            else
            {
                Taxcoe = 0.3f;
            }

            return Taxcoe;
        }
    }
}