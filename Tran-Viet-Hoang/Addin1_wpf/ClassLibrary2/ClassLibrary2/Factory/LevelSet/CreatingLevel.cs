using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClassLibrary2.Data;
using ClassLibrary2.Utils;
using System.Collections.Generic;

namespace ClassLibrary2.Factory.LevelSet
{
    public class CreatingLevel
    {
        private UIApplication _uiapp;
        private UIDocument _uidoc;
        private Document _doc;

        //liệu có thể code thành kiểu add luôn vào list LevelData để sử dụng, từ đó không cần phải chạy vòng lặp ở ngoài class Cmd ?
        public void CreateLevel(ExternalCommandData commandData, List<LevelData> LevelModelData)
        {
            _uiapp = commandData.Application;
            _uidoc = _uiapp.ActiveUIDocument;
            _doc = _uidoc.Document;

            var elems = Common.GetListLevels(_doc);
            foreach (LevelData levelData in LevelModelData)
            {
                string Levelexisted = "false";

                using (var transaction = new Transaction(_doc, "Set Elevation"))
                {
                    foreach (var singlelevel in elems)
                    {
                        Parameter para = singlelevel.LookupParameter("Name");
                        Parameter cote = singlelevel.LookupParameter("Elevation");
                        if (double.TryParse(cote.AsValueString(), out double cotelevel))
                        {
                            if (para.AsString() == levelData.Name)
                            {
                                transaction.Start();
                                Levelexisted = "true";
                                cote.Set(levelData.Elevation);
                                transaction.Commit();
                            }
                        }
                    }

                    if (Levelexisted == "false")
                    {
                        transaction.Start();
                        Level newlevel = Level.Create(_doc, levelData.Elevation);
                        Parameter paranew = newlevel.LookupParameter("Name");
                        paranew.Set(levelData.Name);
                        transaction.Commit();
                    }
                }
            }
        }
    }
}