using System.Collections.Generic;
using System.Data;

namespace WpfEx.Model
{
    public class SheetModel
    {
        public string Name { get; set; }

        public DataTable Data;

        public SheetModel(DataTable table)
        {
            if (table != null)
            {
                Name = table.TableName;
                Data = table;
            }
            else
            {
                Name = string.Empty;
                Data = new DataTable();
            }
        }

        public void AddRecord(List<string> cellDatas)
        {
            var row = Data.NewRow();
            for (int index = 0; index < cellDatas.Count; index++)
            {
                string val = cellDatas[index];
                row[index] = val;
            }
        }

        public void AddIteam(DataRowCollection rowDatas)
        {
            if (rowDatas?.Count > 0)
            {
                foreach (DataRow row in rowDatas)
                    Data.Rows.Add(row.ItemArray);
            }
        }
    }
}