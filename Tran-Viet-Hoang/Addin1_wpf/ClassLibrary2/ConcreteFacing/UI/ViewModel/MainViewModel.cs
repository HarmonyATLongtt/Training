using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ConcreteFacing.UI.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        //tao itemsource de bind vao listbox
        private ObservableCollection<DataTable> _tables;

        public ObservableCollection<DataTable> Tables
        {
            get => _tables;
            set
            {
                _tables = value;
                RaisePropertyChange(nameof(Tables));
            }
        }

        public ICommand CreateCommand { get; set; }
        public ICommand CloseCommand { get; set; }
        public ICommand ViewBeamUICommand { get; set; }
        public ICommand ViewColumnUICommand { get; set; }

        public MainViewModel(List<Element> elems)
        {
            ViewBeamUICommand = new RelayCommand(ViewBeamUICommandInvoke);
            ViewColumnUICommand = new RelayCommand(ViewColumnUICommandInvoke);

            ObservableCollection<TemplateCategoryViewModel> cates = new ObservableCollection<TemplateCategoryViewModel>();
            //List<string> contents = new List<string>() { "Structural Framing", "Structural Column" };
            List<ICommand> cmds = new List<ICommand>() { ViewBeamUICommand, ViewColumnUICommand };
            BindingCategoryList(cates, cmds, elems);

            CreateCommand = new RelayCommand<object>(CreateCommandInvoke);
            CloseCommand = new RelayCommand<object>(CancelCommandInvoke);
        }

        private void BindingCategoryList(ObservableCollection<TemplateCategoryViewModel> cates, List<ICommand> cmds,  List<Element> elems)
        {
            var catNames = elems.Select(x => x.Category.Name).Distinct().ToList();

            foreach(var catName in catNames)
            {
                if (catName == "Structural Framing")
                {
                    cates.Add(new TemplateCategoryViewModel()
                    {
                        btnContent = catName,
                        btnCmd = cmds[0],
                        elems = elems.Where(c => c.Category.Name == catName).ToList(),
                    });
                }
                if (catName == "Structural Columns")
                {
                    cates.Add(new TemplateCategoryViewModel()
                    {
                        btnContent = catName,
                        btnCmd = cmds[1],
                        elems = elems.Where(c => c.Category.Name == catName).ToList(),

                    });
                }
            }
            SourceCatelv = new ObservableCollection<TemplateCategoryViewModel>(cates);
        }

        private void CreateCommandInvoke(object parameter)
        {
            if (parameter is System.Windows.Window window)
            {
                var cates = SourceCatelv;
                string ms = "User Hii has check for: " + "\n" ;
                foreach(var cate in cates)
                {
                    if (cate.cbxIsCheck)
                    {
                        ms += cate.btnContent + "\n";
                    }
                }
                MessageBox.Show(ms);
                window.DialogResult = true;
                window.Close();
            }
        }

        private void CancelCommandInvoke(object parameter)
        {
            if (parameter is System.Windows.Window wnd)
            {
                wnd.DialogResult = false;

                wnd.Close();
            }
        }

        private void ViewBeamUICommandInvoke()
        {
            try
            {
                List<TemplateCoverFaceViewModel> elems = new List<TemplateCoverFaceViewModel>();
                List<string> strings = new List<string>() { "Top", "Left", "Front", "Bottom", "Right", "Back" };
                List<int> xindex = new List<int>() { 0, 0, 0, 1, 1, 1 };
                List<int> yindex = new List<int>() { 0, 1, 2, 0, 1, 2 };
                List<string> paths = new List<string>() {
                    "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/beamtop.png",
                    "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/beamleft.png",
                    "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/beamfront.png",
                    "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/beambottom.png",
                    "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/beamright.png",
                    "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/beamback.png",
                    };

                string cate = "Beam";

                for (int i = 0; i < strings.Count; i++)
                {
                    elems.Add(new TemplateCoverFaceViewModel()
                    {
                        x = xindex[i],
                        y = yindex[i],
                        txtName = "txt" + cate + strings[i],
                        cbxName = "cbx" + cate + strings[i],
                        cbxContent = cate + strings[i],
                        imgSource = new BitmapImage(new Uri(paths[i])),
                        txtText = 5.ToString(),
                        imgheight = 200,
                        imgwidth = 350
                    });
                }
                ////Top
                //elems.Add(new Elem() { x = 0, y = 0, txtName = "txtBeamTop", cbxName = "cbxBeamTop", cbxContent = "BeamTop" });
                ////Left
                //elems.Add(new Elem() { x = 0, y = 1, txtName = "txtBeamLeft", cbxName = "cbxBeamLeft", cbxContent = "BeamLeft" });
                ////Front
                //elems.Add(new Elem() { x = 0, y = 2, txtName = "txtBeamFront", cbxName = "cbxBeamFront", cbxContent = "BeamFront" });
                ////Bottom
                //elems.Add(new Elem() { x = 1, y = 0, txtName = "txtBeamTop", cbxName = "cbxBeamTop", cbxContent = "BeamTop" });
                ////Right
                //elems.Add(new Elem() { x = 1, y = 1, txtName = "txtBeamTop", cbxName = "cbxBeamTop", cbxContent = "BeamTop" });
                ////Back
                //elems.Add(new Elem() { x = 1, y = 2, txtName = "txtBeamTop", cbxName = "cbxBeamTop", cbxContent = "BeamTop" });

                SourceCoverFacelv = new List<TemplateCoverFaceViewModel>(elems);
                MessageBox.Show("Pick Beam Cover Face!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace.ToString());
            }
        }

        private void ViewColumnUICommandInvoke()
        {
            try
            {
                List<TemplateCoverFaceViewModel> elems = new List<TemplateCoverFaceViewModel>();
                List<string> strings = new List<string>() { "Top", "Left", "Front", "Bottom", "Right", "Back" };
                List<int> xindex = new List<int>() { 0, 0, 0, 1, 1, 1 };
                List<int> yindex = new List<int>() { 0, 1, 2, 0, 1, 2 };
                List<string> paths = new List<string>() {
                "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/Coltop.png",
                "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/Colleft.png",
                "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/Colfront.png",
                "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/Colbottom.png",
                "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/Colright.png",
                "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/Colback.png",};

                string cate = "Column";
                for (int i = 0; i < strings.Count; i++)
                {
                    elems.Add(new TemplateCoverFaceViewModel()
                    {
                        x = xindex[i],
                        y = yindex[i],
                        txtName = "txt" + cate + strings[i],
                        cbxName = "cbx" + cate + strings[i],
                        cbxContent = cate + strings[i],
                        imgSource = new BitmapImage(new Uri(paths[i])),
                        txtText = 5.ToString(),
                        imgheight = 350,
                        imgwidth = 300
                    });
                }

                SourceCoverFacelv = new List<TemplateCoverFaceViewModel>(elems);
                MessageBox.Show("Pick Column Cover Face!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace.ToString());
            }
        }

        public void RaisePropertyChange(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }

        public virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #region templateCoverSetting

        private List<TemplateCoverFaceViewModel> _sourceCoverFacelv;

        public List<TemplateCoverFaceViewModel> SourceCoverFacelv
        {
            get => _sourceCoverFacelv;
            set
            {
                _sourceCoverFacelv = value;
                RaisePropertyChange(nameof(SourceCoverFacelv));
            }
        }

        // data for binding in the ListView- data template
        public class TemplateCoverFaceViewModel
        {
            public int x { get; set; }
            public int y { get; set; }
            public string face { get; set; }
            public string cate { get; set; }
            public string txtName { get; set; }
            public string txtText { get; set; }
            public string cbxName { get; set; }
            public string cbxContent { get; set; }
            public bool cbxIsCheck { get; set; }
            public BitmapImage imgSource { get; set; }
            public double imgheight { get; set; }
            public double imgwidth { get; set; }

            public TemplateCoverFaceViewModel()
            {
               
            }
        }

        #endregion templateCoverSetting

        #region templateCate

        public ObservableCollection<TemplateCategoryViewModel> SourceCatelv { get; set; }

        public class TemplateCategoryViewModel
        {
            public string btnContent { get; set; }
            public ICommand btnCmd { get; set; }
            public bool cbxIsCheck { get; set; }
            public List<Element> elems { get; set; }

            public TemplateCategoryViewModel()
            {
            }
        }

        #endregion templateCate
    }
}