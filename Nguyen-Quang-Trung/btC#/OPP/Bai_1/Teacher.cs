namespace Bai_1
{
    internal class Teacher : Person
    {
        public string School { get; set; }

        public override string ToString()
        {
            string baseInfor = base.ToString();
            string infor = string.Format("{0} {1, -20} {2, -15} {3, -5}", baseInfor, School, Income, TaxCoe);
            return infor;
        }
    }
}