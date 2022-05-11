using System.Collections.ObjectModel;

namespace WPF_Sample.Models
{
    /// <summary>
    /// Represent a class
    /// </summary>
    public class ClassModel
    {
        /// <summary>
        /// Name of class
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// The teacher name , who manages the class
        /// </summary>
        public string FormTeacherName { get; set; }

        /// <summary>
        /// List of students in class
        /// </summary>
        public ObservableCollection<StudentModel> StudentModels { get; set; }

        /// <summary>
        /// Create a new class
        /// </summary>
        /// <param name="className"></param>
        /// <param name="formTeacherName"></param>
        public ClassModel(string className, string formTeacherName)
        {
            ClassName = className;
            FormTeacherName = formTeacherName;

            StudentModels = new ObservableCollection<StudentModel>();
            // StudentModels.Add(StudentModel.CreateDefault());
            // StudentModels.Add(StudentModel.CreateDefault());
        }

        /// <summary>
        /// Clone all member of the class with new allocte memory
        /// </summary>
        /// <returns></returns>
        public ClassModel DeepCopy()
        {
            ClassModel copyClassModel = new ClassModel(this.ClassName, this.FormTeacherName);
            copyClassModel.StudentModels.Clear();
            foreach (var studenModel in this.StudentModels)
            {
                StudentModel copyModel = studenModel.DeepCopy();
                copyClassModel.StudentModels.Add(copyModel);
            }
            return copyClassModel;
        }

        /// <summary>
        /// Create a defalt class
        /// </summary>
        /// <returns></returns>
        public static ClassModel CreateDefault()
        {
            return new ClassModel("new class", "new teacher");
        }
    }
}