using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClassLibrary2.Data;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Function
{
    public class Remodel_GetFrame
    {
        //Tạo list trả về dữ liệu kiểu FamilyInstance là các Cột vừa được vẽ
        public List<FamilyInstance> EtabsColumns(Document doc, List<ConcreteColumnData> cols)
        {
            List<FamilyInstance> colelemid = new List<FamilyInstance>();
            foreach (var colname in cols)
            {
                string cmt = "Etabs" + colname.Name;
                var colType = new FilteredElementCollector(doc)
                  .WhereElementIsNotElementType()
                  .OfCategory(BuiltInCategory.OST_StructuralColumns)
                  .Cast<FamilyInstance>()
                  .First(x => x.LookupParameter("Comments").AsString() == cmt);
                if (colType != null)
                {
                    colelemid.Add(colType);
                }
            }
            return colelemid;
        }

        //Tạo list trả về dữ liệu kiểu FamilyInstance là các dầm vừa được vẽ
        public List<FamilyInstance> EtabsBeams(Document doc, List<ConcreteBeamData> beams)
        {
            List<FamilyInstance> beamelemids = new List<FamilyInstance>();
            foreach (var beamname in beams)
            {
                string cmt = "Etabs" + beamname.Name;
                var beamType = new FilteredElementCollector(doc)
                  .WhereElementIsNotElementType()
                  .OfCategory(BuiltInCategory.OST_StructuralFraming)
                  .Cast<FamilyInstance>()
                  .First(x => x.LookupParameter("Comments").AsString() == cmt);
                if (beamType != null)
                {
                    beamelemids.Add(beamType);
                }
            }
            return beamelemids;
        }
    }
}
