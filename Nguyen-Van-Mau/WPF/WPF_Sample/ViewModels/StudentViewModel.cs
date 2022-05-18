using System;
using WPF_Sample.Enums;
using WPF_Sample.Models;

namespace WPF_Sample.ViewModels
{
    public class StudentViewModel : BindableBase
    {
        private bool _allowEditable;
        private StudentModel _student;
        public StudentModel StudentModel => _student;

        public bool AllowEditable
        {
            get => _allowEditable;
            set
            {
                SetProperty(ref _allowEditable, value);
            }
        }

        public string Name
        {
            get
            {
                return _student.Name;
            }
            set
            {
                _student.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public int Age
        {
            get
            {
                return _student.Age;
            }
        }

        public DateTime Birthday
        {
            get
            {
                return _student.Birthday;
            }
            set
            {
                _student.Birthday = value;
                OnPropertyChanged(nameof(Age));
            }
        }

        public Gender Gender
        {
            get
            {
                return _student.Gender;
            }
            set
            {
                _student.Gender = value;
                OnPropertyChanged(nameof(Gender));
            }
        }

        /// <summary>
        /// Create new instance of StudentViewModel
        /// </summary>
        /// <param name="student"></param>
        public StudentViewModel(StudentModel student)
        {
            _student = student;
        }

        /// <summary>
        /// Clone this student viewmodel  with new memory
        /// </summary>
        public StudentViewModel DeepCopy()
        {
            StudentModel studentModel = _student.DeepCopy();
            return new StudentViewModel(studentModel);
        }
    }
}