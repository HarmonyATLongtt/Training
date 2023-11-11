using Autodesk.Revit.DB;
using ConcreteFacing.DATA;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static ConcreteFacing.DATA.OptionViewModel;

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

        public MainViewModel(List<Element> elems)
        {
            ObservableCollection<CategoryViewModel> cates = new ObservableCollection<CategoryViewModel>();
            BindingCategoryList(cates, elems);

            CreateCommand = new RelayCommand<object>(CreateCommandInvoke);
            CloseCommand = new RelayCommand<object>(CancelCommandInvoke);
        }

        private void BindingCategoryList(ObservableCollection<CategoryViewModel> cates, List<Element> elems)
        {
            var catNames = elems.Select(x => x.Category.Name).Distinct().ToList();

            foreach (var catName in catNames)
            {
                ObservableCollection<OptionViewModel> elemfaces = new ObservableCollection<OptionViewModel>();
                OptionsSetData optiondata = new OptionsSetData();

                if (catName == "Structural Framing")
                {
                    optiondata = new OptionsSetBeamData();
                    List<string> paths = optiondata.imgpaths;

                    for (int i = 0; i < paths.Count; i++)
                    {
                        elemfaces.Add(new OptionViewModel()
                        {
                            CoverFaceContent = Enum.ToObject(typeof(BeamFaces), i).ToString(),
                            CoverFaceImgSource = new BitmapImage(new Uri(paths[i])),
                            imgheight = optiondata.imgsize[0],
                            imgwidth = optiondata.imgsize[1]
                        });
                    }
                }
                else if (catName == "Structural Columns")
                {
                    optiondata = new OptionsSetColumnData();
                    List<string> paths = optiondata.imgpaths;

                    for (int i = 0; i < paths.Count; i++)
                    {
                        elemfaces.Add(new OptionViewModel()
                        {
                            CoverFaceContent = Enum.ToObject(typeof(ColumnFaces), i).ToString(),
                            CoverFaceImgSource = new BitmapImage(new Uri(paths[i])),
                            imgheight = optiondata.imgsize[0],
                            imgwidth = optiondata.imgsize[1]
                        });
                    }
                }

                cates.Add(new CategoryViewModel()
                {
                    CateElems = elems.Where(c => c.Category.Name == catName).ToList(),
                    CateName = catName,
                    TemplateCoverFaceViewModels = elemfaces,
                });
            }
            SourceCatelv = new ObservableCollection<CategoryViewModel>(cates);
            
        }

        private void CreateCommandInvoke(object parameter)
        {
            if (parameter is System.Windows.Window window)
            {
                var cates = SourceCatelv;
                string ms = "User Hii has check for: " + "\n";
                //string ms = cates.cbxContent;
                foreach (var cate in cates)
                {
                    //cate.Thickness = Convert.ToDouble(CoverThickness);
                    ms += cate.CateName + " được độ dày lớp cover là: " + cate.Thickness + "\n";
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

        private List<OptionViewModel> _lstCateSelect;

        public List<OptionViewModel> LstCateSelect
        {
            get => _lstCateSelect;
            set
            {
                _lstCateSelect = value;
                RaisePropertyChange(nameof(LstCateSelect));
            }
        }

        #endregion templateCoverSetting

        #region templateCate

        private ObservableCollection<CategoryViewModel> _sourceCatelv;

        public ObservableCollection<CategoryViewModel> SourceCatelv
        {
            get => _sourceCatelv;
            set
            {
                if (_sourceCatelv != value)
                {
                    _sourceCatelv = value;
                    OnPropertyChanged(nameof(SourceCatelv));
                }
            }
        }

        private CategoryViewModel _selectedCate;

        public CategoryViewModel SelectedCate
        {
            get => _selectedCate;
            set
            {
                if (_selectedCate != value)
                {
                    _selectedCate = value;
                    OnPropertyChanged(nameof(SelectedCate));
                }
            }
        }

        #endregion templateCate
    }
}