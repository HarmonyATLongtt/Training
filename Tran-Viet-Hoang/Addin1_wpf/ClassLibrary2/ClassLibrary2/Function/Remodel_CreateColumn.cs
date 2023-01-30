using Autodesk.Revit.DB;
using ClassLibrary2.Data;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Function
{
    public class Remodel_CreateColumn
    {
        public void CreateCols(Document doc, List<ConcreteColumnData> ColDatas, List<LevelData> LLevels)
        {
            var collevels = new FilteredElementCollector(doc)
                       .WhereElementIsNotElementType()
                       .OfCategory(BuiltInCategory.OST_Levels)
                       .Cast<Level>()
                       .ToList();

            var colTypes = new FilteredElementCollector(doc)
                       .WhereElementIsElementType()
                       .OfCategory(BuiltInCategory.OST_StructuralColumns)
                       .Cast<FamilySymbol>()
                       .ToList();

            if (collevels.Count > 0 && colTypes.Count > 0)
            {
                using (Transaction trans = new Transaction(doc, "create col"))
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
            var coltype = colTypes.FirstOrDefault(x => x.Name.Equals(colData.SectionName));
            var coltoplevel = levels.FirstOrDefault(x => x.Name.Equals(colData.Level));

            if (coltype != null && coltoplevel != null)
            {
                if (!coltype.IsActive)
                {
                    coltype.Activate();
                }
                //doc.Create.NewFamilyInstance(colData.Point_I, coltype, collevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                var col = doc.Create.NewFamilyInstance(colData.Point_I, coltype, coltoplevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                Parameter coltoppara = col.LookupParameter("Top Level");
                Parameter coltopoffsetpara = col.LookupParameter("Top Offset");

                Parameter colbasepara = col.LookupParameter("Base Level");
                Parameter colbaseoffsetpara = col.LookupParameter("Base Offset");

                new Remodel_MarkEtabsElement().SetComment(col, colData.Name);

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
    }
}
