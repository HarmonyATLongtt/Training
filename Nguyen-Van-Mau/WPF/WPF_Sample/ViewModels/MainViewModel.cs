using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WPF_Sample.Models;
using WPF_Sample.Utils;

namespace WPF_Sample.ViewModels
{
    public class MainViewModel : BindableBase
    {
        #region Proprties

        private string? _filePath = null;
        private Window _window;
        private const string _appName = "Student Management";
        private MainModel _mainModel;
        public BitmapImage NewImage => Utils.Utils.BitmapToImageSource(Properties.Resources.NewIcon);
        public BitmapImage OpenImage => Utils.Utils.BitmapToImageSource(Properties.Resources.OpenIcon);
        public BitmapImage SaveImage => Utils.Utils.BitmapToImageSource(Properties.Resources.SaveIcon);
        public BitmapImage SaveAsImage => Utils.Utils.BitmapToImageSource(Properties.Resources.SaveAsIcon);
        public BitmapImage CloseImage => Utils.Utils.BitmapToImageSource(Properties.Resources.CloseIcon);
        public static BitmapImage MainIcon => Utils.Utils.BitmapToImageSource(Properties.Resources.ManagerIcon.ToBitmap());

        private ObservableCollection<ClassViewModel> _classViewModels;

        public ObservableCollection<ClassViewModel> ClassViewModels
        {
            get => _classViewModels;

            set
            {
                if (value != null && _classViewModels != value)
                {
                    _classViewModels = value;
                    OnPropertyChanged(nameof(ClassViewModels));
                    //_mainModel.ClassModels= value.
                }
            }
        }

        #endregion Proprties

        #region Commands

        public ICommand NewCmd { get; set; }
        public ICommand OpenCmd { get; set; }
        public ICommand SaveCmd { get; set; }
        public ICommand SaveAsCmd { get; set; }
        public ICommand CloseWndCmd { get; set; }

        public ICommand AddClassCmd { get; set; }
        public ICommand DeleteClassCmd { get; set; }

        #endregion Commands

        #region Constructor

        public MainViewModel(MainModel mainModel, Window wnd)
        {
            _mainModel = mainModel;
            _window = wnd;

            //InitImage();
            InitClassData();
            InitCommands();
        }

        #endregion Constructor

        #region Method

        private void InitClassData()
        {
            ClassViewModels = new ObservableCollection<ClassViewModel>();
            ClassViewModels.CollectionChanged += ClassViewModels_CollectionChanged;
            foreach (var classModel in _mainModel.ClassModels)
            {
                ClassViewModel classVM = new ClassViewModel(classModel);

                ClassViewModels.Add(classVM);
            }
        }

        /// <summary>
        /// Method for experiment
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClassViewModels_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // MessageBox.Show("dsddsf");
        }

        private void InitCommands()
        {
            NewCmd = new RelayCommand<object>(NewInvoke);
            OpenCmd = new RelayCommand<object>(OpenInvoke);
            SaveCmd = new RelayCommand<object>(SaveInvoke);
            SaveAsCmd = new RelayCommand<object>(SaveAsCmdvoke);
            CloseWndCmd = new RelayCommand<object>(CloseWndInvoke);
            AddClassCmd = new RelayCommand<object>(AddClassInvoke);
            DeleteClassCmd = new RelayCommand<object>(DeleteClassInvoke);
        }

        /// <summary>
        /// Save as data to json file
        /// </summary>
        /// <param name="obj"></param>
        private void SaveAsCmdvoke(object obj)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Json files (*.json)|*.json|All files (*.*)|*.*";
            saveDialog.Title = "Save data";

