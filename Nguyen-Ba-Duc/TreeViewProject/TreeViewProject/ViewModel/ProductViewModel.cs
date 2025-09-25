using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using TreeViewProject.Model;

namespace TreeViewProject.ViewModel
{
    public class ProductViewModel : BaseViewModel
    {
        private readonly ProductModel _productModel;
        public string? Name => _productModel.Name;
        public string? ImagePath => _productModel.ImagePath;
        public DateTime? ExpireDate => _productModel.ExpireDate;

        public string? Address => _productModel.Address;
        public string? Description => _productModel.Description;

        public double? Price => _productModel.Price;

        public string? Factory => _productModel.Factory;

        public string? Provider => _productModel.Provider;

        public bool IsChecked
        {
            get => _productModel.IsChecked;
            set
            {
                _productModel.IsChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        public ProductViewModel(ProductModel productModel)
        {
            _productModel = productModel;
        }

        public ProductViewModel Clone()
        {
            return new ProductViewModel(_productModel.Clone());
        }
    }
}