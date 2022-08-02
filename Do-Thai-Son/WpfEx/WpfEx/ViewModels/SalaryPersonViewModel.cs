using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfEx.Model;
using WpfEx.ViewModels.Base;

namespace WpfEx.ViewModels
{
    public class SalaryPersonViewModel : BindableBase
    {
        public SalaryPersonModel salaryModel;

        public string Name
        {
            get => salaryModel.Name;
            set
            {
                salaryModel.Name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public DataTable DataTable
        {
            get => salaryModel.Data;
            set => SetProperty(ref salaryModel.Data, value, nameof(DataTable));
        }

        public SalaryPersonViewModel(SalaryPersonModel Model)
        {
            this.salaryModel = Model;
            InitCommand();
        }

        #region Command and event

        public ICommand OKCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        #endregion Command and event

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