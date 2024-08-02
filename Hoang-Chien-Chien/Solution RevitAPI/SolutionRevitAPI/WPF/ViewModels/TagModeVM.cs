using System.Windows;
using System.Windows.Input;

namespace SolutionRevitAPI.WPF.ViewModels
{
    public class TagModeVM
    {
        public bool TagAllMode { get; set; }
        public ICommand TagCommand { get; set; }
        public ICommand TagAllCommand { get; set; }

        public TagModeVM()
        {
            TagAllMode = false;
            TagCommand = new RelayCommand(Tag, CanTag);
            TagAllCommand = new RelayCommand(TagAll, CanTagAll);
        }

        private bool CanTagAll(object obj)
        {
            return true;
        }

        private void TagAll(object obj)
        {
            Window window = obj as Window;
            TagAllMode = true;
            window.Close();
        }

        private bool CanTag(object obj)
        {
            return true;
        }

        private void Tag(object obj)
        {
            Window window = obj as Window;
            TagAllMode = false;
            window.Close();
        }
    }
}