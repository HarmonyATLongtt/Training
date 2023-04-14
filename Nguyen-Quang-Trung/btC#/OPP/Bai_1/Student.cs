namespace Bai_1
{
    public class Student : Interface_IPerson
    {
        public string Class { get; set; }
        public string School { get; set; }

        public override string ToString()
        {
            string baseInfor = base.ToString();
            string infor = string.Format("{0} {1, -20} {2, -10}", baseInfor, Class, School);
            return infor;
        }
    }
}