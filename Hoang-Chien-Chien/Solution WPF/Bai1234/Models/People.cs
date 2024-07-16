using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bai1
{
    public class People:INotifyPropertyChanged
    {
        private Guid id;
        private string name;
        private int age;
        private string address;
        private double income;
        private double taxCoe;

        public People()
        {
            this.id = Guid.NewGuid();
        }
        public Guid Id { get => id; set { id = value; OnPropertyChanged(nameof(Id)); } }
        public string Name { get => name; set { name = value; OnPropertyChanged(nameof(Name)); } }
        public int Age { get => age; set { age = value; OnPropertyChanged(nameof(Age)); } }
        public string Address { get => address; set { address = value; OnPropertyChanged(nameof(Address)); } }
        public double Income { get => income; set { income = value; OnPropertyChanged(nameof(Income)); } }
        public double TaxCoe { get => taxCoe; set { taxCoe = value; OnPropertyChanged(nameof(TaxCoe)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
