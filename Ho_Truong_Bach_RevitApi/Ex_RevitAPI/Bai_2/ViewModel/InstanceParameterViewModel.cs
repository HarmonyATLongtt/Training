using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bai_2.Model;

namespace Bai_2.ViewModel
{
    public class InstanceParameterViewModel : BaseViewModel
    {
        public InstanceModel Model { get; set; }
        public string ParameterName 
        {
            get
            {
                return Model.ParameterName;
            }
            set
            {
                Model.ParameterName = value;
                OnPropertyChanged(nameof(ParameterName));
            }
        }
        public string DataType
        {
            get
            {
                return Model.DataType;
            }
            set
            {
                Model.DataType = value;
                OnPropertyChanged(nameof(DataType));
            }
        }
        public string Value
        {
            get
            {
                return Model.Value;
            }
            set
            {
                Model.Value = value;
                OnPropertyChanged(nameof(Value));
            }
        }
        public InstanceParameterViewModel(InstanceModel model)
        {
            Model = model;
        }
    }
}
