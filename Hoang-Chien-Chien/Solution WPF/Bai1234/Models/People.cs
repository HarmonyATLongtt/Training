using System;
using System.ComponentModel;

namespace Bai1
{
    public class People : INotifyPropertyChanged
    {
        private Guid id;
        public Guid Id
        { get => id; set { id = value; OnPropertyChanged(nameof(Id)); } }
        private string name;
        public string Name
        { get => name; set { name = value; OnPropertyChanged(nameof(Name)); } }
        private int age;
        public int Age
        { get => age; set { age = value; OnPropertyChanged(nameof(Age)); } }
        private string address;
        public string Address
        { get => address; set { address = value; OnPropertyChanged(nameof(Address)); } }
        private double income;
        public double Income
        { get => income; set { income = value; OnPropertyChanged(nameof(Income)); } }
        private double taxCoe;
        public double TaxCoe
        { get => taxCoe; set { taxCoe = value; OnPropertyChanged(nameof(TaxCoe)); } }

        public People()
        {
            this.id = Guid.NewGuid();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}