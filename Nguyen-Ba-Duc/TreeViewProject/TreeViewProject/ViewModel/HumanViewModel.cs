using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using TreeViewProject.Model;

namespace TreeViewProject.ViewModel
{
    public class HumanViewModel : BaseViewModel
    {
        private readonly HumanModel _humanModel;
        public string? Name => _humanModel.Name;

        public string? ImagePath => _humanModel.ImagePath;
        public DateTime? Birthday => _humanModel.Birthday;

        public string? Address => _humanModel.Address;
        public string? Description => _humanModel.Description;

        public bool IsChecked
        {
            get => _humanModel.IsChecked;
            set
            {
                _humanModel.IsChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        public HumanViewModel(HumanModel humanModel)
        {
            _humanModel = humanModel;
        }
    }
}