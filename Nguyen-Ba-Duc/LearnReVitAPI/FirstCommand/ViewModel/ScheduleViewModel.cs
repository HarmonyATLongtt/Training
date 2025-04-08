using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using FirstCommand.Model;

namespace FirstCommand.ViewModel
{
    public class ScheduleViewModel : BaseViewModel
    {
        private Document _doc;
        public ViewSchedule _schedule;
        public List<int> RowIndexes { get; set; }
        public List<string> DoubleColumns { get; } = new List<string> { "W", "L", "H", "Unconnected Height" };
        private List<string> _listFieldCombinedOrCaculated;

        public List<string> ListFieldCombinedOrCaculated
        {
            get { return _listFieldCombinedOrCaculated; }
            set
            {
                _listFieldCombinedOrCaculated = value;
                OnPropertyChanged();
            }
        }

        private List<CellInfo> _cellInfos;

        public List<CellInfo> CellInfos
        {
            get { return _cellInfos; }
            set
            {
                _cellInfos = value;
                OnPropertyChanged();
            }
        }

        public DataTable dataTable { get; set; }

        //public ObservableCollection<ScheduleRowModel> ScheduleRows { get; set; }
        //public RelayCommand UpdateCommand { get; }

        public ScheduleViewModel(Document doc, ViewSchedule schedule)
        {
            _doc = doc;
            _schedule = schedule;
            dataTable = new DataTable();
            RowIndexes = new List<int>();
            ListFieldCombinedOrCaculated = new List<string>();
            //ListListCellInfos = new List<List<CellInfo>>();
            //UpdateCommand = new RelayCommand(Update);
            CellInfos = GetDataFromSchedule(schedule, dataTable);
        }

        private List<CellInfo> GetDataFromSchedule(ViewSchedule schedule, DataTable dataTable)
        {
            TableData tableData = schedule.GetTableData();
            TableSectionData sectionData = tableData.GetSectionData(SectionType.Body);

            int rowCount = sectionData.NumberOfRows;
            int colCount = sectionData.NumberOfColumns;

            for (int a = 0; a < colCount; a++)
            {
                string cellValue = schedule.GetCellText(SectionType.Body, 0, a);
                dataTable.Columns.Add(cellValue);
            }

            List<CellInfo> cellInfos = new List<CellInfo>();

            List<RowInfo> rowInfosList = new List<RowInfo>();
            for (int row = 1; row < rowCount; row++)
            {
                RowInfo rowInfo = new RowInfo();
                rowInfo.Row = row;
                for (int col = 0; col < colCount; col++)
                {
                    string cellValue = schedule.GetCellText(SectionType.Body, row, col);
                    rowInfo.columnValues.Add(col, cellValue);
                }
                if (IsRowHasValue(rowInfo.columnValues))
                {
                    rowInfosList.Add(rowInfo);
                }
            }
            foreach (RowInfo info in rowInfosList)
            {
                var newRow = dataTable.NewRow();
                foreach (var keyPairValue in info.columnValues)
                {
                    ScheduleField field = schedule.Definition.GetField(keyPairValue.Key);
                    if (field.IsCombinedParameterField == true || field.IsCalculatedField == true)
                    {
                        ListFieldCombinedOrCaculated.Add(field.GetName());
                    }

                    CellInfo cellInfo = new CellInfo();
                    cellInfo.rowCellSchedule = info.Row;
                    cellInfo.colCellSchedule = keyPairValue.Key;
                    cellInfo.colCellTable = keyPairValue.Key;
                    cellInfo.rowCellTable = dataTable.Rows.Count + 1;
                    cellInfo.paramId = field.ParameterId;
                    cellInfos.Add(cellInfo);
                    newRow[keyPairValue.Key] = keyPairValue.Value;
                }
                dataTable.Rows.Add(newRow);
            }
            RowIndexes = GetRowIndexs(dataTable, colCount);
            return cellInfos;
        }

        private List<int> GetRowIndexs(DataTable dataTable, int colCount)
        {
            List<int> listRowIndexs = new List<int>();
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                int num = 0;
                DataRow row = dataTable.Rows[i];
                for (int j = 0; j < colCount; j++)
                {
                    if (row[j].ToString() != "")
                    {
                        num++;
                    }
                }
                if (num == 1)
                {
                    listRowIndexs.Add(i);
                }
            }
            return listRowIndexs;
        }

        private bool IsRowHasValue(Dictionary<int, string> colValues)
        {
            bool hasValue = false;
            foreach (string value in colValues.Values)
            {
                if (value != "")
                {
                    hasValue = true;
                    break;
                }
            }
            return hasValue;

            //int num = 0;
            //foreach (string value in colValues.Values)
            //{
            //    if (value != "")
            //    {
            //        num++;
            //    }
            //}
            //if (num > 1)
            //{
            //    return true;
            //}
            //return false;
        }
    }

    public class CellInfo
    {
        public int rowCellSchedule { get; set; }
        public int colCellSchedule { get; set; }
        public int rowCellTable { get; set; }
        public int colCellTable { get; set; }
        public string value { get; set; }

        public bool isChanged = false;
        public ElementId paramId { get; set; }
    }
}