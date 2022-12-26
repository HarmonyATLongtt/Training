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
using System.Windows.Controls;
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
                var tablebeamobject = vm.Tables;
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

                //vẽ 1 stirrup ban đầu cho cột và dầm đồng thời set lại giá trị cho stirrup đó để phù hợp với kích thước cấu kiện
                drawcolstirrup(doc, ColumnModelData);
                drawbeamstirrup(doc, BeamModelData);

                //sau khi set giá trị mới cho stirrup thì move stirrup về nằm gọn trong cấu kiện
                MoveStirrup(doc, ColumnModelData, BeamModelData);
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
                Parameter colcmt = beamnew.LookupParameter("Comments");
                colcmt.Set("Etabs" + beamData.Name);
            }
        }

        #endregion DrawBeam

        #region SetBeamStirrup
        // hàm tạo list trả về các giá trị kiểu Rebar, giá trị length, b, h và boundingbox của host beam
        public List<RebarSetData> BeamStirrup(Document doc, List<FamilyInstance> beams)
        {
            List<RebarSetData> rebars = new List<RebarSetData>();
            string style = "Stirrup / Tie";
            foreach (var beam in beams)
            {
                var stirrup = new FilteredElementCollector(doc)
                  .WhereElementIsNotElementType()
                  .OfCategory(BuiltInCategory.OST_Rebar)
                  .Cast<Rebar>()
                  .First(x => x.LookupParameter("Style").AsValueString() == style && x.GetHostId() == beam.Id);
                if (stirrup != null)
                {
                    RebarSetData rebar = new RebarSetData();
                    rebar.BeamStirrup = stirrup;
                    rebar.HostLength = beam.LookupParameter("Length").AsDouble();
                    rebar.Host_h = beam.Symbol.LookupParameter("h").AsDouble();
                    rebar.Host_b = beam.Symbol.LookupParameter("b").AsDouble();
                    rebar.BeamStirrupOrigin = BeamStirrupOrigin(beam,50/304.8);
                    rebars.Add(rebar);
                }
            }
            return rebars;
        }

        // hàm tạo 1 stirrup cho nhiều cột và set lại giá trị stirrup đó sao cho phù hợp với kích thước cột
        public void drawbeamstirrup(Document doc, List<BeamData> beams)
        {
            List<FamilyInstance> beametabselems = BeamIDs(doc, beams);
            double cover = 50 / 304.8;
            RebarShape shape = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarShape))
                .Cast<RebarShape>()
                .First(x => x.Name == "M_T1");

            RebarBarType type = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .First(x => x.Name == "8M");

            using (Transaction trans = new Transaction(doc, "create col stirrup"))
            {
                trans.Start();
                foreach (var beametabs in beametabselems)
                {
                    Rebar barnew = rebarbeambefore(beametabs, doc, shape, type);
                    Parameter tie_B = barnew.LookupParameter("B");
                    Parameter tie_C = barnew.LookupParameter("C");
                    Parameter tie_D = barnew.LookupParameter("D");
                    Parameter tie_E = barnew.LookupParameter("E");

                    double B_D = beametabs.Symbol.LookupParameter("b").AsDouble() - 2 * cover;
                    tie_B.Set(B_D);
                    tie_D.Set(B_D);

                    double C_E = beametabs.Symbol.LookupParameter("h").AsDouble() - 2 * cover;
                    tie_C.Set(C_E);
                    tie_E.Set(C_E);
                }
                trans.Commit();
            }
        }

        //Tạo list trả về dữ liệu kiểu FamilyInstance là các Cột vừa được vẽ
        public List<FamilyInstance> BeamIDs(Document doc, List<BeamData> beams)
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

        //Hàm tạo 1 stirrup ban đầu cho 1 dầm
        public Rebar rebarbeambefore(FamilyInstance beametabs, Document doc, RebarShape shape, RebarBarType type)
        {
            //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y
            Location loc = beametabs.Location;
            LocationCurve locCur = loc as LocationCurve;
            Curve curve = locCur.Curve;
            Line locline = curve as Line;
            XYZ xVec = locline.Direction; // để lấy được chiều vẽ của dầm
            //khai báo giá trị other cover, để xác định chính xác length của thép
            double cover = 50 / 304.8;

            BoundingBoxXYZ boundingbox = beametabs.get_BoundingBox(null);

            XYZ yVec = new XYZ(0, 0, 1);
            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương X, thì phương X của family thép đai sẽ map vào phương Y
                xVec = new XYZ(0, 1, 0);
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương Y, thì phương X của family thép đai sẽ map vào phương X
                xVec = new XYZ(1, 0, 0);
            }
            XYZ origin = BeamStirrupOrigin(beametabs, cover);
            Rebar rebar = Rebar.CreateFromRebarShape(doc, shape, type, beametabs, origin, xVec, yVec);
            return rebar;
        }

        // Hàm lấy origin ban đầu về cho stirrup của dầm
        public XYZ BeamStirrupOrigin(FamilyInstance beam, double cover)
        {
            //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y
            Location loc = beam.Location;
            LocationCurve locCur = loc as LocationCurve;
            Curve curve = locCur.Curve;
            Line locline = curve as Line;
            XYZ xVec = locline.Direction; // để lấy được chiều vẽ của dầm

            BoundingBoxXYZ boundingbox = beam.get_BoundingBox(null);

            XYZ origin = XYZ.Zero;

            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương X, thì phương X của family thép đai sẽ map vào phương Y
                origin = new XYZ(boundingbox.Min.X + cover, boundingbox.Min.Y + cover, boundingbox.Min.Z + cover);
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương Y, thì phương X của family thép đai sẽ map vào phương X
                origin = new XYZ(boundingbox.Min.X + cover, boundingbox.Max.Y - cover, boundingbox.Min.Z + cover);
            }
            return origin;
        }
        #endregion SetBeamStirrup

        //#region SetBeamStandardBar
        ////hàm trả về điểm origin để vẽ thép dọc cho dầm
        //public void SetBeamStandard(Document doc, FamilyInstance elem,BeamData beam) 
        //{
        //    double stirrup = 8 / 304.8;
        //    double cover = 50 / 304.8;
        //    Parameter elemlength = elem.LookupParameter("Length");
        //    Parameter elemb = elem.Symbol.LookupParameter("b");
        //    Parameter elemh = elem.Symbol.LookupParameter("h");
        //    RebarSetData designrebar = RebarBeamCaculation(elem, cover, stirrup, beam.AsBottomLongitudinal);
        //    string rebartype = designrebar.Type.ToString() + "M";

        //    RebarShape shape = new FilteredElementCollector(doc)
        //        .OfClass(typeof(RebarShape))
        //        .Cast<RebarShape>()
        //        .First(x => x.Name == "M_00");

        //    RebarBarType type = new FilteredElementCollector(doc)
        //        .OfClass(typeof(RebarBarType))
        //        .Cast<RebarBarType>()
        //        .First(x => x.Name == rebartype);
        //    try
        //    {
        //        using (var transaction = new Transaction(doc, "Create rebar "))
        //        {
        //            transaction.Start();

        //            //nếu Bounding box ngược chiều với hướng vẽ element thì thép sẽ bị bay ra ngoài, vậy nên kiểm tra nếu ngược chiều thì gán chiều vẽ thép bằng ngược chiều của chiều vẽ dầm
        //            if (Math.Round(xVec.X, 4) < 0 || Math.Round(xVec.Y, 4) < 0)
        //            {
        //                //dù giá trị vector có thể là (1.0000; 0,00000; 0,00000) nhưng riêng giá trị Y có thể là giá trị : -3.0723465E-15 - giá trị rất gần với 0 nhưng vẫn là < 0)
        //                // nên phải làm tròn số thập phân của X hoặc Y của vector để lệnh if kiểm tra được như ý muốn
        //                xVec = -locline.Direction;
        //            }
        //            Rebar rebar = Rebar.CreateFromRebarShape(doc, shape, type, elem, origin1, xVec, yVec);

        //            Parameter rebarlength = rebar.LookupParameter("B");
        //            double oldlength = rebarlength.AsDouble(); //giữ lại giá trị length ban đầu để sau thực hiện rotate

        //            XYZ point1 = XYZ.Zero;

        //            //kiểm tra xem cấu kiện được vẽ theo phương nào để có thể lấy được trục để rotate
        //            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
        //            {
        //                point1 = origin1 + XYZ.BasisX * oldlength / 2; ;
        //            }
        //            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
        //            {
        //                point1 = origin1 + XYZ.BasisY * oldlength / 2; ;
        //            }

        //            XYZ point2 = point1 + XYZ.BasisZ * 100;
        //            Line axis = Line.CreateBound(point1, point2);

        //            // set giá trị mới cho length cuả rebar
        //            rebarlength.Set(Convert.ToDouble(elemlengthvalue) / 304.8 - 2 * cover);

        //            ElementTransformUtils.RotateElement(doc, rebar.Id, axis, Math.PI);

        //            rebar.GetShapeDrivenAccessor().SetLayoutAsNumberWithSpacing(designrebar.Number, designrebar.Spacing / 304.8, false, true, true);

        //            transaction.Commit();
        //        }
        //        return Result.Succeeded;
        //    }
        //    catch (Exception ex)
        //    {
        //        message = ex.Message;
        //        return Result.Failed;
        //    }
        //}
        //public XYZ xVecBeam(FamilyInstance elem)
        //{
        //    //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y
        //    Location loc = elem.Location;
        //    LocationCurve locCur = loc as LocationCurve;
        //    Curve curve = locCur.Curve;
        //    Line locline = curve as Line;

        //    //gán vector phương X trong definition của shape thép với vector vẽ cấu kiện
        //    XYZ xVec = locline.Direction;
        //}
        //public XYZ BeamStandardOrigin(FamilyInstance elem, double cover, double stirrup)
        //{
           

        //    BoundingBoxXYZ boundingbox = elem.get_BoundingBox(null);
        //    XYZ origin1 = XYZ.Zero;

        //    if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
        //    {
        //        origin1 = new XYZ(boundingbox.Min.X + cover + stirrup, boundingbox.Max.Y - cover - stirrup, boundingbox.Min.Z + cover + stirrup);
        //    }
        //    else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
        //    {
        //        origin1 = new XYZ(boundingbox.Min.X + cover + stirrup, boundingbox.Min.Y + cover + stirrup, boundingbox.Min.Z + cover + stirrup);
        //    }

        //    return origin1;
        //}
        ////hàm tính lượng thép cần bố trí
        //public RebarSetData RebarBeamCaculation(FamilyInstance beam, double cover, double stirrup, double Astinhtoan)
        //{
        //    double kc = 25; //khoảng cách thông thủy tối thiểu giữa các thanh thép lớp dưới
        //    int[] duongkinhcautao = { 22, 20, 18, 16 };
        //    int[] sothanh = new int[4];

        //    Parameter elemb = beam.Symbol.LookupParameter("b");
        //    Parameter elemh = beam.Symbol.LookupParameter("h");
        //    double Asmin = elemb.AsDouble() * 304.8 * elemh.AsDouble() * 304.8 * 0.05 / 100; // diện tích cốt thép tối thiểu là 0,05%
        //    if (Asmin < Astinhtoan) { Asmin = Astinhtoan; } // chọn ra giá trị mà As thiết kế bắt buộc sẽ phải lớn hơn

        //    RebarSetData rebarsets = new RebarSetData();
        //    for (int i = 0; i < duongkinhcautao.Count(); i++)
        //    {
        //        //số thanh phải nhỏ hơn hoặc bằng, nên dùng hàm Floor để lấy giá trị nguyên lớn nhất và gần kết quả nhất
        //        sothanh[i] = Convert.ToInt32(Math.Floor((elemb.AsDouble() * 304.8 + kc - 2 * (cover + stirrup) * 304.8) / (duongkinhcautao[i] + kc)));
        //    }
        //    for (int i = 0; i < sothanh.Count(); i++)
        //    {
        //        if (Math.Pow(duongkinhcautao[i], 2) * Math.PI / 4 * sothanh[i] >= Asmin)
        //        {
        //            RebarSetData rebarset = new RebarSetData();
        //            rebarset.Number = sothanh[i];
        //            rebarset.Type = duongkinhcautao[i];
        //            rebarset.RebarCrossSectionArea = Math.Pow(duongkinhcautao[i], 2) * Math.PI / 4 * sothanh[i];
        //            rebarset.CrossSectionWidth = elemb.AsDouble();
        //            rebarset.Spacing = ((rebarset.CrossSectionWidth - 2 * (cover + stirrup)) * 304.8 - rebarset.Type) / (rebarset.Number - 1);
        //            rebarsets = rebarset;
        //            break;
        //        }

        //    }
        //    return rebarsets;
        //}
        //#endregion SetBeamStandardBar

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
                Parameter colcmt = col.LookupParameter("Comments");
                colcmt.Set("Etabs" + colData.Name);

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

        #endregion DrawCol

        #region SetColumnStirrup

        // hàm tạo 1 stirrup cho nhiều cột và set lại giá trị stirrup đó sao cho phù hợp với kích thước cột
        public void drawcolstirrup(Document doc, List<ColumnData> cols)
        {
            List<FamilyInstance> coletabselems = ColumnIDs(doc, cols);
            double cover = 50 / 304.8;
            RebarShape shape = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarShape))
                .Cast<RebarShape>()
                .First(x => x.Name == "M_T1");

            RebarBarType type = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .First(x => x.Name == "8M");

            using (Transaction trans = new Transaction(doc, "create col stirrup"))
            {
                trans.Start();
                foreach (var coletabs in coletabselems)
                {
                    Rebar barnew = rebarcolumnbefore(coletabs, doc, shape, type);
                    Parameter tie_B = barnew.LookupParameter("B");
                    Parameter tie_C = barnew.LookupParameter("C");
                    Parameter tie_D = barnew.LookupParameter("D");
                    Parameter tie_E = barnew.LookupParameter("E");

                    double B_D = coletabs.Symbol.LookupParameter("b").AsDouble() - 2 * cover;
                    tie_B.Set(B_D);
                    tie_D.Set(B_D);

                    double C_E = coletabs.Symbol.LookupParameter("h").AsDouble() - 2 * cover;
                    tie_C.Set(C_E);
                    tie_E.Set(C_E);
                }
                trans.Commit();
            }
        }

       

        //Hàm tạo 1 stirrup ban đầu cho 1 cột
        public Rebar rebarcolumnbefore(FamilyInstance coletabs, Document doc, RebarShape shape, RebarBarType type)
        {
            double cover = 50 / 304.8;

            BoundingBoxXYZ boundingbox = coletabs.get_BoundingBox(null);

            XYZ origin = new XYZ(boundingbox.Min.X + cover, boundingbox.Min.Y + cover, boundingbox.Min.Z + cover);

            XYZ yVec = new XYZ(0, 1, 0);
            XYZ xVec = new XYZ(1, 0, 0);
            if (Convert.ToDouble(boundingbox.Max.X - boundingbox.Min.X) > Convert.ToDouble(boundingbox.Max.Y - boundingbox.Min.Y))
            {
                origin = new XYZ(boundingbox.Min.X + cover, boundingbox.Min.Y + cover, boundingbox.Max.Z - cover);
                xVec = new XYZ(0, 1, 0);
                yVec = new XYZ(1, 0, 0);
            }
            Rebar rebar = Rebar.CreateFromRebarShape(doc, shape, type, coletabs, origin, xVec, yVec);
            return rebar;
        }

        //Tạo list trả về dữ liệu kiểu FamilyInstance là các Cột vừa được vẽ
        public List<FamilyInstance> ColumnIDs(Document doc, List<ColumnData> cols)
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

        // hàm tạo list trả về các giá trị kiểu Rebar, giá trị length, b, h và boundingbox của host column
        public List<RebarSetData> ColumnStirrup(Document doc, List<FamilyInstance> cols)
        {
            List<RebarSetData> rebars = new List<RebarSetData>();
            string style = "Stirrup / Tie";
            foreach (var col in cols)
            {
                var stirrup = new FilteredElementCollector(doc)
                  .WhereElementIsNotElementType()
                  .OfCategory(BuiltInCategory.OST_Rebar)
                  .Cast<Rebar>()
                  .First(x => x.LookupParameter("Style").AsValueString() == style && x.GetHostId() == col.Id);
                if (stirrup != null)
                {
                    RebarSetData rebar = new RebarSetData();
                    rebar.ColumnStirrup = stirrup;
                    rebar.HostLength = col.LookupParameter("Length").AsDouble();
                    rebar.Host_h = col.Symbol.LookupParameter("h").AsDouble();
                    rebar.Host_b = col.Symbol.LookupParameter("b").AsDouble();
                    rebar.Host_boundingbox_1 = col.get_BoundingBox(null);
                    rebars.Add(rebar);
                }
            }
            return rebars;
        }

        #endregion SetColumnStirrup

        #region Move Stirrup
        //hàm di chuyển các stirrup vừa set lại giá trị, về đúng vị trí nằm trong cột đồng thời rải stirrup đi hết cấu kiện
        public void MoveStirrup(Document doc, List<ColumnData> columns, List<BeamData> beams)
        {
            List<FamilyInstance> cols = ColumnIDs(doc, columns);
            List<FamilyInstance> framings = BeamIDs(doc, beams);

            List<RebarSetData> colstirrups = ColumnStirrup(doc, cols);
            List<RebarSetData> beamstirrups = BeamStirrup(doc, framings);

            double cover = 50 / 304.8;
            using (Transaction trans = new Transaction(doc, "create stirrup"))
            {
                trans.Start();
                //cột
                foreach (var stirrup in colstirrups)
                {
                    BoundingBoxXYZ boundingbox = stirrup.Host_boundingbox_1;
                    XYZ min = boundingbox.Transform.OfPoint(boundingbox.Min);
                    XYZ origin = min + XYZ.BasisX * cover + XYZ.BasisY * cover + XYZ.BasisZ * cover;

                    BoundingBoxXYZ boundingboxnew = stirrup.ColumnStirrup.get_BoundingBox(null);
                    XYZ origin1 = boundingboxnew.Transform.OfPoint(boundingboxnew.Min);
                    XYZ vect = origin - origin1;

                    ElementTransformUtils.MoveElement(doc, stirrup.ColumnStirrup.Id, vect);
                    stirrup.ColumnStirrup.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(250 / 304.8, stirrup.HostLength, true, true, false);
                }
                //dầm
                foreach (var stirrup in beamstirrups)
                {
                    XYZ origin = stirrup.BeamStirrupOrigin;

                    BoundingBoxXYZ boundingboxnew = stirrup.BeamStirrup.get_BoundingBox(null);
                    XYZ origin1 = boundingboxnew.Transform.OfPoint(boundingboxnew.Min);
                    XYZ vect = origin - origin1;

                    ElementTransformUtils.MoveElement(doc, stirrup.BeamStirrup.Id, vect);
                    stirrup.BeamStirrup.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(250 / 304.8, stirrup.HostLength, true, true, false);
                }
                trans.Commit();
            }
        }
        #endregion Move Stirrup

       
    }
}