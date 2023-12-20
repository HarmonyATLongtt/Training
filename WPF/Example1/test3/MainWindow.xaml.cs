using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace test3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataTable dataTable;
        string filename = "";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Excel| *.xlsx";
            if (dialog.ShowDialog() == true)
            {
                filename = dialog.FileName;
            }

            try
            {
                var workbook = new XLWorkbook(filename);
                var worksheet = workbook.Worksheets.First();
                var columns = worksheet.LastColumnUsed().ColumnNumber();

                dataTable = new DataTable();

                for (int i = 1; i <= columns; i++)
                {
                    dataTable.Columns.Add(GetExcelColumnName(i));
                }

                for (int i = 1; i <= worksheet.LastRowUsed().RowNumber(); i++)
                {
                    var row = worksheet.Row(i);
                    var dataRow = dataTable.NewRow();
                    for (int j = 0; j < columns; j++)
                    {
                        dataRow[j] = row.Cell(j + 1).Value.ToString();
                    }

                    dataTable.Rows.Add(dataRow);
                }

                //foreach(var row in worksheet.RowsUsed())
                //{
                //    var dataRow = dataTable.NewRow();
                //    for (int i = 0; i < columns; i++)
                //    {
                //        dataRow[i] =  row.Cell(i + 1).Value.ToString();
                //    }

                //    dataTable.Rows.Add(dataRow);
                //}
                gridView.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataTable != null)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Sheet1");

                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            for (int j = 0; j < dataTable.Columns.Count; j++)
                            {
                                worksheet.Cell(i + 1, j + 1).Value = dataTable.Rows[i][j].ToString();
                            }
                        }

                        workbook.SaveAs(filename);
                        MessageBox.Show("Data saved successfully.");
                    }
                }
                else
                {
                    MessageBox.Show("No data to save.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private string GetExcelColumnName(int columnIndex)
        {
            int dividend = columnIndex;
            string columnName = String.Empty;

            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            dataTable = null;
            gridView.ItemsSource = null;
        }
    }
}