            if (saveDialog.ShowDialog() == true)
            {
                SaveFile(saveDialog.FileName);
            }
        }

        /// <summary>
        /// Save data to json file
        /// </summary>
        /// <param name="obj"></param>
        private void SaveInvoke(object obj)
        {
            if (_filePath == null || !File.Exists(_filePath))
            {
                SaveAsCmdvoke(obj);
            }
            else
            {
                SaveFile(_filePath);
            }
        }

        /// <summary>
        /// Save  data to json file
        /// </summary>
        /// <param name="obj"></param>
        private void SaveFile(string filePath)
        {
            ObservableCollection<ClassModel> classes = new ObservableCollection<ClassModel>();
            foreach (var classVM in ClassViewModels)
            {
                var newClassVM = new ClassModel(classVM.ClassName, classVM.FormTeacherName);
                foreach (StudentViewModel? studentVM in classVM.StudentViewModels)
                {
                    newClassVM.StudentModels.Add(studentVM.StudentModel);
                }
                classes.Add(newClassVM);
            }

            if (JsonUtils.ExportJsonNet(classes, filePath))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                _window.Title = $"{_appName} - {fileName}";
                if (_filePath == null || !_filePath.Equals(filePath))
                {
                    MessageBox.Show("Save Successfull !");
                }

                _filePath = filePath;
            }
            else MessageBox.Show("A error occurred when save file !");
        }

        /// <summary>
        /// Open and deserialze json data
        /// </summary>
        /// <param name="obj"></param>
        private void OpenInvoke(object obj)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "txt files (*.json)|*.json|All files (*.*)|*.*";
            openDialog.Title = "Open data";
            if (openDialog.ShowDialog() == true)
            {
                IdGenerator.Reset();
                string jsonString = File.ReadAllText(openDialog.FileName);

                var data = JsonUtils.ImportJsonNet<ObservableCollection<ClassModel>>(jsonString);
                ObservableCollection<ClassViewModel> newClassViewModels = new();

                if (data != null)
                {
                    foreach (ClassModel classModel in data)
                    {
                        ClassViewModel classVM = new ClassViewModel(classModel);
                        newClassViewModels.Add(classVM);
                    }
                    ClassViewModels.Clear();
                    ClassViewModels = newClassViewModels;
                    this._mainModel.ClassModels.Clear();
                    this._mainModel.ClassModels = data;
                    _filePath = openDialog.FileName;
                    string fileName = Path.GetFileNameWithoutExtension(_filePath);
                    _window.Title = $"{_appName} - {fileName}";
                }
                else
                {
                    MessageBox.Show("A error occurred when open file !");
                }
            }
        }

        /// <summary>
        /// Delete and create new all data
        /// </summary>
        /// <param name="obj"></param>
        private void NewInvoke(object obj)
        {
            _window.Title = _appName;
            _filePath = null;
            ClassViewModels.Clear();
            IdGenerator.Reset();
        }

        /// <summary>
        /// Close main window
        /// </summary>
        /// <param name="obj"></param>
        private void CloseWndInvoke(object obj)
        {
            if ((_filePath == null && ClassViewModels.Count > 0)
                || (_filePath != null && !File.ReadAllText(_filePath).Equals(JsonUtils.SerializeJsonNet(ClassViewModels))))
            {
                MessageBoxResult result = MessageBox.Show($"Do you want to save change !", "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    SaveInvoke(obj);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            Application.Current.Shutdown();
        }

        /// <summary>
        /// Add a class to collection of class
        /// </summary>
        /// <param name="obj"></param>
        private void AddClassInvoke(object obj)
        {
            if (obj is DataGrid dgv && dgv.DataContext is MainViewModel mainVM)
            {
                var classsample = CreateClassSample();
                if (dgv.SelectedItem is ClassViewModel cls)
                {
                    int index = mainVM.ClassViewModels.IndexOf(cls);

                    mainVM.ClassViewModels.Insert(index + 1, classsample);
                    mainVM._mainModel.ClassModels.Insert(index + 1, classsample.ClassModel);
                }
                else
                {
                    mainVM.ClassViewModels.Add(classsample);
                    mainVM._mainModel.ClassModels.Add(classsample.ClassModel);
                }
            }
        }

        /// <summary>
        /// Detele selected classes
        /// </summary>
        /// <param name="obj"></param>
        private void DeleteClassInvoke(object obj)
        {
            if (obj is DataGrid dgv && dgv.DataContext is MainViewModel mainVM && dgv.SelectedItems.Count > 0)
            {
                var selected = dgv.SelectedItems;
                if (selected == null || selected.Count == 0)
                {
                    return;
                }
                MessageBoxResult result = MessageBox.Show($"Are you sure delete {selected.Count} student{(selected.Count == 1 ? "" : "s")}", "Warning", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                    for (int i = selected.Count - 1; i >= 0; i--)
                    {
                        if (selected[i] is ClassViewModel classVM)
                        {
                            mainVM.ClassViewModels.Remove(classVM);
                            mainVM._mainModel.ClassModels.Remove(classVM.ClassModel);
                        }
                    }
            }
        }

        /// <summary>
        /// Create a sample Class
        /// </summary>
        /// <returns></returns>
        private ClassViewModel CreateClassSample()
        {
            ClassViewModel classVM = new ClassViewModel(ClassModel.CreateDefault());
            classVM.AllowEditable = true;
            return classVM;
        }

        #endregion Method
    }
}