using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeViewProject.Model
{
    public class HumanModel
    {
        public string? ImagePath { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }

        public bool IsChecked { get; set; } = false;

        public HumanModel Clone()
        {
            var clone = new HumanModel()
            {
                Name = Name,
                Address = Address,
                Birthday = Birthday,
                Description = Description,
                IsChecked = IsChecked,
                ImagePath = ImagePath
            };
            return clone;
        }
    }
}