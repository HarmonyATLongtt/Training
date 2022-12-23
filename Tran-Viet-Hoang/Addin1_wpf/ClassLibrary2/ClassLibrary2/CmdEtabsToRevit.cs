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

                List<LevelData> LevelModelData = vm.LevelDatas;
                List<BeamData> BeamModelData = vm.BeamDatas;
                List<ColumnData> ColumnModelData = vm.ColDatas;

                //tạo hệ thống level
                foreach (LevelData levelData in LevelModelData)
                {
                    LevelModel(commandData, levelData.Name, levelData.Elevation);
                }

                //vẽ dầm kèm set rebarcover
                CreateBeams(doc, BeamModelData);
                CreateCols(doc, ColumnModelData, LevelModelData);
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
                SetRebarCover(doc, beamnew, beamData);
               
            }
        }

        #endregion DrawBeam

        #region DrawCol

        private void CreateCols(Document doc, List<ColumnData> ColDatas, List<LevelData> LLevels)
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
                    foreach (ColumnData colData in ColDatas)
                    {
                        CreateCol(doc, colData, LLevels, collevels, colTypes);
                    }
                    trans.Commit();
                }
            }
        }

        private void CreateCol(Document doc, ColumnData colData, List<LevelData> LLevels, List<Level> levels, List<FamilySymbol> colTypes)
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



                ////lấy những param cần cho vẽ thép
                //Parameter elemlength = col.LookupParameter("Length");
                //Parameter col_b = col.Symbol.LookupParameter("b");
                //Parameter col_h = col.Symbol.LookupParameter("h");
                ////khai báo giá trị other cover, để xác định chính xác length của thép
                //double cover = 50 / 304.8;

                //BoundingBoxXYZ boundingbox = columnn.get_BoundingBox(null);
                //XYZ min = boundingbox.Transform.OfPoint(boundingbox.Min);
                //XYZ max = boundingbox.Transform.OfPoint(boundingbox.Max);

                //XYZ origin = new XYZ(boundingbox.Min.X + cover, boundingbox.Min.Y + cover, boundingbox.Min.Z + cover);

                //XYZ yVec = new XYZ(0, 1, 0);
                //XYZ xVec = new XYZ(1, 0, 0);
                //if (Convert.ToDouble(boundingbox.Max.X - boundingbox.Min.X) > Convert.ToDouble(boundingbox.Max.Y - boundingbox.Min.Y))
                //{
                //    origin = new XYZ(boundingbox.Min.X + cover, boundingbox.Min.Y + cover, boundingbox.Max.Z - cover);
                //    xVec = new XYZ(0, 1, 0);
                //    yVec = new XYZ(1, 0, 0);
                //}
                //Rebar rebar = Rebar.CreateFromRebarShape(doc, shape, type, columnn, origin, xVec, yVec);

                //Parameter tie_B = rebar.LookupParameter("B");
                //Parameter tie_C = rebar.LookupParameter("C");
                //Parameter tie_D = rebar.LookupParameter("D");
                //Parameter tie_E = rebar.LookupParameter("E");
                //MessageBox.Show(xVec + "\n" + yVec);

                //double B_D = col_b.AsDouble() - 2 * cover;
                //tie_B.Set(B_D);
                //tie_D.Set(B_D);

                //double C_E = col_h.AsDouble() - 2 * cover;
                //tie_C.Set(C_E);
                //tie_E.Set(C_E);

                
                //BoundingBoxXYZ boundingboxnew = rebar.get_BoundingBox(null);
                //XYZ origin1 = boundingboxnew.Transform.OfPoint(boundingboxnew.Min);
                //XYZ vect = origin - origin1;

                //ElementTransformUtils.MoveElement(doc, rebar.Id, vect);

                //rebar.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(250 / 304.8, elemlength.AsDouble(), true, true, false);
            }
        }

        #endregion DrawCol

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

        #region CreateBeamRebar

        private void CreateBeamRebar(Document doc, BeamData beam, FamilyInstance beamnew)
        {
            RebarShape shape = new FilteredElementCollector(doc)
               .OfClass(typeof(RebarShape))
               .Cast<RebarShape>()
               .First(x => x.Name == "M_00");

            RebarBarType type = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .First(x => x.Name == "19M");

            //Element elem = doc.GetElement(beamnew);
            Parameter elemlength = beamnew.LookupParameter("Length");

            //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y
            Location loc = beamnew.Location;
            LocationCurve locCur = loc as LocationCurve;
            Curve curve = locCur.Curve;
            Line locline = curve as Line;

            //gán vector phương X trong definition của shape thép với vector vẽ cấu kiện
            XYZ yVec = new XYZ(0, 0, 1);
            XYZ xVec = locline.Direction;

            //khai báo giá trị other cover, để xác định chính xác length của thép
            double cover = beam.BottomCover / 304.8;
            double stirrup = 13 / 304.8;
            //lấy giá trị length của cấu kiện
            string elemlengthvalue = elemlength.AsValueString();

            BoundingBoxXYZ boundingbox = beamnew.get_BoundingBox(null);
            XYZ origin1 = XYZ.Zero;

            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                origin1 = new XYZ(boundingbox.Min.X + cover + stirrup, boundingbox.Max.Y - cover - stirrup, boundingbox.Min.Z + cover + stirrup);

            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                origin1 = new XYZ(boundingbox.Min.X + cover + stirrup, boundingbox.Min.Y + cover + stirrup, boundingbox.Min.Z + cover + stirrup);

            }
            XYZ min = boundingbox.Transform.OfPoint(boundingbox.Min);
            XYZ max = boundingbox.Transform.OfPoint(boundingbox.Max);






            MessageBox.Show("vec = " + xVec.Y);
            try
            {
                using (var transaction = new Transaction(doc, "Create rebar "))
                {
                    transaction.Start();

                    //nếu Bounding box ngược chiều với hướng vẽ element thì thép sẽ bị bay ra ngoài, vậy nên kiểm tra nếu ngược chiều thì gán chiều vẽ thép bằng ngược chiều của chiều vẽ dầm
                    if (Math.Round(xVec.X, 4) < 0 || Math.Round(xVec.Y, 4) < 0)
                    {
                        //dù giá trị vector có thể là (1.0000; 0,00000; 0,00000) nhưng riêng giá trị Y có thể là giá trị : -3.0723465E-15 - giá trị rất gần với 0 nhưng vẫn là < 0)
                        // nên phải làm tròn số thập phân của X hoặc Y của vector để lệnh if kiểm tra được như ý muốn
                        xVec = -locline.Direction;
                    }
                    Rebar rebar = Rebar.CreateFromRebarShape(doc, shape, type, beamnew, origin1, xVec, yVec);


                    Parameter rebarlength = rebar.LookupParameter("B");
                    double oldlength = rebarlength.AsDouble(); //giữ lại giá trị length ban đầu để sau thực hiện rotate


                    XYZ vect = new XYZ(cover + stirrup, cover + stirrup, cover + stirrup);
                    XYZ point1 = XYZ.Zero;

                    //kiểm tra xem cấu kiện được vẽ theo phương nào để có thể lấy được trục để rotate
                    if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
                    {
                        point1 = origin1 + XYZ.BasisX * oldlength / 2; ;
                    }
                    else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
                    {
                        point1 = origin1 + XYZ.BasisY * oldlength / 2; ;
                    }

                    XYZ point2 = point1 + XYZ.BasisZ * 100;
                    Line axis = Line.CreateBound(point1, point2);

                    // set giá trị mới cho length cuả rebar
                    rebarlength.Set(Convert.ToDouble(elemlengthvalue) / 304.8 - 2 * cover);

                    ElementTransformUtils.RotateElement(doc, rebar.Id, axis, Math.PI);

                    rebar.GetShapeDrivenAccessor().SetLayoutAsNumberWithSpacing(3, 75 / 304.8, false, true, true);


                    transaction.Commit();
                }
            }

            catch (Exception ex)
            {
                
                string message = ex.Message;
            }
        }   
    

        #endregion CreateBeamRebar
    }
}