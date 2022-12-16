using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using ClassLibrary2.Data;
using ClassLibrary2.UI.ViewModel;
using ClassLibrary2.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace ClassLibrary2
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdEtabsToRevit : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            try
            {
                MainView view = new MainView();
                view.DataContext = new MainViewModel();
                view.ShowDialog();

                MainViewModel vm = view.DataContext as MainViewModel;
                MainViewModel vmbeam = view.DataContext as MainViewModel;

                List<LevelData> LevelModelData = vm.LevelDatas;
                List<BeamData> BeamModelData = vmbeam.BeamDatas;

                //tạo hệ thống level
                foreach (LevelData levelData in LevelModelData)
                {
                    LevelModel(commandData, levelData.Name, levelData.Elevation);
                }

                //vẽ dầm kèm set rebarcover
                CreateBeams(doc, BeamModelData);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }

            return Result.Succeeded;
        }


        #region CreateEtabsStory
        public void LevelModel(ExternalCommandData commandData, string uniquename, double elevation)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            var elems = new FilteredElementCollector(doc)
                        .WhereElementIsNotElementType()
                        .OfCategory(BuiltInCategory.OST_Levels);
            string Levelexisted = "false";

            using (var transaction = new Transaction(doc, "Set Elevation"))
            {
                foreach (var singlelevel in elems)
                {
                    Parameter para = singlelevel.LookupParameter("Name");
                    Parameter cote = singlelevel.LookupParameter("Elevation");
                    if (double.TryParse(cote.AsValueString(), out double cotelevel))
                    {
                        if (para.AsString() == uniquename)
                        {
                            transaction.Start();
                            Levelexisted = "true";
                            cote.Set(elevation);
                            transaction.Commit();
                        }
                    }
                }

                if (Levelexisted == "false")
                {
                    transaction.Start();
                    Level newlevel = Level.Create(doc, elevation);
                    Parameter paranew = newlevel.LookupParameter("Name");
                    paranew.Set(uniquename);
                    transaction.Commit();
                }
            }
        }
        #endregion CreateEtabsStory

        #region DrawBeam
        private void CreateBeams(Document doc, List<BeamData> beamDatas)
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
                    foreach (BeamData beamData in beamDatas)
                    {
                        CreateBeam(doc, beamData, levels, beamTypes);
                    }
                    trans.Commit();
                }
            }
        }

        private void CreateBeam(Document doc, BeamData beamData, List<Level> levels, List<FamilySymbol> beamTypes)
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
                SetRebarCover(doc, beamnew,beamData);
            }
        }
        #endregion DrawBeam

        #region SetBeamRebarCover
        private void SetRebarCover(Document doc, FamilyInstance fam, BeamData beam)
        {
            var rebarcover = new FilteredElementCollector(doc)
                .WhereElementIsElementType()
                .OfCategory(BuiltInCategory.OST_CoverType);

            Parameter Bottomcover = fam.LookupParameter("Rebar Cover - Bottom Face");
            string newnamebot = "Etabs rebar botcover value";

            Parameter Topcover = fam.LookupParameter("Rebar Cover - Top Face");
            string newnametop = "Etabs rebar topcover value";

            //Parameter othercover = fam.LookupParameter("Rebar Cover - Other Faces");

            if (rebarcover.FirstOrDefault(x => x.Name.Equals(newnametop)) != null)
            {
                Parameter existlength = rebarcover.FirstOrDefault(x => x.Name.Equals(newnametop)).LookupParameter("Length");
                existlength.Set(beam.TopCover);
                Topcover.Set(rebarcover.FirstOrDefault(x => x.Name.Equals(newnametop)).Id);
            }
            else
            {
                RebarCoverType topp = CreateRebarCover(doc, newnametop, beam.TopCover);
                Topcover.Set(topp.Id);
            }

            if (rebarcover.FirstOrDefault(x => x.Name.Equals(newnamebot)) != null)
            {
                Parameter existlength = rebarcover.FirstOrDefault(x => x.Name.Equals(newnamebot)).LookupParameter("Length");
                existlength.Set(beam.BottomCover);
                Bottomcover.Set(rebarcover.FirstOrDefault(x => x.Name.Equals(newnamebot)).Id);
            }
            else
            {
                RebarCoverType bott = CreateRebarCover(doc, newnamebot, beam.BottomCover);
                Bottomcover.Set(bott.Id);
                //othercover.Set(fifty.Id);
            }
        }

        private RebarCoverType CreateRebarCover(Document doc, string name, double coverDistance)
        {
            RebarCoverType rebarcovernew = RebarCoverType.Create(doc, name, coverDistance);
            return rebarcovernew;
        }

        #endregion SetBeamRebarCover
    }
}