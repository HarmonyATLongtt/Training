using System;
using WPF_Sample.Enums;

namespace WPF_Sample.Models
{
    /// <summary>
    /// Represent a student
    /// </summary>
    public class StudentModel : PersonModel
    {
        /// <summary>
        /// Create new instance of student
        /// </summary>
        /// <param name="name"></param>
        /// <param name="birthday"></param>
        /// <param name="gender"></param>
        public StudentModel(string name, DateTime birthday, Gender gender) : base(name, birthday, gender)
        {
        }

        /// <summary>
        /// Create default stuent
        /// </summary>
        public static StudentModel CreateDefault()
        {
            return new StudentModel("new student", DateTime.Now, Gender.None);
        }

        /// <summary>
        /// Clone this student  with new memory
        /// </summary>
        public StudentModel DeepCopy()
        {
            return new StudentModel(this.Name, this.Birthday, this.Gender);
        }
    }
}