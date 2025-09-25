using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeViewProject.Model
{
    public class ProductModel
    {
        public string? Name { get; set; }
        public double? Price { get; set; }

        public string? Factory { get; set; }

        public string? Provider { get; set; }

        public DateTime? ExpireDate { get; set; }

        public string? Address { get; set; }

        public string? ImagePath { get; set; }
        public string? Description { get; set; }

        public bool IsChecked { get; set; } = false;

        public ProductModel Clone()
        {
            var clone = new ProductModel()
            {
                Name = Name,
                Price = Price,
                Factory = Factory,
                Provider = Provider,
                ExpireDate = ExpireDate,
                Address = Address,
                Description = Description,
                IsChecked = IsChecked,
                ImagePath = ImagePath
            };
            return clone;
        }
    }
}