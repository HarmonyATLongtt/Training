using System;
using System.Collections.Generic;

namespace BaiTapThem.Model
{
    public class NhanVien
    {
        public string Ten { get; set; }
        public DateTime NgaySinh { get; set; }
        public string SoCanCuoc { get; set; }
        public DateTime NgayKyHopDong { get; set; }
        public double ThamNien { get; set; }
        public string ChucVu { get; set; }
        public double Luong { get; set; }
        public double DiemNhanVien { get; set; }
    }

    //public class DataCell
    //{
    //    public Object CellValue { get; set; }

    //    public bool CellStatus = false;
    //    public Type CellType { get; set; }

    //}

    //public class ExcelTable
    //{
    //    public string Name { get; set; }
    //    public List<Title> ListTitles { get; set; }

    //    public List<DataCell> ListDatas { get; set; }

    //}

    //public class Title
    //{
    //    public string Name { get; set; }
    //    public Type TitleType { get; set; }
    //}

    // tree view with checkbox
    // draw custom control gõ hình tròn thì vẽ ra button hình tròn, vuông ra vuông,
    // ảnh trên button
    // datagrid _datatable
    //          _view

    //trigger _property change, lost focus ..
}