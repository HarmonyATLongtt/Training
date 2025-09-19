using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeViewProject.Model;

namespace TreeViewProject.ViewModel
{
    public class ProductViewModel
    {
        private readonly ProductModel _productModel;
        public string? ProductName => _productModel.ProductName;
        public string? ImagePath => _productModel.ImagePath;
        public DateTime? ExpireDate => _productModel.ExpireDate;

        public string? Address => _productModel.Address;
        public string? Description => _productModel.Description;

        public double? Price => _productModel.Price;

        public string? Factory => _productModel.Factory;

        public string? Provider => _productModel.Provider;

        public ProductViewModel(ProductModel productModel)
        {
            _productModel = productModel;
        }
    }
}