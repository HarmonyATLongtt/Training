using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolutionRevitAPI.Commands.ExternalEventHandler
{
    public class CreatNewInstanceEEH : IExternalEventHandler
    {
        public Level SelectedLevel { get; set; }
        public Category SelectedCategory { get; set; }
        public Object SelectedFamilySymbol { get; set; }
        public int Mode { get; set; }

        public List<Curve> Curves { get; set; }
        public List<XYZ> Points { get; set; }

        public void Execute(UIApplication app)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Mode == 1)
            {
                Points.Clear();
                Curves.Clear();
                try
                {
                    while ((Points.Count() < 2 || SelectedCategory.BuiltInCategory == BuiltInCategory.OST_Floors))
                    {
                        // Yêu cầu người dùng chọn một điểm
                        XYZ point = uidoc.Selection.PickPoint("Chọn một điểm");
                        // Thêm điểm vào danh sách
                        Points.Add(point);

                        // Hỏi người dùng nếu họ muốn tiếp tục chọn điểm
                        if ((Points.Count() < 2 || SelectedCategory.BuiltInCategory == BuiltInCategory.OST_Floors))
                        {
                            TaskDialogResult result = TaskDialog.Show("Tiếp tục", "Bạn có muốn chọn thêm điểm không?", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);
                            if (result == TaskDialogResult.No)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    Points.Clear();
                    return;
                }
            }
            if (Mode == 2)
            {
                Points.Clear();
                Curves.Clear();
                try
                {
                    // Sử dụng PickObjects để chọn nhiều ModelLine
                    SelectionFilter LineSelection = new SelectionFilter(BuiltInCategory.OST_Lines);
                    IList<Reference> pickedRefs = uidoc.Selection.PickObjects(ObjectType.Element, LineSelection, "Chọn các modelLine để tạo đối tượng");

                    // Lấy các model line đã chọn
                    Curves.Clear();
                    foreach (Reference pickedRef in pickedRefs)
                    {
                        Element element = doc.GetElement(pickedRef);
                        if (element is ModelLine modelLine)
                        {
                            if (modelLine.Location is LocationCurve locCurve)
                            {
                                Curves.Add(locCurve.Curve);
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Curves.Clear(); //Tạo cột nghiêng
                    return;
                }
            }
            if (Mode == 3)
            {
                using (Transaction trans = new Transaction(doc, $"Creat New {SelectedCategory.Name}"))
                {
                    trans.Start();
                    Category category = SelectedCategory;
                    if (Points.Count > 0)
                    {
                        switch (category.BuiltInCategory)
                        {
                            case BuiltInCategory.OST_Walls: //Tạo tường
                                if (Points.Count() < 2)
                                {
                                    TaskDialog.Show("Warning", "Không thể tạo tường từ 1 điểm, vui lòng chọn lại!");
                                }
                                else
                                {
                                    // Xác định các điểm để tạo tường
                                    XYZ startPoint = Points[0]; // Điểm bắt đầu
                                    XYZ endPoint = Points[1];   // Điểm kết thúc

                                    // Tạo một Line từ điểm bắt đầu và kết thúc
                                    Line wallLine = Line.CreateBound(startPoint, endPoint);

                                    if (SelectedFamilySymbol is WallType wallType)
                                    {
                                        Wall.Create(doc, wallLine, wallType.Id, SelectedLevel.Id, UnitUtils.ConvertToInternalUnits(4.0, UnitTypeId.Meters), 0.0, false, false);
                                    }
                                }
                                break;

                            case BuiltInCategory.OST_Roofs:
                                if (Curves.Count() < 3)
                                {
                                    TaskDialog.Show("Warning", "Không thể tạo tường ít hơn 3 đường, vui lòng chọn lại!");
                                }
                                else
                                {
                                    CurveArray footprint = new CurveArray();

                                    foreach (Curve curve in Curves)
                                    {
                                        footprint.Append(curve);
                                    }
                                    //if (SelectedFamilySymbol is RoofType roofStyle)
                                    //{
                                    //    ModelCurveArray footPrintToModelCurveMapping = new ModelCurveArray();
                                    //    FootPrintRoof footprintRoof = doc.Create.NewFootPrintRoof(footprint, SelectedLevel, roofStyle, out footPrintToModelCurveMapping);
                                    //}
                                }
                                break;

                            case BuiltInCategory.OST_Floors:
                                if (Points.Count() < 3)
                                {
                                    TaskDialog.Show("Warning", "Không thể tạo tường ít hơn 3 điểm, vui lòng chọn lại!");
                                }
                                else
                                {
                                    CurveLoop profile = new CurveLoop();
                                    for (int i = 0; i < Points.Count() - 1; i++)
                                    {
                                        profile.Append(Line.CreateBound(Points[i], Points[i + 1]));
                                    }
                                    profile.Append(Line.CreateBound(Points.Last(), Points.First()));
                                    if (SelectedFamilySymbol is FloorType floorStyle)
                                    {
                                        Floor.Create(doc, new List<CurveLoop> { profile }, floorStyle.Id, SelectedLevel.Id);
                                    }
                                }
                                break;

                            case BuiltInCategory.OST_StructuralFraming:
                                if (Points.Count() < 2)
                                {
                                    TaskDialog.Show("Warning", "Không thể tạo dầm từ 1 điểm, vui lòng chọn lại!");
                                }
                                else
                                {
                                    var beamSymbol = SelectedFamilySymbol as FamilySymbol;
                                    if (!beamSymbol.IsActive)
                                    {
                                        beamSymbol.Activate();
                                    }
                                    // Xác định các điểm để tạo tường
                                    XYZ startPoint = Points[0]; // Điểm bắt đầu
                                    XYZ endPoint = Points[1];   // Điểm kết thúc

                                    // Tạo một Line từ điểm bắt đầu và kết thúc
                                    Line beamlLine = Line.CreateBound(startPoint, endPoint);

                                    doc.Create.NewFamilyInstance(beamlLine, beamSymbol, SelectedLevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                }
                                break;

                            case BuiltInCategory.OST_StructuralColumns:
                                var familySymbol = SelectedFamilySymbol as FamilySymbol;
                                if (!familySymbol.IsActive)
                                {
                                    familySymbol.Activate();
                                }

                                if (Points.Count() < 2)
                                {
                                    doc.Create.NewFamilyInstance(Points[0], familySymbol, SelectedLevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                                }
                                else
                                {
                                    //Tạo cột nghiêng

                                    XYZ startPoint = Points[0]; // Điểm bắt đầu
                                    XYZ endPoint = Points[1];   // Điểm kết thúc

                                    // Tạo một Line từ điểm bắt đầu và kết thúc
                                    Line line = Line.CreateBound(startPoint, endPoint);
                                    FamilyInstance columnInstance = doc.Create.NewFamilyInstance(line, familySymbol, SelectedLevel, Autodesk.Revit.DB.Structure.StructuralType.Column);

                                    double offset = 0.0;
                                    Parameter topOffsetPara = columnInstance.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM);
                                    topOffsetPara.Set(UnitUtils.ConvertToInternalUnits(offset, topOffsetPara.GetUnitTypeId()));
                                    Parameter baseOffsetPara = columnInstance.get_Parameter(BuiltInParameter.SCHEDULE_BASE_LEVEL_OFFSET_PARAM);
                                    baseOffsetPara.Set(UnitUtils.ConvertToInternalUnits(offset, topOffsetPara.GetUnitTypeId()));
                                }
                                break;

                            default:
                                TaskDialog.Show("Warning", "Danh mục đã chọn không được hỗ trợ.");
                                break;
                        }
                    }
                    else
                    {
                        switch (category.BuiltInCategory)
                        {
                            case BuiltInCategory.OST_Walls: //Tạo tường

                                if (SelectedFamilySymbol is WallType wallType)
                                {
                                    for (int i = 0; i < Curves.Count; i++)
                                    {
                                        Line line = (Line)Curves[i];
                                        Wall.Create(doc, line, wallType.Id, SelectedLevel.Id, UnitUtils.ConvertToInternalUnits(4.0, UnitTypeId.Meters), 0.0, false, false);
                                    }
                                }

                                break;

                            case BuiltInCategory.OST_Roofs:
                                if (Curves.Count() < 3)
                                {
                                    TaskDialog.Show("Warning", "Không thể tạo sàn từ ít hơn 3 đường, vui lòng chọn lại!");
                                }
                                else
                                {
                                    CurveArray footprint = new CurveArray();

                                    foreach (Curve curve in Curves)
                                    {
                                        footprint.Append(curve);
                                    }
                                    //if (SelectedFamilySymbol is RoofType roofStyle)
                                    //{
                                    //    ModelCurveArray footPrintToModelCurveMapping = new ModelCurveArray();
                                    //    FootPrintRoof footprintRoof = doc.Create.NewFootPrintRoof(footprint, SelectedLevel, roofStyle, out footPrintToModelCurveMapping);
                                    //}
                                }
                                break;

                            case BuiltInCategory.OST_Floors:

                                if (Curves.Count() < 3)
                                {
                                    TaskDialog.Show("Warning", "Không thể tạo sàn ít hơn 3 đường, vui lòng chọn lại!");
                                }
                                else
                                {
                                    List<Curve> closedLoop = ArrangeCurvesToClosedLoop(Curves);
                                    if (closedLoop == null)
                                    {
                                        TaskDialog.Show("Error", "Không thể sắp xếp các đường thành Boundary Line khép kín.");
                                    }
                                    CurveLoop profile = new CurveLoop();
                                    foreach (Curve curve in closedLoop)
                                    {
                                        profile.Append(curve);
                                    }

                                    if (SelectedFamilySymbol is FloorType floorStyle)
                                    {
                                        Floor.Create(doc, new List<CurveLoop> { profile }, floorStyle.Id, SelectedLevel.Id);
                                    }
                                }
                                break;

                            case BuiltInCategory.OST_StructuralFraming:

                                var beamSymbol = SelectedFamilySymbol as FamilySymbol;
                                if (!beamSymbol.IsActive)
                                {
                                    beamSymbol.Activate();
                                }
                                if (beamSymbol != null)
                                {
                                    for (int i = 0; i < Curves.Count; i++)
                                    {
                                        Line beamLine = (Line)Curves[i];

                                        doc.Create.NewFamilyInstance(beamLine, beamSymbol, SelectedLevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                    }
                                }
                                break;

                            case BuiltInCategory.OST_StructuralColumns:
                                var columnSymbol = SelectedFamilySymbol as FamilySymbol;
                                if (!columnSymbol.IsActive)
                                {
                                    columnSymbol.Activate();
                                }
                                //Tạo cột nghiêng
                                if (columnSymbol != null)
                                {
                                    for (int i = 0; i < Curves.Count; i++)
                                    {
                                        Line columnLine = (Line)Curves[i];

                                        FamilyInstance columnInstance = doc.Create.NewFamilyInstance(columnLine, columnSymbol, SelectedLevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                                        double offset = 0.0;
                                        Parameter topOffsetPara = columnInstance.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM);
                                        topOffsetPara.Set(UnitUtils.ConvertToInternalUnits(offset, topOffsetPara.GetUnitTypeId()));
                                        Parameter baseOffsetPara = columnInstance.get_Parameter(BuiltInParameter.SCHEDULE_BASE_LEVEL_OFFSET_PARAM);
                                        baseOffsetPara.Set(UnitUtils.ConvertToInternalUnits(offset, topOffsetPara.GetUnitTypeId()));
                                    }
                                }
                                break;

                            default:
                                TaskDialog.Show("Warning", "Danh mục đã chọn không được hỗ trợ.");
                                break;
                        }
                    }
                    trans.Commit();
                }
                Points.Clear();
                Curves.Clear();
            }
        }
        #region Hàm xử lý kiểm tra xem có tạo thành Boundary Line khép kín không
        private List<Curve> ArrangeCurvesToClosedLoop(List<Curve> curves)
        {
            // Hàm này sắp xếp các đường để tạo thành vòng tròn kín
            // Trả về null nếu không thể sắp xếp được
            List<Curve> closedLoop = new List<Curve>();

            Curve currentCurve = curves[0];
            closedLoop.Add(currentCurve);
            curves.RemoveAt(0);

            while (curves.Count > 0)
            {
                bool foundNext = false;
                for (int i = 0; i < curves.Count; i++)
                {
                    Curve nextCurve = curves[i];
                    if (AreCurvesConnected(currentCurve, nextCurve))
                    {
                        closedLoop.Add(nextCurve);
                        curves.RemoveAt(i);
                        currentCurve = nextCurve;
                        foundNext = true;
                        break;
                    }
                }
                if (!foundNext)
                {
                    return null;
                }
            }
            // Kiểm tra xem điểm cuối của vòng tròn có khớp với điểm đầu không
            if (!ArePointsEqual(closedLoop.First().GetEndPoint(0), closedLoop.Last().GetEndPoint(1)))
            {
                return null;
            }

            return closedLoop;
        }

        private bool AreCurvesConnected(Curve curve1, Curve curve2)
        {
            return ArePointsEqual(curve1.GetEndPoint(1), curve2.GetEndPoint(0)) || ArePointsEqual(curve1.GetEndPoint(1), curve2.GetEndPoint(1));
        }

        private bool ArePointsEqual(XYZ point1, XYZ point2)
        {
            return point1.IsAlmostEqualTo(point2);
        }
        #endregion
        public string GetName()
        {
            return "Creat New Instance";
        }
    }
}