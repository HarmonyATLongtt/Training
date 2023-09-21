namespace NguyenVanViet.Core
{
    public class TaxData
    {
        public float GetTaxCoe(int age, float income)
        {
            if (age < 18)
                return 0;
            else if (income <= 9000000)
                return 0.05f;
            else if (income <= 15000000)
                return 0.1f;
            else if (income <= 20000000)
                return 0.15f;
            else
                return 0.2f;
        }
    }
}