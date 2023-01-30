using Autodesk.Revit.DB;
using ClassLibrary2.Data;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Function
{
    public class Remodel_CreateBeam
    {
        public void CreateBeams(Document doc, List<ConcreteBeamData> beamDatas)
        {
            var levels = new FilteredElementCollector(doc)
                       .WhereElementIsNotElementType()
                       .OfCategory(BuiltInCategory.OST_Levels)
                       .Cast<Level>()
                       .ToList();

            var beamTypes = new FilteredElementCollector(doc)
                       .WhereElementIsElementType()
                       .OfCategory(BuiltInCategory.OST_StructuralFraming)
                       .Cast<FamilySymbol>()
                       .ToList();

            if (levels.Count > 0 && beamTypes.Count > 0)
            {
                using (Transaction trans = new Transaction(doc, "create beams"))
                {
                    trans.Start();
                    foreach (ConcreteBeamData beamData in beamDatas)
                    {
                        CreateBeam(doc, beamData, levels, beamTypes);
                    }
                    trans.Commit();
                }
            }
        }

        public void CreateBeam(Document doc, ConcreteBeamData beamData, List<Level> levels, List<FamilySymbol> beamTypes)
        {
            var beamtype = beamTypes.FirstOrDefault(x => x.Name.Equals(beamData.SectionName));
            var beamlevel = levels.FirstOrDefault(x => x.Name.Equals(beamData.Level));

            if (beamtype != null && beamlevel != null)
            {
                if (!beamtype.IsActive)
                {
                    beamtype.Activate();
                }
                Curve beamLine = Line.CreateBound(beamData.Point_I, beamData.Point_J);
                FamilyInstance beamnew = doc.Create.NewFamilyInstance(beamLine, beamtype, beamlevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                //mỗi khi 1 beam mới đc tạo ra thì gán ngay rebar cover của etabs mình tạo cho beam đó
                new Remodel_SetRebarCover().SetRebarCover(doc,beamnew,beamData);
                new Remodel_MarkEtabsElement().SetComment(beamnew,beamData.Name);
                
            }
        }
    }
}
