using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WPF_Sample.Converters;
using WPF_Sample.Models;
using WPF_Sample.Utils;
using WPF_Sample.Views;
using System.Collections.Generic;

namespace WPF_Sample.ViewModels
{
    public class ClassViewModel : BindableBase
    {
        #region Propeties

        private bool _allowApplyChange;
        public int Id { get; set; }
        public BitmapImage MainIcon { get; }
        private string _textSearch;

        /// <summary>
        /// Text search for name of Student
        /// </summary>
        public string TextSearch
        {
            get { return _textSearch; }
            set
            {
                _textSearch = value;
                OnPropertyChanged(nameof(StudentViewModels));
            }
        }

        private ClassModel _classModel;
        public ClassModel ClassModel { get => _classModel; private set => _classModel = value; }

        public string ClassName
        {
            get
            {
                return _classModel.ClassName;
            }
            set
            {
                _classModel.ClassName = value;
                OnPropertyChanged(nameof(ClassName));
            }
        }

        /// <summary>
        /// The teacher name , who manages the class
        /// </summary>
        public string FormTeacherName
        {
            get
            {
                return _classModel.FormTeacherName;
            }
            set
            {
                _classModel.FormTeacherName = value;
                OnPropertyChanged(nameof(FormTeacherName));
            }
        }

        private ObservableCollection<StudentViewModel> _studentViewModels;
        /// <summary>
        /// Collection of StudentViewModel
        /// </summary>

