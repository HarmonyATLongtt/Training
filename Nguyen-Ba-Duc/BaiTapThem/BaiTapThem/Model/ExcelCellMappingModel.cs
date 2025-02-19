using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiTapThem.Model
{
    public class ExcelCellMappingModel
    {
        public int SheetIndex { get; set; }
        public int ExcelColumn { get; set; }
        public int ExcelRow { get; set; }
        public int UiColumn { get; set; }
        public int UiRow { get; set; }
        public bool IsChanged { get; set; }
        public string? Value { get; set; }
    }
}