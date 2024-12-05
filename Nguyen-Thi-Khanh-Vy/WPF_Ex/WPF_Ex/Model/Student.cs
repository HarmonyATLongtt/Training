namespace WPF_Ex.Model
{
    public class Student : Person
    {
        //public string ID { get; set; }
        //public string Name { get; set; }
        //public int Age { get; set; }
        public string Class { get; set; } // Lớp
        public string School { get; set; } // Trường

        public Student() : base() { }

        // Constructor với tham số, sử dụng base constructor để khởi tạo các thuộc tính từ lớp cha Person
        public Student(string id, string name, int age, string @class, string school) : base(id, name, age)
        {
            Class = @class;
            School = school;
        }
    }
}
