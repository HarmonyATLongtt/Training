using Autodesk.Revit.DB;
using ClassLibrary2.Data;
using ClassLibrary2.Data.FrameData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ClassLibrary2.Factory.EtabDataExtractor
{
    public class EtabExtractor
    {
        private List<DataTable> _tables;

        public EtabExtractor(List<DataTable> tables)
        {
            _tables = tables;
        }

        #region Beam ReadData

        public List<ConcreteBeamData> ExtractBeam()
        {
            List<ConcreteBeamData> beamall = new List<ConcreteBeamData>();
            var tablebeamobject = _tables.FirstOrDefault(x => x.TableName.Equals("Beam Object Connectivity"));

            foreach (DataRow row in tablebeamobject.Rows)
            {
                ConcreteBeamData beam = new ConcreteBeamData();
                //nhập vô beam name, level, start, end
                ReadBeamObject(ref beam, row);
                // nhập vô As
                ReadBeamFlexureDesign(ref beam);
                // nhập vô section name
                ReadBeamSectionName(ref beam);
                // nhập vô tọa độ start, end
                ReadBeamPointLocation(ref beam);
                //nhập vào kích thước tiết diện
                ReadBeamDimenson(ref beam);
                //nhập vào lớp bê tông bảo vệ
                ReadBeamCover(ref beam);

                beamall.Add(beam);
            }
            return beamall;
        }

        private void ReadBeamPointLocation(ref ConcreteBeamData beam)
        {
            var rowI = FindRow("Point Object Connectivity", "UniqueName", beam.StartPoint.Id);
            var rowJ = FindRow("Point Object Connectivity", "UniqueName", beam.EndPoint.Id);

            if (rowI != null && rowJ != null)
            {
                beam.StartPoint.Point = ConvertPoint(rowI);
                beam.EndPoint.Point = ConvertPoint(rowJ);
            }
        }

        private void ReadBeamSectionName(ref ConcreteBeamData beam) //đọc section name mục đích ban đầu là set family instance cho cấu kiện vừa được vẽ, nhưng thời gian không nhiều nên tạo sẵn family trong project và section name hiện tại chỉ dùng để tham chiếu kích thước tiết diện
        {
            var row = FindRow("Frame Assignments - Section Properties", "UniqueName", beam.Name);

            if (row != null)
            {
                beam.Dimensions.SectionName = row["Section Property"].ToString();
            }
        }

        private void ReadBeamFlexureDesign(ref ConcreteBeamData beam)
        {
            var tablesection = _tables.FirstOrDefault(x => x.TableName.Equals("Concrete Beam Flexure Envelope - TCVN 5574-2012"));

            List<double> allAstop = new List<double>();
            List<double> allAsbot = new List<double>();
            foreach (DataRow row in tablesection.Rows)
            {
                if (beam.Name == row["UniqueName"].ToString())
                {
                    allAstop.Add(Convert.ToDouble(row["As Top"]));
                    allAsbot.Add(Convert.ToDouble(row["As Bot"]));
                }
            }

            beam.Reinforcing.AsBot = allAsbot.Max();
            beam.Reinforcing.AsTop = allAstop.Max();
        }

        private void ReadBeamCover(ref ConcreteBeamData beam)
        {
            var row = FindRow("Frame Section Property Definitions - Concrete Beam Reinforcing", "Name", beam.Dimensions.SectionName);
            if (row != null)
            {
                beam.Covers.Top = UnitUtils.ConvertToInternalUnits(Convert.ToDouble(row["Top Cover"]), DisplayUnitType.DUT_MILLIMETERS);
                beam.Covers.Bottom = UnitUtils.ConvertToInternalUnits(Convert.ToDouble(row["Bottom Cover"]), DisplayUnitType.DUT_MILLIMETERS);
            }
        }

        private void ReadBeamDimenson(ref ConcreteBeamData beam)
        {
            var row = FindRow("Frame Section Property Definitions - Concrete Rectangular", "Name", beam.Dimensions.SectionName);

            if (row != null)
            {
                beam.Dimensions.h = UnitUtils.ConvertToInternalUnits(Convert.ToDouble(row["Depth"]), DisplayUnitType.DUT_MILLIMETERS);
                beam.Dimensions.b = UnitUtils.ConvertToInternalUnits(Convert.ToDouble(row["Width"]), DisplayUnitType.DUT_MILLIMETERS);
            }
        }

        private void ReadBeamObject(ref ConcreteBeamData beam, DataRow row)
        {
            beam.Name = row["Unique Name"].ToString();
            beam.Level = row["Story"].ToString();
            beam.StartPoint.Id = row["UniquePtI"].ToString();
            beam.EndPoint.Id = row["UniquePtJ"].ToString();
            beam.Length = Convert.ToDouble(row["Length"]) / 304.8;
        }

        #endregion Beam ReadData

        #region Column Data

        public List<ConcreteColumnData> ExtractCol()
        {
            List<ConcreteColumnData> colall = new List<ConcreteColumnData>();
            var tablebeamobject = _tables.FirstOrDefault(x => x.TableName.Equals("Column Object Connectivity"));

            foreach (DataRow row in tablebeamobject.Rows)
            {
                ConcreteColumnData col = new ConcreteColumnData();
                //nhập vô beam name, level, start, end
                ReadColumnObject(ref col, row);
                // nhập vô section name
                ReadColumnSectionName(ref col);
                // nhập vô tọa độ start, end
                ReadColumnPointLocation(ref col);
                // nhập vô kích thước tiết diện
                ReadColumnDimenson(ref col);
                colall.Add(col);
            }
            return colall;
        }

        private void ReadColumnDimenson(ref ConcreteColumnData col)
        {
            if (col?.Dimensions != null)
            {
                var dataRow = FindRow("Frame Section Property Definitions - Concrete Rectangular", "Name", col.Dimensions.SectionName);
                if (dataRow != null)
                {   // Revit: Col bxh = Cx x Cy
                    // Etabs: SectionName: Cbxh = Depth x Width =  Cx x Cy
                    col.Dimensions.b = UnitUtils.ConvertToInternalUnits(Convert.ToDouble(dataRow["Depth"]), DisplayUnitType.DUT_MILLIMETERS);
                    col.Dimensions.h = UnitUtils.ConvertToInternalUnits(Convert.ToDouble(dataRow["Width"]), DisplayUnitType.DUT_MILLIMETERS);
                }
            }
        }

        private void ReadColumnObject(ref ConcreteColumnData col, DataRow row)
        {
            col.Name = row["Unique Name"].ToString();
            col.Level = row["Story"].ToString();
            col.EndPoint = null;
            col.StartPoint.Id = row["UniquePtI"].ToString();
            col.Length = Convert.ToDouble(row["Length"]) / 304.8;
        }

        private void ReadColumnSectionName(ref ConcreteColumnData col) //đọc section name mục đích ban đầu là set family instance cho cấu kiện vừa được vẽ, nhưng thời gian không nhiều nên tạo sẵn family trong project và section name hiện tại chỉ dùng để tham chiếu kích thước tiết diện
        {
            var dataRow = FindRow("Frame Assignments - Section Properties", "UniqueName", col.Name);
            if (dataRow != null)
            {
                col.Dimensions = new elemDimensionData();
                col.Dimensions.SectionName = dataRow["Section Property"].ToString();
            }
        }

        private void ReadColumnPointLocation(ref ConcreteColumnData col)
        {
            var dataRow = FindRow("Point Object Connectivity", "UniqueName", col.StartPoint.Id);
            if (dataRow != null)
                col.StartPoint.Point = ConvertPoint(dataRow);
        }

        #endregion Column Data

        #region Level ReadData

        public List<LevelData> LevelReadData()
        {
            var table = _tables.FirstOrDefault(x => x.TableName.Equals("Story Definitions"));

            double elev = 0;
            var levelclass = new List<LevelData>()
            {
                new LevelData("Base", elev),
            };

            var accendingRows = table.Rows
                                .Cast<DataRow>()
                                .OrderBy(r => r["Name"].ToString());
            foreach (DataRow row in accendingRows)
            {
                string height = row["Height"].ToString();
                //double height2 = Convert.ToDouble( row["Height"]);
                if (double.TryParse(height, out double val))
                {
                    elev += val;
                    var levelElev = UnitUtils.ConvertToInternalUnits(elev, DisplayUnitType.DUT_MILLIMETERS);
                    var name = row["Name"].ToString();
                    levelclass.Add(new LevelData(name,levelElev));
                };
            }
            return levelclass;
        }

        #endregion Level ReadData

        #region utils

        private DataRow FindRow(string tableName, string rowName, string queryName)
        {
            //lấy về bảng cần đọc
            var tablesection = _tables.FirstOrDefault(x => x.TableName.Equals(tableName));
            if (tablesection != null)
            {
                //trả về row có ô shell chứa giá trị đang xét
                return tablesection.Rows
                        .Cast<DataRow>()
                        .FirstOrDefault(row => row[rowName].ToString() == queryName);
            }
            return null;
        }

        public XYZ ConvertPoint(DataRow row)
        {
            // sử dụng hàm chuyển đổi đơn  vị version cũ nên hiện thông báo, nhưng vẫn chạy được!
            double x = UnitUtils.ConvertToInternalUnits(Convert.ToDouble(row["X"]), DisplayUnitType.DUT_MILLIMETERS);
            double y = UnitUtils.ConvertToInternalUnits(Convert.ToDouble(row["Y"]), DisplayUnitType.DUT_MILLIMETERS);
            double z = UnitUtils.ConvertToInternalUnits(Convert.ToDouble(row["Z"]), DisplayUnitType.DUT_MILLIMETERS);
            return new XYZ(x, y, z);
        }

        #endregion utils
    }
}