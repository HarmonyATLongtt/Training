﻿using Autodesk.Revit.DB;
using CreateFamily.Model;
using CreateFamily.Models;
using CreateFamily.ModelView;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace CreateFamily.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        private SelectModel _model;

        private bool _isAllChecked;

        private ObservableCollection<FamilySymbol> _structuralColumns;

        public ObservableCollection<FamilySymbol> StructuralColumns
        {
            get { return _structuralColumns; }
            set
            {
                _structuralColumns = value;
                OnPropertyChanged(nameof(StructuralColumns));
            }
        }

        private FamilySymbol _selectedColumnIndex;

        public FamilySymbol SelectedColumnIndex
        {
            get { return _selectedColumnIndex; }
            set
            {
                _selectedColumnIndex = value;
                OnPropertyChanged(nameof(SelectedColumnIndex));

                if (SelectedColumnIndex != null)
                {
                    LevelVisibility = true;
                    PropertyButton = true;
                }
            }
        }

        public bool IsAllChecked
        {
            get => _isAllChecked;
            set
            {
                _isAllChecked = value;
                OnPropertyChanged(nameof(IsAllChecked));

                // Cập nhật trạng thái IsChecked cho tất cả các mục
                foreach (var item in intersection)
                {
                    item.IsChecked = _isAllChecked;
                }
            }
        }

        private ObservableCollection<Level> _listLevel;

        public ObservableCollection<Level> ListLevel
        {
            get => _listLevel;
            set
            {
                _listLevel = value;
                OnPropertyChanged(nameof(ListLevel));
            }
        }

        private Level _selectlevel;

        public Level SelectLevel
        {
            get => _selectlevel;
            set
            {
                _selectlevel = value;
                OnPropertyChanged(nameof(SelectLevel));
            }
        }

        private Level _selecToptlevel;

        public Level SelectTopLevel
        {
            get => _selecToptlevel;
            set
            {
                _selecToptlevel = value;
                OnPropertyChanged(nameof(SelectTopLevel));
            }
        }

        private double _offsetValue = 0;

        public double OffsetValue
        {
            get => _offsetValue;
            set
            {
                _offsetValue = value;
                OnPropertyChanged(nameof(OffsetValue));
            }
        }

        private bool _propertyButton = false;

        public bool PropertyButton
        {
            get { return _propertyButton; }
            set
            {
                _propertyButton = value;
                OnPropertyChanged(nameof(PropertyButton));
            }
        }

        private bool _labelVisibility = true;

        public bool LabelVisibility
        {
            get { return _labelVisibility; }
            set
            {
                _labelVisibility = value;
                OnPropertyChanged(nameof(LabelVisibility));
            }
        }

        private bool _levelVisibility = false;

        public bool LevelVisibility
        {
            get { return _levelVisibility; }
            set
            {
                _levelVisibility = value;
                OnPropertyChanged(nameof(LevelVisibility));
            }
        }

        private bool _deleteAllColumn;

        public bool DeleteAllColumn
        {
            get { return _deleteAllColumn; }
            set
            {
                _deleteAllColumn = value;
                OnPropertyChanged(nameof(DeleteAllColumn));
            }
        }

        private string _labelContent;

        public string LabelContent
        {
            get { return _labelContent; }
            set
            {
                _labelContent = value;
                OnPropertyChanged(nameof(LabelContent));
            }
        }

        public FamilySymbol _setFamilySymbol;

        public FamilySymbol SetFamilySymbol
        {
            get => _setFamilySymbol;
            set
            {
                _setFamilySymbol = value;
                OnPropertyChanged(nameof(SetFamilySymbol));
            }
        }

        private ObservableCollection<ItemCheckViewModel> _intersection;

        public ObservableCollection<ItemCheckViewModel> intersection
        {
            get => _intersection;
            set
            {
                _intersection = value;
                OnPropertyChanged(nameof(intersection));
            }
        }

        public ICommand CreateFamilyCommand { get; set; }
        public ICommand ImportFamilyCommand { get; set; }

        public MainViewModel(SelectModel model)
        {
            _model = model;

            Document doc = _model.Doc;

            CreateFamilyCommand = new RelayCommand<FamilySymbol>(CreateFamily);

            ImportFamilyCommand = new RelayCommand<Document>(ImportRevitFile);

            LabelContent = "Vui lòng chọn Family.";

            GetLevel(_model.Doc);

            GetCreatePoint(_model.Doc);

            GetStructuralColumns(_model.Doc);

            SelectLevel = ListLevel.FirstOrDefault();

            SelectTopLevel = ListLevel.FirstOrDefault();
        }

        public void CreateFamily(FamilySymbol familySymbol)
        {
            Document doc = _model.Doc;

            familySymbol = SelectedColumnIndex;

            if (DeleteAllColumn == true)
            {
                DeleleColumnInView(doc);
            }

            CreateFamilyInstances(doc, familySymbol);
        }

        public void GetLevel(Document doc)
        {
            FilteredElementCollector levelCollector = new FilteredElementCollector(doc);

            ICollection<Element> levels = levelCollector.OfCategory(BuiltInCategory.OST_Levels)
                .OfClass(typeof(Level)).ToElements();

            ListLevel = new ObservableCollection<Level>();

            foreach (Element levelElement in levels)
            {
                Level level = levelElement as Level;
                if (level != null)
                {
                    ListLevel.Add(level);
                }
            }
        }

        private List<XYZ> GetCreatePoint(Document doc)
        {
            //lay toan bo grid trong project
            List<Grid> grids = GetAllGrid(doc);

            //tim giao diem cua 2 grid
            Dictionary<string, XYZ> dic = new Dictionary<string, XYZ>();
            for (int i = 0; i < grids.Count; i++)
            {
                for (int j = i + 1; j < grids.Count; j++)
                {
                    XYZ p = GetIntersection(grids[i], grids[j]);
                    if (p != null)
                        dic.Add(grids[i].Name + "-" + grids[j].Name, p);
                }
            }
            if (dic.Count > 0)
            {
                return new List<XYZ> { SelectMultiIntersect(doc, dic) };
            }
            else
            {
                showHideLabel("Không tìm thấy giao điểm.");
                return null;
            }
        }

        private List<Grid> GetAllGrid(Document doc)
        {
            return new FilteredElementCollector(doc).WhereElementIsNotElementType()
                                                    .OfCategory(BuiltInCategory.OST_Grids)
                                                    .OfClass(typeof(Grid))
                                                    .Cast<Grid>()
                                                    .ToList();
        }

        private XYZ GetIntersection(Grid g1, Grid g2)
        {
            if (g1 != null && g2 != null)
            {
                var ret = g1.Curve.Intersect(g2.Curve, out IntersectionResultArray result);
                if (ret == SetComparisonResult.Overlap && result.Size == 1)
                {
                    return result.get_Item(0).XYZPoint;
                }
            }
            return null;
        }

        private string ShowOpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Family Files (*.rfa)|*.rfa";
            openFileDialog.Title = "Chọn file Revit để import";

            // Hiển thị hộp thoại chọn file
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }

            return null;
        }

        private void ImportRevitFile(Document doc)
        {
            doc = _model.Doc;

            string filePath = ShowOpenFileDialog();
            // Hiển thị hộp thoại chọn file

            if (filePath != null)
            {
                using (Transaction trans = new Transaction(doc, "import File to REVIT"))
                {
                    trans.Start();
                    Family family = null;
                    bool load = LoadFamily(doc, filePath, out family);
                    if (load && family != null)
                    {
                        FamilySymbol familySymbol = GetActiveFamilySymbol(doc, family);
                        ActivateFamilySymbol(familySymbol);

                        _setFamilySymbol = familySymbol;

                        if (familySymbol != null)
                        {
                            LabelVisibility = false;
                            StructuralColumns.Add(familySymbol);
                            showHideLabel("Family đã được tải thành công.");

                            if (!File.Exists(filePath))
                            {
                                LabelVisibility = false;
                                showHideLabel("Đường dẫn file không hợp lệ.");
                                return;
                            }
                        }

                        trans.Commit();
                    }
                    else
                    {
                        LabelVisibility = false;

                        showHideLabel("Family đã tồn tại.");
                    }
                }
            }
        }

        private bool LoadFamily(Document doc, string filePath, out Family family)
        {
            family = null;
            bool load = doc.LoadFamily(filePath, out family);

            return load;
        }

        private FamilySymbol GetActiveFamilySymbol(Document doc, Family family)
        {
            return doc.GetElement(family.GetFamilySymbolIds().First()) as FamilySymbol;
        }

        private void ActivateFamilySymbol(FamilySymbol familySymbol)
        {
            if (!familySymbol.IsActive)
            {
                familySymbol.Activate();
            }
        }

        private void CreateFamilyInstances(Document doc, FamilySymbol familySymbol)
        {
            using (Transaction trans = new Transaction(doc, "Create Family"))
            {
                trans.Start();

                if (intersection != null)
                {
                    if (intersection.Any(x => x.IsChecked == true))
                    {
                        foreach (var item in intersection)
                        {
                            if (item.IsChecked)
                            {
                                XYZ newPoint = item._model.Point;

                                familySymbol = SelectedColumnIndex;

                                ActivateFamilySymbol(familySymbol);

                                if (SelectTopLevel.Equals(SelectLevel) == false)
                                {
                                    CreateColumns(doc, newPoint, familySymbol);

                                    LabelVisibility = false;

                                    showHideLabel("Đã đặt Columns thành công.");
                                }
                                else if (SelectTopLevel.Equals(SelectLevel) && OffsetValue == 0)
                                {
                                    LabelVisibility = false;
                                    showHideLabel("Base Level và Top Level giống nhau thì Base Offset phải khác 0.");
                                }
                                else if (OffsetValue != 0 && SelectTopLevel.Equals(SelectLevel))
                                {
                                    LabelVisibility = false;

                                    CreateColumns(doc, newPoint, familySymbol);

                                    showHideLabel("Đã đặt Columns thành công.");
                                }
                            }
                        }
                    }
                    else if (intersection.Any(x => x.IsChecked == false))
                    {
                        LabelVisibility = false;
                        showHideLabel("Vui lòng chọn giao điểm đặt Family.");
                    }

                    if (SelectLevel == null)
                    {
                        LabelVisibility = false;
                        showHideLabel("Vui lòng chọn Level.");
                    }
                }
                else
                {
                    LabelVisibility = false;
                    showHideLabel("Vui lòng thêm các đối tượng Grid để tạo giao điểm.");
                }

                trans.Commit();
            }
        }

        public void CreateColumns(Document doc, XYZ newPoint, FamilySymbol familySymbol)
        {
            FamilyInstance instance = doc.Create.NewFamilyInstance(newPoint, familySymbol, SelectLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            doc.Regenerate(); //tạo lại các yếu tố và đối tượng

            Parameter paraTopLevel = instance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);

            paraTopLevel.Set(SelectTopLevel.Id);

            doc.Regenerate(); //tạo lại các yếu tố và đối tượng

            Parameter para = instance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);

            para.Set(MillimetersToFeet(OffsetValue));
        }

        public XYZ SelectMultiIntersect(Document doc, Dictionary<string, XYZ> points)
        {
            XYZ selectedPoint = null;

            ObservableCollection<ItemCheckViewModel> interSect = new ObservableCollection<ItemCheckViewModel>();

            foreach (var dic in points)
            {
                string option = $"Giao điểm: " + dic.Key;
                ItemCheckModel itemModel = new ItemCheckModel();
                itemModel.Name = option;

                itemModel.Point = dic.Value;
                ItemCheckViewModel itemViewModel = new ItemCheckViewModel(itemModel);

                interSect.Add(itemViewModel);

                intersection = interSect;
                selectedPoint = itemModel.Point;
            }
            return selectedPoint;
        }

        public string showHideLabel(string text)
        {
            LabelContent = text;

            LabelVisibility = true;

            return LabelContent;
        }

        public ObservableCollection<FamilySymbol> GetStructuralColumns(Document doc)
        {
            StructuralColumns = new ObservableCollection<FamilySymbol>();

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> familySymbols = collector
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .ToElements();

            foreach (Element elm in familySymbols)
            {
                FamilySymbol familySymbol = elm as FamilySymbol;
                if (familySymbol != null)
                {
                    StructuralColumns.Add(familySymbol);
                }
            }

            return StructuralColumns;
        }

        public double MillimetersToFeet(double feet)
        {
            //UnitUtils.ConvertFromInternalUnit
            const double millimetersPerFoot = 0.00328084;
            return feet * millimetersPerFoot;
        }

        public void DeleleColumnInView(Document doc)
        {
            using (Transaction trans = new Transaction(doc, "Delete Columns"))
            {
                trans.Start();

                List<ViewSchedule> columns = new List<ViewSchedule>();

                // Lọc các phần tử cột trên View
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                ICollection<Element> elements = collector.OfCategory(BuiltInCategory.OST_StructuralColumns)
                    .WhereElementIsNotElementType()
                    .ToElements();

                // Xóa từng cột
                foreach (var column in elements)
                {
                    doc.Delete(column.Id);
                }

                trans.Commit();
            }
        }
    }
}