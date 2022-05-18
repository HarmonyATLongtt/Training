using System;
using WPF_Sample.Enums;

namespace WPF_Sample.Models
{
    /// <summary>
    /// Represent a person
    /// </summary>
    public class PersonModel
    {
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        /// <summary>
        /// Age  =  current year - Birthday year
        /// </summary>

        [Newtonsoft.Json.JsonIgnore]
        public int Age { get => DateTime.Now.Year - Birthday.Year; }

        private Gender _gender;

        public Gender Gender
        {
            get => _gender;
            set
            {
                if (Enum.IsDefined(typeof(Gender), value))
                {
                    _gender = value;
                }
                else
                {
                    _gender = Gender.Female;
                }
            }
        }

        /// <summary>
        /// Create instance of person
        /// </summary>
        /// <param name="name"></param>
        /// <param name="birthDay"></param>
        /// <param name="gender"></param>
        public PersonModel(string name, DateTime birthDay, Gender gender)
        {
            Name = name;
            Birthday = birthDay;
            Gender = gender;
        }
    }
}