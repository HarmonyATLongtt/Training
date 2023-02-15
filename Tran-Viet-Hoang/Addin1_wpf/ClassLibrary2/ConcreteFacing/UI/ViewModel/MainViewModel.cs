using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows.Input;

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

        public MainViewModel()
        {
            CreateCommand = new RelayCommand<object>(CreateCommandInvoke);
            CloseCommand = new RelayCommand<object>(CancelCommandInvoke);
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

        public void RaisePropertyChange(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}