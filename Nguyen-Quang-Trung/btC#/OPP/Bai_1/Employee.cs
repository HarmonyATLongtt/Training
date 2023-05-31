namespace Bai_1
{
    public class Employee : Person
    {
        public string Company { get; set; }
        public string JobTitle { get; set; }

        public override string ToString()
        {
            string baseInfor = base.ToString();
            string infor = string.Format("{0} {1, -20} {2, -20} {3, -15} {4, -5}", baseInfor, Company, JobTitle, Income, TaxCoe);
            return infor;
        }
    }
}