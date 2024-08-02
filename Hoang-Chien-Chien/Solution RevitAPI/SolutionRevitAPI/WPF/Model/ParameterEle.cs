using System;
using System.ComponentModel;

namespace SolutionRevitAPI.WPF.Model
{
    public class ParameterEle : INotifyPropertyChanged
    {
        private string name;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public double ValuePara
        {
            get => valuePare;
            set
            {
                valuePare = value;
                OnPropertyChanged(nameof(ValuePara));
            }
        }

        private Double valuePare;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}