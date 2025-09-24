using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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

        //public string SelectedText { get; set; }
        //public ICommand HighLightCommand { get; }

        public HumanViewModel(HumanModel humanModel)
        {
            _humanModel = humanModel;
            //HighLightCommand = new RelayCommand(_ => HighLightCommandInvoke());
        }

        public HumanViewModel Clone()
        {
            return new HumanViewModel(_humanModel.Clone());
        }

        //public void Copy(MainViewModel mainVM)
        //{
        //    mainVM.ClipBoardObj = SelectedText;
        //}

        //public void Paste(MainViewModel mainVM)
        //{
        //    if (mainVM.ClipBoardObj is not string) return;
        //}

        //private void HighLightCommandInvoke()
        //{
        //    if (Keyboard.FocusedElement is TextBox tb)
        //    {
        //        if (!string.IsNullOrEmpty(tb.SelectedText))
        //        {
        //            SelectedText = tb.SelectedText;
        //        }
        //    }
        //}
    }
}