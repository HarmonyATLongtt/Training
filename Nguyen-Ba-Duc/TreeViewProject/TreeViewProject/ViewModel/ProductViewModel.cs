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
    public class ProductViewModel : BaseViewModel, ICopyPasteHandler
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

        public bool IsChecked
        {
            get => _productModel.IsChecked;
            set
            {
                _productModel.IsChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        public string SelectedText { get; set; }
        public ICommand HighLightCommand { get; }

        public ProductViewModel(ProductModel productModel)
        {
            _productModel = productModel;
            HighLightCommand = new RelayCommand(_ => HighLightCommandInvoke());
        }

        public void Copy(MainViewModel mainVM)
        {
            mainVM.ClipBoardObj = SelectedText;
        }

        public void Paste(MainViewModel mainVM)
        {
            if (mainVM.ClipBoardObj is not string) return;
        }

        private void HighLightCommandInvoke()
        {
            if (Keyboard.FocusedElement is TextBox tb)
            {
                if (!string.IsNullOrEmpty(tb.SelectedText))
                {
                    SelectedText = tb.SelectedText;
                }
            }
        }
    }
}