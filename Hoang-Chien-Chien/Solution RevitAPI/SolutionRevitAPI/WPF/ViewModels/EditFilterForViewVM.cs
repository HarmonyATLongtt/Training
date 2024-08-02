using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using SolutionRevitAPI.WPF.Model;
using SolutionRevitAPI.WPF.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SolutionRevitAPI.WPF.ViewModels
{
    [Transaction(TransactionMode.Manual)]
    public class EditFilterForViewVM : INotifyPropertyChanged
    {
        private bool isSelected;

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

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

        private View selectedView;

        public View SelectedView
        {
            get => selectedView;
            set
            {
                selectedView = value;
                OnPropertyChanged(nameof(SelectedView));
                LoadLstFilter(SelectedView);
            }
        }

        private ObservableCollection<EditFilterForView_M> lstFilter;

        public ObservableCollection<EditFilterForView_M> LstFilter
        {
            get => lstFilter;
            set
            {
                lstFilter = value;
                OnPropertyChanged(nameof(LstFilter));
            }
        }

        private EditFilterForView_M selectedFil;

        public EditFilterForView_M SelectedFil
        {
            get => selectedFil;
            set
            {
                selectedFil = value;
                OnPropertyChanged(nameof(SelectedFil));
            }
        }

        public bool IsSave { get; set; }
        public Document Doc { get; set; }
        public Window CurrentWindow { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ApplyCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public EditFilterForViewVM()
        {
            LstView = new ObservableCollection<View>();
            lstFilter = new ObservableCollection<EditFilterForView_M>();
            IsSave = false;
            AddCommand = new RelayCommand(Add, CanAdd);
            DeleteCommand = new RelayCommand(Delete, CanDelete);
            ApplyCommand = new RelayCommand(Apply, CanApply);
        }

        private bool CanApply(object obj)
        {
            return LstFilter.Count() > 0;
        }

        private void Apply(object obj)
        {
            using (Transaction trans = new Transaction(Doc, "Set Enable Filter For View"))
            {
                trans.Start();
                foreach (var filter in LstFilter)
                {
                    SelectedView.SetIsFilterEnabled(filter.Fil.Id, filter.IsSelected);
                }
                trans.Commit();
            }
        }

        private bool CanDelete(object obj)
        {
            return SelectedFil != null;
        }

        private void Delete(object obj)
        {
            using (Transaction tx = new Transaction(Doc, "Remove Filter"))
            {
                tx.Start();
                var filterId = SelectedFil.Fil.Id;
                SelectedView.RemoveFilter(filterId);
                Doc.Delete(filterId);
                LoadLstFilter(SelectedView);
                tx.Commit();
            }
        }

        private bool CanAdd(object obj)
        {
            return SelectedView != null;
        }

        private void Add(object obj)
        {
            HashSet<ElementId> categoryIds = new HashSet<ElementId>();
            List<Element> list = new FilteredElementCollector(Doc, SelectedView.Id).WhereElementIsNotElementType().ToElements().ToList();
            foreach (Element element in list)
            {
                Category category = element.Category;
                if (category != null)
                {
                    categoryIds.Add(category.Id);
                }
            }
            // Tạo danh sách các category từ danh sách ElementId
            ObservableCollection<CreatFilterForView_M> lstCategories = new ObservableCollection<CreatFilterForView_M>();
            foreach (ElementId categoryId in categoryIds)
            {
                Category category = Category.GetCategory(Doc, categoryId);
                if (category != null && category.IsVisibleInUI)
                {
                    lstCategories.Add(new CreatFilterForView_M() { Cat = category });
                }
            }
            CreatFilterForView creatWindow = new CreatFilterForView();
            CreatFilterForView_VM filterForCurrentView_VM = new CreatFilterForView_VM();
            CreatFilterForView_VM creatViewModel = filterForCurrentView_VM;
            creatViewModel.Doc = Doc;
            creatViewModel.LstCategory = lstCategories;
            creatWindow.DataContext = creatViewModel;
            CurrentWindow.Hide();
            creatWindow.ShowDialog();
            categoryIds.Clear();
            foreach (var item in creatViewModel.LstCategory)
            {
                if (item.IsSelected)
                    categoryIds.Add(item.Cat.Id);
            }
            if (creatViewModel.IsSave && categoryIds.Count() > 0)
            {
                using (Transaction trans = new Transaction(Doc, "Creat Filter"))
                {
                    trans.Start();
                    ParameterFilterElement filter = ParameterFilterElement.Create(Doc, creatViewModel.FilterName, categoryIds);

                    // Tạo bộ màu
                    var colorDictionary = typeof(System.Drawing.Color).GetProperties().ToDictionary(p => p.Name, p => System.Drawing.Color.FromName(p.Name));
                    // Tạo một đối tượng để lưu trữ FillPatternElement kiểu Solid
                    FillPatternElement solidFillPattern = new FilteredElementCollector(Doc).OfClass(typeof(FillPatternElement)).ToElements().Cast<FillPatternElement>()
                                                            .Where(p => p.GetFillPattern().IsSolidFill).FirstOrDefault();
                    // Tạo một đối tượng để lưu trữ FillLineElement kiểu Solid
                    FilteredElementCollector collector = new FilteredElementCollector(Doc);
                    collector.OfClass(typeof(LinePatternElement));

                    // Duyệt qua tất cả các LinePatternElement và tìm phần tử với tên "Solid"
                    LinePatternElement solidLinePattern = new FilteredElementCollector(Doc).OfClass(typeof(LinePatternElement)).ToElements().Cast<LinePatternElement>().ToList()
                                                            .Where(p => p.GetLinePattern().Name.Contains("Center")).FirstOrDefault();

                    OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                    if (!string.IsNullOrEmpty(creatViewModel.ColorLine))
                    {
                        if (colorDictionary.TryGetValue(creatViewModel.ColorLine, out System.Drawing.Color systemColor))
                        {
                            Autodesk.Revit.DB.Color revitColor = new Autodesk.Revit.DB.Color(systemColor.R, systemColor.G, systemColor.B);
                            ogs.SetProjectionLineColor(revitColor);
                            ogs.SetProjectionLinePatternId(solidLinePattern.Id);
                        }
                    }
                    if (!string.IsNullOrEmpty(creatViewModel.PatternColor))
                    {
                        if (colorDictionary.TryGetValue(creatViewModel.PatternColor, out System.Drawing.Color systemColor))
                        {
                            Autodesk.Revit.DB.Color revitColor = new Autodesk.Revit.DB.Color(systemColor.R, systemColor.G, systemColor.B);
                            ogs.SetSurfaceForegroundPatternColor(revitColor);
                            ogs.SetSurfaceForegroundPatternId(solidFillPattern.Id);
                        }
                    }
                    if (creatViewModel.LineWeight > 0)
                    {
                        ogs.SetProjectionLineWeight(creatViewModel.LineWeight);
                    }
                    ogs.SetSurfaceForegroundPatternId(solidFillPattern.Id);
                    if (creatViewModel.Transparency > 0)
                    {
                        if (creatViewModel.Transparency > 100) creatViewModel.Transparency = 100;
                        ogs.SetProjectionLineWeight(creatViewModel.Transparency);
                    }
                    ogs.SetHalftone(creatViewModel.Halftone);

                    // Thêm filter vào view
                    SelectedView.AddFilter(filter.Id);
                    SelectedView.SetFilterVisibility(filter.Id, creatViewModel.IsVisibleChecked);
                    // Áp dụng các bộ lọc thiết lập cho filter
                    SelectedView.SetFilterOverrides(filter.Id, ogs);

                    LoadLstFilter(SelectedView);
                    trans.Commit();
                }
            }
            CurrentWindow.ShowDialog();
        }

        private void LoadLstFilter(View view)
        {
            var lst = view.GetFilters();
            LstFilter.Clear();
            foreach (var filterId in lst)
            {
                // Kiểm tra xem filter có được bật hay không
                bool isEnabled = view.GetIsFilterEnabled(filterId);
                LstFilter.Add(new EditFilterForView_M() { IsSelected = isEnabled, Fil = Doc.GetElement(filterId) as FilterElement });
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}