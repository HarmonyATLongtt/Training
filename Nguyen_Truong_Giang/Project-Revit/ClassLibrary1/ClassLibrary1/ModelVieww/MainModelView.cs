using Autodesk.Revit.DB;
using ClassLibrary1.Model;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ClassLibrary1.ModelVieww
{
    internal class MainModelView : BaseModelView
    {
        private SelectModel _model;

        private ObservableCollection<string> _level;

        public ObservableCollection<string> listLevel
        {
            get => _level;
            set
            {
                _level = value;
                OnPropertyChanged(nameof(listLevel));
            }
        }

        public ICommand LoadLevelCommand { get; set; }

        public MainModelView(SelectModel model)
        {
            _model = model;

            listLevel = new ObservableCollection<string>();

            LoadLevelCommand = new RelayCommand<object>(GetLevelInView);
        }

        private void GetLevelInView(object parameter)
        {
            GetLevel(_model.Doc);
        }

        public void GetLevel(Document doc)
        {
            FilteredElementCollector levelCollector = new FilteredElementCollector(doc);

            ICollection<Element> levels = levelCollector.OfCategory(BuiltInCategory.OST_Levels)
                .OfClass(typeof(Level)).ToElements();

            foreach (Element levelElement in levels)
            {
                Level level = levelElement as Level;
                if (level != null)
                {
                    listLevel.Add(level.Name);
                }
            }
        }
    }
}