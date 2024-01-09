using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WPF.Model;

namespace WPF.View
{
    /// <summary>
    /// Interaction logic for ViewExcel.xaml
    /// </summary>
    public partial class ViewExcel : UserControl
    {
        private List<Student> students = new List<Student>();

        public ViewExcel()
        {
            InitializeComponent();
            LoadStudent();
        }

        private void LoadStudent()
        {
            students.Add(new Student() { Id = 1, Name = "ABC", Age = 20, Address = "HN" });
            students.Add(new Student() { Id = 2, Name = "ABC", Age = 20, Address = "HN" });
            students.Add(new Student() { Id = 3, Name = "ABC", Age = 20, Address = "HN" });
            students.Add(new Student() { Id = 4, Name = "ABC", Age = 20, Address = "HN" });
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            dgExcel.Columns.Clear();
            dgExcel.ItemsSource = students;
            //dgExcel.MaxColumnWidth = dgExcel.Width / dgExcel.Columns.Count;
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            string filePath = "";
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Excel | *.xlsx | Excel 2003 | *.xls";
            if(dialog.ShowDialog() == true)
            {
                filePath = dialog.FileName;
            }
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Invalid path");
                return;
            }
            try
            {
                using(ExcelPackage p = new ExcelPackage())
                { 
                    p.Workbook.Properties.Author = "VMD";
                    p.Workbook.Properties.Title = "FileExport";
                    p.Workbook.Worksheets.Add(Name);
                    ExcelWorksheet ws = p.Workbook.Worksheets[1];
                }
            }catch(Exception ew)
            {
                MessageBox.Show(ew.ToString());
            }
        }
    }
}