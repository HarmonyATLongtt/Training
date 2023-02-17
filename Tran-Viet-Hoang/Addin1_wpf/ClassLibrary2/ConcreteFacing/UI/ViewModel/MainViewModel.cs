using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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

        public MainViewModel()
        {
            CreateCommand = new RelayCommand<object>(CreateCommandInvoke);
            CloseCommand = new RelayCommand<object>(CancelCommandInvoke);
            ViewBeamUICommand = new RelayCommand(ViewBeamUICommandInvoke);
            ViewColumnUICommand = new RelayCommand(ViewColumnUICommandInvoke);
        }

        public class MenuItem
        {
            public MenuItem()
            {
                this.Items = new ObservableCollection<MenuItem>();
            }

            public string Category { get; set; }

            public ObservableCollection<MenuItem> Items { get; set; }
        }

        private void CreateCommandInvoke(object parameter)
        {
            if (parameter is System.Windows.Window window)
            {
                DataTable Beam = new DataTable();
                DataTable Column = new DataTable();

                Beam.TableName = "Dầm";
                Column.TableName = "Cột";

                List<DataTable> list = new List<DataTable>() { Beam, Column };

                Tables = new ObservableCollection<DataTable>(list);

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
                List<TemplateData> elems = new List<TemplateData>();
                List<string> strings = new List<string>() { "Top", "Left", "Front", "Bottom", "Right", "Back" };
                List<int> xindex = new List<int>() { 0, 0, 0, 1, 1, 1 };
                List<int> yindex = new List<int>() { 0, 1, 2, 0, 1, 2 };
                string cate = "Beam";
                string executablePath = System.IO.Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);

                string path = executablePath + "../../Facelayout/Colback.png";
                for (int i = 0; i < strings.Count; i++)
                {
                    elems.Add(new TemplateData()
                    {
                        x = xindex[i],
                        y = yindex[i],
                        txtName = "txt" + cate + strings[i],
                        cbxName = "cbx" + cate + strings[i],
                        cbxContent = cate + strings[i],
                        ImageFace = new BitmapImage(new Uri(path, UriKind.Relative)),
                        txtText = executablePath
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

                TableSelected = new List<TemplateData>(elems);
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
                List<TemplateData> elems = new List<TemplateData>();
                List<string> strings = new List<string>() { "Top", "Left", "Front", "Bottom", "Right", "Back" };
                List<int> xindex = new List<int>() { 0, 0, 0, 1, 1, 1 };
                List<int> yindex = new List<int>() { 0, 1, 2, 0, 1, 2 };
                string cate = "Column";
                for (int i = 0; i < strings.Count; i++)
                {
                    elems.Add(new TemplateData()
                    {
                        x = xindex[i],
                        y = yindex[i],
                        txtName = "txt" + cate + strings[i],
                        cbxName = "cbx" + cate + strings[i],
                        cbxContent = cate + strings[i]
                    });
                }

                TableSelected = new List<TemplateData>(elems);
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

        private List<TemplateData> _tablesselected;

        public List<TemplateData> TableSelected
        {
            get => _tablesselected;
            set
            {
                _tablesselected = value;
                RaisePropertyChange(nameof(TableSelected));
            }
        }

        // data for binding in the ListView- data template
        public class TemplateData
        {
            public int x { get; set; }
            public int y { get; set; }
            public string face { get; set; }
            public string cate { get; set; }
            public string txtName { get; set; }
            public string txtText { get; set; }
            public string cbxName { get; set; }
            public string cbxContent { get; set; }
            public BitmapImage ImageFace { get; set; }

            public TemplateData()
            {
                txtName = "txt" + cate + face;
                cbxName = "cbx" + cate + face;
                cbxContent = cate + face;
            }
        }
    }
}