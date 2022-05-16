using System.Data;
using System.Windows;
using System.Windows.Input;
using WpfEx.Model;
using WpfEx.ViewModels.Base;

namespace WpfEx.ViewModels
{
    public class AddIteamDataGridViewModle : BindableBase
    {
        #region Property

        public AddIteamDataGridModle AddModel;

        public string Name
        {
            get => AddModel.Name;
            set
            {
                AddModel.Name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public DataTable DataTable
        {
            get => AddModel.Data;
            set => SetProperty(ref AddModel.Data, value, nameof(DataTable));
        }

        #endregion Property

        #region Command and event

        public ICommand OKCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        #endregion Command and event

        public AddIteamDataGridViewModle(AddIteamDataGridModle Model)
        {
            this.AddModel = Model;
            InitCommand();
        }

        private void InitCommand()
        {
            OKCommand = new RelayCommand<object>(OKCommandInvoke);
            CancelCommand = new RelayCommand<object>(CancelCommandInvoke);
        }

        #region command implementations and event handlers

        /// <summary>
        /// Accept all user add iteam from UI
        /// </summary>
        /// <param name="obj"></param>
        private void OKCommandInvoke(object obj)
        {
            if (obj is Window window)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        /// <summary>
        /// Cancel all user add iteam from UI
        /// </summary>
        /// <param name="obj"></param>
        private void CancelCommandInvoke(object obj)
        {
            if (obj is Window window)
            {
                window.DialogResult = false;
                window.Close();
            }
        }

        #endregion command implementations and event handlers
    }
}