        public ObservableCollection<StudentViewModel> StudentViewModels
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_textSearch))
                {
                    IEnumerable<StudentViewModel>? studentFilterViewModels = _studentViewModels.Where(item => item.Name.ToLower(System.Globalization.CultureInfo.CurrentCulture).StartsWith(_textSearch.ToLower()));
                    return new ObservableCollection<StudentViewModel>(studentFilterViewModels);
                    /* for experiment
                     https://wpf-tutorial.com/listview-control/listview-filtering/
                    var view = CollectionViewSource.GetDefaultView(_studentViewModels);
                    view.Filter = (item) => ((StudentViewModel)item).Name.ToLower().StartsWith(_textSearch.ToLower());
                    return null;
                    */
                }
                return _studentViewModels;
            }
            set
            {
                _studentViewModels = value;
                OnPropertyChanged(nameof(StudentViewModels));
            }
        }

        private bool _allowEditableAll;

        public bool AllowEditableAll
        {
            get => _allowEditableAll;
            set
            {
                SetProperty(ref _allowEditableAll, value);
                AllowEditableAllInvoke(value);
            }
        }

        private bool _allowEditable;

        public bool AllowEditable
        {
            get => _allowEditable;
            set => SetProperty(ref _allowEditable, value);
        }

        #endregion Propeties

        #region Commands

        public ICommand ShowDetailClassCmd { get; set; }
        public ICommand AddStudentCmd { get; set; }
        public ICommand DeleteStudentCmd { get; set; }
        public ICommand ExportExcelCmd { get; set; }

        public ICommand CloseWndCmd { get; set; }

        public ICommand ApplyChangeCmd { get; set; }

        #endregion Commands

        #region Constructors

        /// <summary>
        /// Create new instance of ClassViewModel
        /// </summary>
        /// <param name="classModel"></param>
        public ClassViewModel(ClassModel classModel)
        {
            _classModel = classModel;
            InitStudentData();
            InitCommands();
            MainIcon = MainViewModel.MainIcon;
            Id = IdGenerator.NextId;
        }

        #endregion Constructors

        #region Methods

        private void InitStudentData()
        {
            StudentViewModels = new ObservableCollection<StudentViewModel>();
            foreach (var item in _classModel.StudentModels)
            {
                StudentViewModels.Add(new StudentViewModel(item));
            }
        }

        /// <summary>
        ///Init commands
        /// </summary>
        private void InitCommands()
        {
            ShowDetailClassCmd = new RelayCommand<object>(ShowDetailInvoke);
            AddStudentCmd = new RelayCommand<object>(AddStudentInvoke);
            DeleteStudentCmd = new RelayCommand<object>(DeleteStudentInvoke);
            ExportExcelCmd = new RelayCommand<object>(ExportExcelInvoke);
            CloseWndCmd = new RelayCommand<object>(CloseWndInvoke);
            ApplyChangeCmd = new RelayCommand<object>(ApplyChangeCmdInvoke);
        }

        public ClassViewModel DeepCopy()
        {
            ClassModel copyClass = _classModel.DeepCopy();
            ClassViewModel newClassVM = new ClassViewModel(copyClass);
            return newClassVM;
        }

        /// <summary>
        /// Allow selected student can editable
        /// </summary>
        /// <param name="value"></param>
        private void AllowEditableAllInvoke(bool value)
        {
            foreach (StudentViewModel studenVM in StudentViewModels)
            {
                studenVM.AllowEditable = value;
            }
        }

        /// <summary>
        /// Add a student into the class
        /// </summary>
        private void AddStudentInvoke(object obj)
        {
            if (obj != null && obj is ListView lsv)
            {
                StudentModel student = StudentModel.CreateDefault();
                StudentViewModel studentViewModel = new StudentViewModel(student);
                studentViewModel.AllowEditable = true;
                StudentViewModels.Add(studentViewModel);
                _classModel.StudentModels.Add(student);
            }
        }

        /// <summary>
        /// Delete selected students
        /// </summary>
        /// <param name="obj"></param>
        private void DeleteStudentInvoke(object obj)
        {
            if (obj != null && obj is ListView lsv
                && StudentViewModels != null && StudentViewModels.Count > 0)
            {
                var selected = lsv.SelectedItems;
                if (selected == null || selected.Count == 0)
                {
                    return;
                }
                MessageBoxResult result = MessageBox.Show($"Are you sure delete {selected.Count} student{(selected.Count == 1 ? "" : "s")}", "Warning", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                    for (int i = selected.Count - 1; i >= 0; i--)
                    {
                        if (selected[i] is StudentViewModel studentVM)
                        {
                            StudentViewModels.Remove(studentVM);
                            _classModel.StudentModels.Remove(studentVM.StudentModel);
                        }
                    }
            }
        }

        /// <summary>
        /// Export current class to excel file
        /// </summary>
        /// <param name="obj"></param>
        private void ExportExcelInvoke(object obj)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            saveDialog.Title = "Save data";
            saveDialog.DefaultExt = ".xlsx";
            if (saveDialog.ShowDialog() == true)
            {
                if (File.Exists(saveDialog.FileName) &&
                  Utils.Utils.FileInUse(saveDialog.FileName))
                {
                    MessageBox.Show("Can not export cause the file is using by another app", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DataTable studentTable = new DataTable(ClassName);
                DataColumn index = new DataColumn();
                index.ColumnName = "Index";
                index.DataType = typeof(int);
                index.Caption = "STT";
                studentTable.Columns.Add(index);

                DataColumn name = new DataColumn();
                name.ColumnName = "Name";
                name.DataType = typeof(string);
                name.Caption = "Tên";
                studentTable.Columns.Add(name);

                DataColumn birthday = new DataColumn();
                birthday.ColumnName = "birthday";
                birthday.DataType = typeof(string);
                birthday.Caption = "Ngày Sinh";
                studentTable.Columns.Add(birthday);

                DataColumn age = new DataColumn();
                age.ColumnName = "Age";
                age.DataType = typeof(int);
                age.Caption = "Tuổi";
                studentTable.Columns.Add(age);

                DataColumn gender = new DataColumn();
                gender.ColumnName = "Gender";
                gender.DataType = typeof(string);
                gender.Caption = "Giới tính";
                studentTable.Columns.Add(gender);
                int stt = 1;
                foreach (StudentViewModel studentVM in StudentViewModels)
                {
                    studentTable.Rows.Add(stt, studentVM.Name, studentVM.Birthday.ToShortDateString(), studentVM.Age, EnumToStringConverter.GetEnumDescription(studentVM.Gender));
                    stt++;
                }
                ExcelUtils.Export(studentTable, saveDialog.FileName);
                MessageBox.Show("Export done !");
            }
        }

        /// <summary>
        /// Close current window
        /// </summary>
        /// <param name="obj"></param>
        private void CloseWndInvoke(object obj)
        {
            MessageBox.Show("Close");
            if (obj != null && obj is Window wnd)
            {
                _allowApplyChange = false;
                TextSearch = string.Empty;
                wnd.Close();
                wnd.Owner.Show();
            }
        }

        /// <summary>
        /// Apply data change
        /// </summary>
        /// <param name="obj"></param>
        private void ApplyChangeCmdInvoke(object obj)
        {
            MessageBox.Show("Apply");

            if (obj != null && obj is Window wnd)
            {
                _allowApplyChange = true;
                TextSearch = string.Empty;
                wnd.Close();
                wnd.Owner.Show();
              
            }
        }

        /// <summary>
        /// Show detail class
        /// </summary>
        /// <param name="obj"></param>
        private void ShowDetailInvoke(object obj)
        {
            ClassViewModel temVM = new ClassViewModel(_classModel.DeepCopy());
            ClassView classView = new ClassView(temVM);
            Window owner = Window.GetWindow(obj as DependencyObject);
            classView.Owner = owner;
            owner.Hide();

            classView.ShowDialog();
            
            if (classView.DataContext is ClassViewModel vm_Main && vm_Main._allowApplyChange)
            {
                StudentViewModels = vm_Main.StudentViewModels;
                _classModel = vm_Main._classModel;
            }
        }

        #endregion Methods
    }
}