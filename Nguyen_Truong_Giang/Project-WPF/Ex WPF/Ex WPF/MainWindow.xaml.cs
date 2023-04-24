using ExcelDataReader;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;

namespace Ex_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class Person
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public string Address { get; set; }
            public double TaxFactor { get; set; }
        }

        public class Student : Person
        {
            public Student()
            {

            }
        }

        public class Teacher : Person
        {
        }

        public class Employee : Person
        {
        }


        string filePath = @"D:\Training\Nguyen_Truong_Giang\Project-WPF\Ex WPF\Ex WPF\Excel/Data.xlsx";

        public MainWindow()
        {
            InitializeComponent();

            //Đọc data từ file excecl

            //using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            //{
            //    using (var reader = ExcelReaderFactory.CreateReader(stream))
            //    {
            //        var dataSet = reader.AsDataSet();
            //        var dataTable = dataSet.Tables["Student"];
            //        var bindingList = new BindingList<Student>();
                    
            //      for (int indexRow = 1; indexRow<dataTable.Rows.Count; indexRow++)
            //      {
            //            DataRow row = dataTable.Rows[indexRow];
            //            Student student = new Student();
            //            student.ID = row[0].ToString();
            //            student.Name = row[1].ToString();
            //            student.Age = int.Parse(row[2].ToString());
            //            student.Address = row[3].ToString();
            //            student.TaxFactor = double.Parse(row[4].ToString());

            //            bindingList.Add(student);
            //            studentsDataGrid.ItemsSource = bindingList;
            //        }
        
            //    }
            //}

            // Set data context cho data grid
            DataContext = new ViewModel(new Model());
        }
    }
}
