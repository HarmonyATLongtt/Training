using Autodesk.Revit.DB;
using ClassLibrary2.Data;
using ClassLibrary2.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Factory.ReinforcingColumnSet
{
    public class CreatingConcreteColumnHost
    {
        public void CreateCols(Document doc, List<ConcreteColumnData> ColDatas, List<LevelData> LLevels)
        {
            var cols = new List<BuiltInCategory>() { BuiltInCategory.OST_StructuralColumns };

            var collevels =  Common.GetListLevels(doc);
            var colTypes =  Common.GetListFamilySymbols(doc, cols);

            if (collevels.Count > 0 && colTypes.Count > 0)
            {
                using (Transaction trans = new Transaction(doc, "create cols"))
                {
                    trans.Start();
                    foreach (ConcreteColumnData colData in ColDatas)
                    {
                        CreateCol(doc, colData, LLevels, collevels, colTypes);
                    }
                    trans.Commit();
                }
            }
        }

        public void CreateCol(Document doc, ConcreteColumnData colData, List<LevelData> LLevels, List<Level> levels, List<FamilySymbol> colTypes)
        {
            var coltype = colTypes.FirstOrDefault(x => x.Name.Equals(colData.Dimensions.SectionName));
            var coltoplevel = levels.FirstOrDefault(x => x.Name.Equals(colData.Level));

            if (coltype != null && coltoplevel != null)
            {
                if (!coltype.IsActive)
                {
                    coltype.Activate();
                }
                //doc.Create.NewFamilyInstance(colData.Point_I, coltype, collevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                var col = doc.Create.NewFamilyInstance(colData.StartPoint.Point, coltype, coltoplevel, Autodesk.Revit.DB.Structure.StructuralType.Column);

                Parameter elemlength = col.LookupParameter("Length");

                colData.Dimensions.b = col.Symbol.LookupParameter("b").AsDouble();
                colData.Dimensions.h = col.Symbol.LookupParameter("h").AsDouble();
                colData.Length = elemlength.AsDouble();

                colData.Host = col;

                Parameter coltoppara = col.LookupParameter("Top Level");
                Parameter coltopoffsetpara = col.LookupParameter("Top Offset");

                Parameter colbasepara = col.LookupParameter("Base Level");
                Parameter colbaseoffsetpara = col.LookupParameter("Base Offset");

                SetComment(col, colData.Name);

                string storybase = null;
                string ok = "No";
                foreach (var story in LLevels) // vòng lặp để lấy được giá trị base level của cột, do trong bảng dữ liệu Etabs, cột Story trong bảng Cột chỉ thể hiện top level của cột đó
                {
                    if (story.Name == coltoplevel.Name)
                    {
                        ok = "Yes";
                        break;
                    }
                    if (ok == "No")
                        //Mỗi khi trạng thái là "No" thì storybase thay đổi giá trị, thay đổi cho đến khi nhận thấy level xét đến tiếp theo là top level, do đó storybase chỉ giữ giá trị cuối cùng là level trước đó của toplevel - đó chính là baselevel
                        storybase = story.Name;
                }
                var colbaselevel = levels.FirstOrDefault(x => x.Name.Equals(storybase));
                colbasepara.Set(colbaselevel.Id);
                colbaseoffsetpara.Set(0); // set bằng 0 vì đôi khi vẽ cột ra thì 1 giá trị top hay base offset của cột tự động gán 1 giá trị không mong muốn
                coltoppara.Set(coltoplevel.Id);
                coltopoffsetpara.Set(0);
            }
        }

        public void SetComment(FamilyInstance ins, string name)
        {
            Parameter cmt = ins.LookupParameter("Comments");
            cmt.Set("Etabs" + name);
        }

        

        
    }
}