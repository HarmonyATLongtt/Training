namespace OOP.Data
{
    public class TaxData
    {
        public float GetTaxCoe(int Age, double Income)
        {
            if(Age < 18) return 0;
            else
            {
                if (Income <= 9000000) return 0.05f;
                else if (Income <= 15000000) return 0.1f;
                else if (Income <= 20000000) return 0.15f;
                else return 0.2f;
            }
        }
    }
}