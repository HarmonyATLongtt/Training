using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SolutionRevitAPI.Commands.ExternalEvents;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SolutionRevitAPI.WPF.ViewModels
{
    public class EditSheetVM : INotifyPropertyChanged
    {
       
        private ObservableCollection<View> lstView;

        public ObservableCollection<View> LstView
        {
            get => lstView;
            set
            {
                lstView = value;
                OnPropertyChanged(nameof(LstView));
            }
        }

        private ObservableCollection<ViewSheet> lstSheet;

        public ObservableCollection<ViewSheet> LstSheet
        {
            get => lstSheet;
            set
            {
                lstSheet = value;
                OnPropertyChanged(nameof(LstSheet));
            }
        }

        private string sheetName;

        public string SheetName
        {
            get => sheetName;
            set
            {
                sheetName = value;
                OnPropertyChanged(nameof(SheetName));
            }
        }

        private string sheetNumber;

        public string SheetNumber
        {
            get => sheetNumber;
            set
            {
                sheetNumber = value;
                OnPropertyChanged(nameof(SheetNumber));
            }
        }

        private ViewSheet selectedSheet;

        public ViewSheet SelectedSheet
        {
            get => selectedSheet;
            set
            {
                selectedSheet = value;
                OnPropertyChanged(nameof(SelectedSheet));
            }
        }

        private View selectedViewPlan;

        public View SelectedViewPlan
        {
            get => selectedViewPlan;
            set
            {
                selectedViewPlan = value;
                OnPropertyChanged(nameof(SelectedViewPlan));
            }
        }
        public ExternalEvent ExternalEvent { get; set; }
        public EditSheetEEH Handler { get; set; }
        public FamilySymbol TitleBlock { get; set; }
        public Document Doc { get; set; }
        public ICommand CreatCommand { get; set; }
        public ICommand ApplyCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public EditSheetVM()
        {
            LstSheet = new ObservableCollection<ViewSheet>();
            LstView = new ObservableCollection<View>();

            CreatCommand = new RelayCommand(Creat, CanCreat);

            ApplyCommand = new RelayCommand(Apply, CanApply);
        }

        private bool CanCreat(object obj)
        {
            if (string.IsNullOrEmpty(SheetNumber) || string.IsNullOrEmpty(SheetName))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void Creat(object obj)
        {
            var viewSheet = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Sheets).WhereElementIsNotElementType()
                                .ToElements().Cast<ViewSheet>().ToList()
                                .Where(p => p.SheetNumber == SheetNumber).FirstOrDefault();
            if (viewSheet != null)
            {
                MessageBox.Show("Sheet Number này đã tồn tại, vui lòng chọn Sheet Number khác!", "Warning");
                return;
            }
            Handler.TitleBlock = TitleBlock;
            Handler.SheetName = SheetNumber;
            Handler.SheetNumber = SheetNumber;
            Handler.LstSheet = LstSheet;
            Handler.Mode = 1;
            ExternalEvent.Raise();
            SheetName = string.Empty;
            SheetNumber = string.Empty;
        }

        private bool CanApply(object obj)
        {
            if ((SelectedSheet == null) || (SelectedViewPlan == null))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void Apply(object obj)
        {
            Handler.SelectedSheet = SelectedSheet;
            Handler.SelectedViewPlan = SelectedViewPlan;
            Handler.Mode = 2;
            ExternalEvent.Raise();
            SelectedViewPlan = null;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}